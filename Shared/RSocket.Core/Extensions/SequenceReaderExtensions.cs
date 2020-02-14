using System.Buffers;
using System.Runtime.CompilerServices;

namespace RSocket.Core.Extensions
{
    internal static class SequenceReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int Length, bool IsEndOfMessage) PeekFrame(this ReadOnlySequence<byte> sequence)
        {
            var position = sequence.GetPosition(0L);
            if (sequence.TryGet(ref position, out var prefix, false)&&2<prefix.Length)
            {
                var memory = prefix.Slice(0, RSocketProtocol.MESSAGEFRAMESIZE).ToArray();
                //return memory.TryRead(out byte b1) && reader.TryRead(out byte b2) && reader.TryRead(out byte b3)
                return ((memory[0] << 8 * 2) | (memory[1] << 8 * 1) | (memory[2] << 8), true);
            }

            return (0, false);
        }
	}
}