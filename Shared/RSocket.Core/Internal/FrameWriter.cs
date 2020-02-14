using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace RSocket.Core.Internal
{
    internal sealed class FrameWriter
    {
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

        [ThreadStatic]
        private static FrameWriter instance;
        private IBufferWriter<byte> writer;
        private Memory<byte> memory;
        private int bytesPerChar;
        private int used;

        public Encoding Encoding
        {
            get;
        }

        public Encoder Encoder
        {
            get;
        }

        public bool InUse => null != writer;

        private int Remaining => memory.Length - used;

        private FrameWriter(IBufferWriter<byte> writer, Encoding encoding)
        {
            this.writer = writer;

            memory = Memory<byte>.Empty;
            used = 0;

            Encoding = encoding ?? DefaultEncoding;
            Encoder = Encoding.GetEncoder();

            bytesPerChar = Encoding.GetMaxByteCount(1);
        }

        public (Memory<byte>, int) Frame()
        {
            EnsureBuffer(sizeof(int));

            var frame = (memory, used);

            used += sizeof(int);

            return frame;
        }

        public void Frame((Memory<byte>, int) frame, int value)
        {
            var (m, u) = frame;
            var span = m.Span.Slice(u, m.Length - u);

            BinaryPrimitives.WriteInt32BigEndian(span, value);
        }

        public int WriteByte(byte value)
        {
            const int count = sizeof(byte);
            
            EnsureBuffer(count); 
            
            memory.Span[used++] = value;
            
            return count;
        }

        public int WriteUInt16(UInt16 value)
        {
            const int count = sizeof(UInt16);

            BinaryPrimitives.WriteUInt16BigEndian(GetBuffer(count), value);

            used += count;

            return count;
        }

        public int Write(UInt32 value)
        {
            const int count = sizeof(UInt32);

            BinaryPrimitives.WriteUInt32BigEndian(GetBuffer(count), value); 

            used += count; 

            return count;
        }

        public int WriteInt32(Int32 value)
        {
            const int count = sizeof(Int32);

            BinaryPrimitives.WriteInt32BigEndian(GetBuffer(count), value); 

            used += count; 

            return count;
        }

        public int Write(Int64 value)
        {
            const int count = sizeof(Int64);

            BinaryPrimitives.WriteInt64BigEndian(GetBuffer(count), value); 

            used += count; 

            return count;
        }

        public int WriteInt24(Int32 value)
        {
            const int count = 3; 
            var span = GetBuffer(count); 
            
            span[0] = (byte)((value >> 16) & 0xFF); 
            span[1] = (byte)((value >> 8) & 0xFF); 
            span[2] = (byte)(value & 0xFF); 
            
            used += count; 
            
            return count;
        }

        public int Write(byte[] values)
        {
            foreach (var value in values)
            {
                Write(value);
            } 

            return values.Length;
        }

        public int Write(ReadOnlySpan<byte> values)
        {
            foreach (var value in values)
            {
                WriteByte(value);
            }

            return values.Length;
        }

        public int Write(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return Write(sequence.First.Span);
            }

            var count = 0;

            foreach (var mem in sequence)
            {
                count += Write(mem.Span);
            } 
            
            return count;
        }

        public int Write(string text) => Write(text.AsSpan(), Encoder, bytesPerChar);

        public void Write(char value, Encoder encoder, int encodingBytesPerChar)
        {
            var destination = GetBuffer(encodingBytesPerChar);

#if NETCOREAPP2_2
            var bytesUsed = 0;
            var charsUsed = 0;
            _encoder.Convert(new Span<char>(&value, 1), destination, false, out charsUsed, out bytesUsed, out _);
#else
            /*fixed (byte* destinationBytes = &MemoryMarshal.GetReference(destination))
            {
                encoder.Convert(&value, 1, destinationBytes, destination.Length, false, out charsUsed, out bytesUsed, out _);
            }*/

            //fixed (byte* destinationBytes = &MemoryMarshal.GetReference(destination))
            //{
            var chars = new Span<char>(new[] {value}).ToArray();
            var array = destination.ToArray();
            encoder.Convert(chars,0,1, array,0,array.Length, false, out var charsUsed, out var bytesUsed, out _);
            //}
#endif
            //System.Diagnostics.Debug.Assert(charsUsed == 1);
            used += bytesUsed;
        }

        public void Write(char[] buffer, int index, int count) => Write(buffer.AsSpan(index, count), Encoder, bytesPerChar);
        
        public void Write(char[] buffer) => Write(buffer, Encoder, bytesPerChar);
        
        public void Write(char value) => Write(value, Encoder, bytesPerChar);

        public int WriteMimeType(string text)
        {
            var length = Encoding.GetByteCount(text);

            if (Byte.MaxValue < length)
            {
                throw new ArgumentOutOfRangeException(nameof(text), text, $"String encoding [{length}] would exceed the maximum prefix length. [{Byte.MaxValue}]");
            }

            var count = WriteByte((Byte) length);
            
            count += Write(text.AsSpan(), Encoder, bytesPerChar);

            return count;
        }

        /*public int WriteToken(byte[] token)
        {
            var length = token.Length;
            var count = WriteUInt16((UInt16) length);

            for (var index = 0; index < token.Length; index++)
            {
                var @byte = token[index];
                count += WriteByte(@byte);
            }

            return count;
        }*/

        public int WriteToken(ReadOnlySequence<byte> token)
        {
            var count = WriteUInt16((UInt16) token.Length);
            
            count += Write(token);

            return count;
        }

        public int WriteMetadata(ReadOnlySequence<byte> metadata)
        {
            var count = WriteInt24((int) metadata.Length);
            
            count += Write(metadata);

            return count;
        }

        public int WritePrefixShort(string text)
        {
            var count = Encoding.GetByteCount(text);

            if (UInt16.MaxValue < count)
            {
                throw new ArgumentOutOfRangeException(nameof(text), text, $"String encoding [{count}] would exceed the maximum prefix length. [{UInt16.MaxValue}]");
            }

            WriteUInt16((UInt16) count);

            return sizeof(UInt16) + Write(text);
        }

        public int WritePrefixShort(ReadOnlySpan<byte> buffer)
        {
            var length = buffer.Length;

            if (UInt16.MaxValue < length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), length, $"Buffer [{length}] would exceed the maximum prefix length. [{UInt16.MaxValue}]");
            }

            WriteUInt16((UInt16) length);
            
            return sizeof(UInt16) + Write(buffer);
        }

        public int WritePrefixShort(ReadOnlySequence<byte> buffer)
        {
            var length = buffer.Length;

            if (UInt16.MaxValue < length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Buffer [{length}] would exceed the maximum prefix length. [{UInt16.MaxValue}]");
            }

            WriteUInt16((UInt16) length);
            
            return sizeof(UInt16) + Write(buffer);
        }

        public int WriteTimeSpan(TimeSpan value) => WriteInt32((int) value.TotalMilliseconds);

        public void Flush()
        {
            if (0 < used)
            {
                writer.Advance(used);
                memory = memory.Slice(used, Remaining);
                used = 0;
            }
        }

        private int Write(ReadOnlySpan<char> source, Encoder encoder, int encodingBytesPerChar)
        {
            var length = 0;

            while (source.Length > 0)
            {
                var destination = GetBuffer(encodingBytesPerChar);
                var array = destination.ToArray();

                //encoder.Convert(source, destination, false, out var charsUsed, out var bytesUsed, out _);
                encoder.Convert(source.ToArray(), 0, source.Length, array,0, array.Length, false, out var charsUsed, out var bytesUsed, out _);

                source = source.Slice(charsUsed);

                used += bytesUsed;
                length += bytesUsed;
            }

            return length;
        }

        private FrameWriter Reset(IBufferWriter<byte> value = null)
        {
            writer = value;

            memory = Memory<byte>.Empty;
            used = 0;

            Encoder.Reset();

            return this;
        }

        private void EnsureBuffer(int needed)
        {
            if (Remaining < needed)
            {
                if (used > 0)
                {
                    writer.Advance(used);
                }

                memory = writer.GetMemory(needed);
                used = 0;
            }
        }

        private Span<byte> GetBuffer(int needed)
        {
            EnsureBuffer(needed);
            return memory.Span.Slice(used, Remaining);
        }

        public static FrameWriter Get(IBufferWriter<byte> writer, Encoding encoding = null)
        {
            var temp = instance ?? new FrameWriter(null, encoding);

            instance = null;

            if (temp.InUse)
            {
                throw new InvalidOperationException();
            }

            return temp.Reset(writer);
        }

        public static void Return(FrameWriter writer)
        {
            instance = writer.Reset();
        }
    }
}