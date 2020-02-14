using System;
using System.Buffers;
using System.Net.WebSockets;
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
            while (true)
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
            }

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
    }
}