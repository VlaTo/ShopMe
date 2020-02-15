using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RSocket.Core.Extensions
{
    internal static class WebSocketExtensions
    {
        public static async ValueTask<SequencePosition> SendAsync(
            this WebSocket socket,
            ReadOnlySequence<byte> buffer,
            SequencePosition position,
            WebSocketMessageType messageType,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            for (
                var frame = buffer.Slice(position).PeekFrame(); 
                frame.Length > 0; 
                frame = buffer.Slice(position).PeekFrame())
            {
                //Console.WriteLine($"Send Frame[{frame.Length}]");
                var offset = buffer.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE, position);
                if (buffer.Slice(offset).Length < frame.Length)
                {
                    break;
                }    //If there is a partial message in the buffer, yield to accumulate more. Can't compare SequencePositions...

                var sequence = buffer.Slice(offset, frame.Length);
                var hasSegment = MemoryMarshal.TryGetArray(sequence.First, out var segment);

                await socket.SendAsync(segment, messageType, frame.IsEndOfMessage, cancellationToken);

                position = buffer.GetPosition(frame.Length, offset);
            }


            /*while (true)
            {
                var payload = buffer.Slice(position);
                var frame = payload.PeekFrame();

                if (0 == frame.Length || payload.Length < frame.Length)
                {
                    //If there is a partial message in the buffer, yield to accumulate more. Can't compare SequencePositions...
                    break;
                }

                var offset = buffer.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE, position);
                //var sequence = buffer.Slice(position, frame.Length);
                var sequence = buffer.Slice(offset, frame.Length);
                
                await SendAsync(socket, sequence, messageType, logger, cancellationToken);

                //position = buffer.GetPosition(frame.Length, position);
                position = buffer.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE + frame.Length, position);
            }*/

            /*for (var frame = buffer.Slice(position).PeekFrame();
                0 < frame.Length;
                frame = buffer.Slice(position).PeekFrame())
            {
                //var offset = buffer.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE, position);
                //var payload = buffer.Slice(offset);
                var payload = buffer.Slice(position);
                var length = payload.Length - RSocketProtocol.MESSAGEFRAMESIZE;

                if (length < frame.Length)
                {
                    //If there is a partial message in the buffer, yield to accumulate more. Can't compare SequencePositions...
                    break;
                }

                //var sequence = buffer.Slice(offset, frame.Length);
                var sequence = buffer.Slice(position, frame.Length);

                await SendAsync(socket, sequence, messageType, cancellationToken);

                //position = buffer.GetPosition(frame.Length, offset);
                position = buffer.GetPosition(frame.Length, position);
            }*/

            return position;
        }

        private static async ValueTask SendAsync(
            WebSocket socket,
            ReadOnlySequence<byte> buffer,
            WebSocketMessageType webSocketMessageType,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (buffer.IsSingleSegment)
            {
                var segment = new ArraySegment<byte>(buffer.First.ToArray());

                await socket.SendAsync(segment, webSocketMessageType, true, cancellationToken);
                
                logger.SocketDataSend(buffer.First, true);
                
                return;
            }

            var count = buffer.Length;
            var position = buffer.Start;

            while (buffer.TryGet(ref position, out var memory, true))
            {
                var segment = new ArraySegment<byte>(memory.Span.ToArray());

                count -= memory.Length;

                await socket.SendAsync(segment, webSocketMessageType, 0L == count, cancellationToken);
                
                logger.SocketDataSend(memory, 0L == count);
            }
        }

        /*private static (int Length, bool IsEndOfMessage) PeekFrame(ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence); 
            return reader.TryRead(out byte b1) 
                   && reader.TryRead(out byte b2) 
                   && reader.TryRead(out byte b3) 
                ? ((b1 << 8 * 2) | (b2 << 8 * 1) | (b3 << 8 * 0), true) : (0, false);
        }*/
    }
}