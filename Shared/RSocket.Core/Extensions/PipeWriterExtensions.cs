using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core.Extensions
{
    public static class PipeWriterExtensions
    {
        /*public static Memory<byte> GetMemory(this PipeWriter output, out Memory<byte> memoryframe, bool haslength = false)
        {
            memoryframe = output.GetMemory(); 
            return haslength ? memoryframe : memoryframe.Slice(RSocketProtocol.MESSAGEFRAMESIZE);
        }

        public static void Advance(this PipeWriter output, int bytes, bool endOfMessage, in Memory<byte> memoryframe)
        {
            RSocketProtocol.MessageFrameWrite(bytes, endOfMessage, memoryframe.Span); 
            output.Advance(RSocketProtocol.MESSAGEFRAMESIZE + bytes);
        }

        public static int WriteInt24(this PipeWriter writer, Int32 value)
        {
            const int count = sizeof(Int32) - 1;
            var memory = writer.GetMemory(count);
            var span = memory.Span;

            span[0] = (byte)((value >> 16) & 0xFF);
            span[1] = (byte)((value >> 8) & 0xFF);
            span[2] = (byte)(value & 0xFF);

            //used += count;
            writer.Advance(count);

            return count;
        }

        public static int WriteInt32(this PipeWriter writer, Int32 value)
        {
            const int count = sizeof(Int32);
            var memory = writer.GetMemory(count);
            var span = memory.Span;

            BinaryPrimitives.WriteInt32BigEndian(span, value);

            //used += count;
            writer.Advance(count);

            return count;
        }

        public static int WriteUInt16(this PipeWriter writer, UInt16 value)
        {
            const int count = sizeof(UInt16);
            var memory = writer.GetMemory(count);
            var span = memory.Span;

            BinaryPrimitives.WriteUInt16BigEndian(span, value);

            //used += count;
            writer.Advance(count);

            return count;
        }*/

        public static async ValueTask FlushAsync(this PipeWriter writer, bool force, CancellationToken cancellationToken)
        {
            var result = await writer.FlushAsync(cancellationToken);

            if (result.IsCanceled)
            {
                ;
            }
        }
    }
}