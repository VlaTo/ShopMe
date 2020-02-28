using System;
using System.Buffers;
using System.Diagnostics;
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
            while (true)
            {
                var payload = buffer.Slice(position);
                var frame = payload.PeekFrame();

                if (0 >= frame.Length)
                {
                    logger.ZeroFrameLength(socket, payload);
                    break;
                }

                var payloadLength = payload.Length - RSocketProtocol.MESSAGEFRAMESIZE;

                if (payloadLength < frame.Length)
                {
                    break;
                }

                var offset = payload.GetPosition(RSocketProtocol.MESSAGEFRAMESIZE);
                var sequence = buffer.Slice(offset, frame.Length);
                var hasArray = MemoryMarshal.TryGetArray(sequence.First, out var array);

                Debug.Assert(hasArray);

                await socket.SendAsync(array, messageType, frame.IsEndOfMessage, cancellationToken);

                position = buffer.GetPosition(frame.Length, offset);
            }

            return position;
        }

        /*private static async ValueTask SendAsync(
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
        }*/

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