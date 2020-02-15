using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RSocket.Core.Extensions;

namespace RSocket.Core.Transports
{
    public partial class WebSocketTransport
    {
        protected class WebSocketsTransport
        {
            public static readonly WebSocketOptions DefaultWebSocketOptions;

            private readonly IDuplexPipe pipe;
            private readonly WebSocketOptions options;
            private readonly ILogger logger;
            private bool aborted;
            private readonly CancellationTokenSource cancellation;
            private readonly TransferFormat transferFormat;
            private readonly TimeSpan closeTimeout = TimeSpan.FromMinutes(2.0d);

            public WebSocketsTransport(IDuplexPipe pipe, WebSocketOptions options, ILogger logger)
            {
                this.pipe = pipe;
                this.options = options;
                this.logger = logger;
                cancellation = new CancellationTokenSource();
                transferFormat = TransferFormat.Binary;
            }

            static WebSocketsTransport()
            {
                DefaultWebSocketOptions = new WebSocketOptions(
                    TimeSpan.FromSeconds(5.0d),
                    protocols => null
                );
            }

            public async ValueTask ProcessSocketAsync(WebSocket socket)
            {
                // Begin sending and receiving. Receiving must be started first because ExecuteAsync enables SendAsync.
                var receiving = StartReceiving(socket);
                var sending = StartSendingAsync(socket);

                // Wait for send or receive to complete
                var trigger = await Task.WhenAny(receiving, sending);

                if (trigger == receiving)
                {
                    logger.WaitingForSend();

                    // We're waiting for the transport to finish and there are 2 things it could be doing
                    // 1. Waiting for application data
                    // 2. Waiting for a websocket send to complete

                    // Cancel the application so that ReadAsync yields
                    pipe.Input.CancelPendingRead();

                    using (var cts = new CancellationTokenSource())
                    {
                        var resultTask = await Task.WhenAny(sending, Task.Delay(closeTimeout, cts.Token));

                        if (resultTask != sending)
                        {
                            // We timed out so now we're in ungraceful shutdown mode
                            logger.CloseTimedOut(socket);

                            // Abort the websocket if we're stuck in a pending send to the client
                            aborted = true;

                            socket.Abort();
                        }
                        else
                        {
                            cts.Cancel();
                        }
                    }
                }
                else
                {
                    logger.WaitingForClose(socket);

                    // We're waiting on the websocket to close and there are 2 things it could be doing
                    // 1. Waiting for websocket data
                    // 2. Waiting on a flush to complete (backpressure being applied)

                    using (var cts = new CancellationTokenSource())
                    {
                        var resultTask = await Task.WhenAny(receiving, Task.Delay(closeTimeout, cts.Token));

                        if (resultTask != receiving)
                        {
                            // We timed out so now we're in ungraceful shutdown mode
                            logger.CloseTimedOut(socket);

                            // Abort the websocket if we're stuck in a pending receive from the client
                            aborted = true;

                            socket.Abort();

                            // Cancel any pending flush so that we can quit
                            pipe.Output.CancelPendingFlush();
                        }
                        else
                        {
                            cts.Cancel();
                        }
                    }
                }
            }

            private async Task StartReceiving(WebSocket socket)
            {
                var cancellationToken = cancellation.Token;

                try
                {
                    var bytes = new byte[4096];

                    while (false == cancellationToken.IsCancellationRequested)
                    {
                        // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                        //var result = await socket.ReceiveAsync(Memory<byte>.Empty, cancellationToken);
                        //var result = await socket.ReceiveAsync(new ArraySegment<byte>(), cancellationToken);

                        //if (result.MessageType == WebSocketMessageType.Close)
                        //{
                        //    return;
                        //}

                        var memory = pipe.Output.GetMemory();
                        //var memory = new Memory<byte>(bytes);
                        var frame = memory.Slice(RSocketProtocol.MESSAGEFRAMESIZE);
                        //var segment = new ArraySegment<byte>(frame.ToArray());
                        var hasSegment = MemoryMarshal.TryGetArray<byte>(frame, out var segment);

                        // Exceptions are handled above where the send and receive tasks are being run.
                        var receiveResult = await socket.ReceiveAsync(segment, cancellationToken);

                        // Need to check again for netcoreapp3.0 because a close can happen between a 0-byte read and the actual read
                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            return;
                        }

                        logger.SocketDataReceived(receiveResult);

                        var length = memory.FixLength(receiveResult.Count);
                        var size = memory.CopyFrame(pipe.Output, length + receiveResult.Count);

                        pipe.Output.Advance(size);

                        var flushResult = await pipe.Output.FlushAsync(cancellationToken);

                        // We canceled in the middle of applying back pressure
                        // or if the consumer is done
                        if (flushResult.IsCanceled || flushResult.IsCompleted)
                        {
                            break;
                        }
                    }
                }
                catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    // Client has closed the WebSocket connection without completing the close handshake
                    //Log.ClosedPrematurely(_logger, ex);
                }
                catch (OperationCanceledException)
                {
                    // Ignore aborts, don't treat them like transport errors
                }
                catch (Exception ex)
                {
                    if (!aborted && !cancellationToken.IsCancellationRequested)
                    {
                        pipe.Output.Complete(ex);

                        // We re-throw here so we can communicate that there was an error when sending
                        // the close frame
                        throw;
                    }
                }
                finally
                {
                    // We're done writing
                    pipe.Output.Complete();
                }
            }

            private async Task StartSendingAsync(WebSocket socket)
            {
                Exception error = null;

                try
                {
                    while (true)
                    {
                        var result = await pipe.Input.ReadAsync();
                        var buffer = result.Buffer;
                        var consumed = buffer.Start;

                        try
                        {
                            if (result.IsCanceled)
                            {
                                break;
                            }

                            if (false == buffer.IsEmpty)
                            {
                                try
                                {
                                    if (WebSocketCanSend(socket))
                                    {
                                        var messageType = GetMessageType(transferFormat);
                                        consumed = await socket.SendAsync(buffer, buffer.Start, messageType, logger); //RSOCKET Framing
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (false == aborted)
                                    {
                                        logger.ErrorWritingFrame(exception);
                                    }

                                    break;
                                }
                            }
                            else if (result.IsCompleted)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            pipe.Input.AdvanceTo(consumed, buffer.End); //RSOCKET Framing
                        }
                    }
                }
                catch (Exception exception)
                {
                    error = exception;
                }
                finally
                {
                    // Send the close frame before calling into user code
                    if (WebSocketCanSend(socket))
                    {
                        // We're done sending, send the close frame to the client if the websocket is still open
                        var closeStatus = error != null
                            ? WebSocketCloseStatus.InternalServerError
                            : WebSocketCloseStatus.NormalClosure;

                        await socket.CloseOutputAsync(
                            closeStatus,
                            "",
                            CancellationToken.None
                        );
                    }

                    pipe.Input.Complete();
                }
            }

            private static void WriteFrameLength(Memory<byte> memory, int length)
            {
                
            }

            private static bool WebSocketCanSend(WebSocket ws) =>
                !(WebSocketState.Aborted == ws.State ||
                  WebSocketState.Closed == ws.State ||
                  WebSocketState.CloseSent == ws.State);

            private static WebSocketMessageType GetMessageType(TransferFormat format) =>
                TransferFormat.Binary == format
                    ? WebSocketMessageType.Binary
                    : WebSocketMessageType.Text;
        }
    }
}