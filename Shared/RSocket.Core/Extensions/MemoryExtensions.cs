using System;
using System.IO.Pipelines;

namespace RSocket.Core.Extensions
{
    internal static class MemoryExtensions
    {
        public static int FixLength(this Memory<byte> memory, int length)
        {
            var span = memory.Span;

            if (RSocketProtocol.MESSAGEFRAMESIZE > span.Length)
            {
                throw new InvalidOperationException();
            }

            span[0] = (byte)((length >> 16) & 0xFF);
            span[1] = (byte)((length >> 8) & 0xFF);
            span[2] = (byte)(length & 0xFF);

            return RSocketProtocol.MESSAGEFRAMESIZE;
        }

        public static int CopyFrame(this Memory<byte> memory, PipeWriter pipe, int length)
        {
            //var count = RSocketProtocol.MESSAGEFRAMESIZE + length;
            var buffer = pipe.GetMemory(length);
            var slice = memory.Slice(0, length);

            slice.CopyTo(buffer);

            return length;
        }
    }
}