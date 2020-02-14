using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core.Transports
{
    public sealed class ClientWebSocketTransport : WebSocketTransport
    {
        private Task handler;

        public Uri Url
        {
            get;
        }

        public ClientWebSocketTransport(
            string url,
            ILogger<WebSocketTransport> logger,
            PipeOptions outputPipeOptions = default,
            PipeOptions inputPipeOptions = default)
            : this(new Uri(url), logger, outputPipeOptions, inputPipeOptions)
        {
        }

        public ClientWebSocketTransport(
            Uri url, 
            ILogger<WebSocketTransport> logger, 
            PipeOptions outputPipeOptions = default, 
            PipeOptions inputPipeOptions = default)
            : base(logger, outputPipeOptions, inputPipeOptions)
        {
            Url = url;
        }

        /*public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var socket = new ClientWebSocket();
            var subProtocol = _options.SubProtocolSelector?.Invoke(context.WebSockets.WebSocketRequestedProtocols);

            await socket.ConnectAsync(Url, cancellationToken);

            if (WebSocketState.Open == socket.State)
            {
                handler = StartSending1Async(socket);
            }
            else
            {
                throw new Exception();
            }
        }*/

        protected override async ValueTask ProcessRequestAsync(CancellationToken cancellationToken)
        {
            var socket = new ClientWebSocket();
            //var subProtocol = _options.SubProtocolSelector?.Invoke(context.WebSockets.WebSocketRequestedProtocols);

            await socket.ConnectAsync(Url, cancellationToken);

            if (WebSocketState.Open != socket.State)
            {
                throw new Exception();
            }

            await Transport.ProcessSocketAsync(socket);
        }

        /*private async Task StartSending1Async(WebSocket socket)
        {
            var aborted = false;
            Exception error = null;

            try
            {
                while (true)
                {
                    var result = await Input.ReadAsync();
                    var buffer = result.Buffer;
                    var consumed = buffer.Start; //RSOCKET Framing
                    // Get a frame from the application

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
                                var messageType = WebSocketMessageType.Binary;

                                if (WebSocketCanSend(socket))
                                {
                                    consumed = await SendAsync(socket, buffer, buffer.Start, messageType); //RSOCKET Framing
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!aborted)
                                {
                                    //Log.ErrorWritingFrame(_logger, ex);
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
                        Input.AdvanceTo(consumed, buffer.End); //RSOCKET Framing
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                // Send the close frame before calling into user code
                if (WebSocketCanSend(socket))
                {
                    // We're done sending, send the close frame to the client if the websocket is still open
                    await socket.CloseOutputAsync(
                        error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure,
                        "", CancellationToken.None);
                }

                Input.Complete();
            }
        }*/

        /*private static async ValueTask<SequencePosition> SendAsync(
            WebSocket socket,
            ReadOnlySequence<byte> buffer,
            SequencePosition position,
            WebSocketMessageType webSocketMessageType,
            CancellationToken cancellationToken = default)
        {
            for (var frame = PeekFrame(buffer.Slice(position)); frame.Length > 0; frame = PeekFrame(buffer.Slice(position)))
            {
                var offset = buffer.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE, position);
                var sequence = buffer.Slice(offset);
                
                if (sequence.Length < frame.Length)
                {
                    //If there is a partial message in the buffer, yield to accumulate more. Can't compare SequencePositions...
                    break;
                }

                sequence = buffer.Slice(offset, frame.Length);
                position = buffer.GetPosition(frame.Length, offset);

                await SendAsync(socket, sequence, webSocketMessageType, cancellationToken);

                if (frame.IsEndOfMessage)
                {
                    break;
                }

                //position = buffer.GetPosition(frame.Length, offset);
            }

            //TODO Length may actually be on a boundary... Should use a reader.
            //while (buffer.TryGet(ref position, out var memory, advance: false))
            //{
            //	var (length, isEndOfMessage) = RSocketProtocol.MessageFrame(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(memory.Span));
            //	var offset = buffer.GetPosition(sizeof(int), position);
            //	if (buffer.Slice(offset).Length < length) { break; }	//If there is a partial message in the buffer, yield to accumulate more.
            //	await socket.SendAsync(buffer.Slice(offset, length), webSocketMessageType, cancellationToken);
            //	position = buffer.GetPosition(length, offset);
            //}
            return position;
        }*/

        /*private static ValueTask SendAsync(WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            if (buffer.IsSingleSegment)
            {
                return webSocket.SendAsync(buffer.First, webSocketMessageType, true, cancellationToken);
            }

            return SendMultiSegmentAsync(webSocket, buffer, webSocketMessageType, cancellationToken);
        }*/

        /*private static async ValueTask SendMultiSegmentAsync(WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            var position = buffer.Start;
            // Get a segment before the loop so we can be one segment behind while writing
            // This allows us to do a non-zero byte write for the endOfMessage = true send
            buffer.TryGet(ref position, out var prevSegment);

            while (buffer.TryGet(ref position, out var segment))
            {
                await webSocket.SendAsync(prevSegment, webSocketMessageType, false, cancellationToken);
                prevSegment = segment;
            }

            // End of message frame
            await webSocket.SendAsync(prevSegment, webSocketMessageType, true, cancellationToken);
        }*/

        /*private static (int Length, bool IsEndOfMessage) PeekFrame(ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);
            //var length = reader.CurrentSpan.Slice(0, sizeof(long));
            //return ((int) BitConverter.ToInt64(length), false);
            return reader.TryRead(out byte b1) && reader.TryRead(out byte b2) && reader.TryRead(out byte b3)
                ? ((b1 << 8 * 2) | (b2 << 8 * 1) | (b3 << 8 * 0), true)
                : (0, false);
        }*/
    }
}