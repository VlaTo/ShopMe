using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;

namespace RSocket.Core.Internal
{
    internal sealed class FrameReader
    {
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

        private ReadOnlySequence<byte> sequence;
        private SequencePosition position;

        [ThreadStatic]
        private static FrameReader instance;

        public Encoding Encoding
        {
            get;
        }

        public Encoder Encoder
        {
            get;
        }

        public bool IsEmpty => sequence.Equals(ReadOnlySequence<byte>.Empty) || sequence.IsEmpty;

        private FrameReader(ref ReadOnlySequence<byte> sequence, Encoding encoding = null)
        {
            this.sequence = sequence;
            position = sequence.GetPosition(0);

            Encoding = encoding ?? DefaultEncoding;
            Encoder = Encoding.GetEncoder();
        }

        public static FrameReader Get(ref ReadOnlySequence<byte> sequence, Encoding encoding = null)
        {
            var reader = instance ?? new FrameReader(ref sequence, encoding);

            instance = null;

            if (reader.IsEmpty)
            {
                reader.Reset(ref sequence);
            }

            return reader;
        }

        public static void Return(FrameReader reader)
        {
            var sequence = ReadOnlySequence<byte>.Empty;
            instance = reader.Reset(ref sequence);
        }

        public int ReadInt32(out Int32 value)
        {
            //const int size = sizeof(Int32);
            var size = Marshal.SizeOf(typeof(Int32));
            var span = GetBuffer(size);

            value = BinaryPrimitives.ReadInt32BigEndian(span);

            return size;
        }

        public int ReadByte(out Byte value)
        {
            var size = Marshal.SizeOf(typeof(Byte));
            var span = GetBuffer(size);

            value = span[0];

            return size;
        }

        public int ReadUInt16(out UInt16 value)
        {
            var size = Marshal.SizeOf(typeof(UInt16));
            var span = GetBuffer(size);

            value = BinaryPrimitives.ReadUInt16BigEndian(span);

            return size;
        }

        public int ReadInt24(out Int32 value)
        {
            var size = Marshal.SizeOf(typeof(Int32)) - 1;
            var span = GetBuffer(size);

            value = span[0] << 16 | span[1] << 8 | span[2];

            return size;
        }

        public int ReadUInt24(out Int32 value)
        {
            var size = Marshal.SizeOf(typeof(UInt32)) - 1;
            var span = GetBuffer(size);

            value = span[0] << 16 | span[1] << 8 | span[2];

            return size;
        }

        public int ReadTimeSpan(out TimeSpan value)
        {
            var count = ReadInt32(out var milliseconds);
            value = TimeSpan.FromMilliseconds(milliseconds);
            return count;
        }

        public int ReadString(out string value)
        {
            var count = ReadByte(out var length);
            count += Read(length, out var bytes);
            value = Encoding.GetString(bytes.ToArray());
            return count;
        }

        /*public int Read(out Span<byte> value)
        {
            var count = ReadByte(out var length);
            count += Read(length, out var bytes);
            value = Encoding.GetString(bytes);
            return count;
        }*/

        public int ReadMimeType(out string value)
        {
            var count = ReadByte(out var length);
            
            count += Read(length, out var bytes);

            value = Encoding.GetString(bytes.ToArray());
            
            return count;
        }

        public int ReadToken(out byte[] value)
        {
            var count = ReadUInt16(out var length);
            
            count += Read(length, out var buffer);
            
            value = buffer.ToArray();
            
            return count;
        }

        public int Read(int length, out Span<byte> span)
        {
            var temp = GetBuffer(length);
            span = new Span<byte>(temp.ToArray());
            return length;
        }

        private ReadOnlySpan<byte> GetBuffer(int size)
        {
            var buffer = new Memory<byte>(new byte[size]);
            var offset = position;
            var length = 0;

            while (length < size)
            {
                if (false == sequence.TryGet(ref offset, out var memory))
                {
                    throw new Exception();
                }

                if (memory.IsEmpty)
                {
                    continue;
                }

                var count = Math.Min(memory.Length, size - length);
                var source = memory.Slice(0, count);
                var destination = buffer.Slice(length);

                source.CopyTo(destination);

                length += count;

                offset = Advance(count);
            }

            return buffer.Span;
        }

        private SequencePosition Advance(int offset)
        {
            position = sequence.GetPosition(offset, position);
            return position;
        }

        private FrameReader Reset(ref ReadOnlySequence<byte> value)
        {
            sequence = value;
            position = sequence.GetPosition(0);

            Encoder.Reset();

            return this;
        }
    }
}