using RSocket.Core.Internal;
using System;
using System.Buffers;
using System.IO.Pipelines;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        internal ref struct RequestStream
        {
            public const ushort FLAG_FOLLOWS = 0b___00_10000000;

            private const int innerLength = sizeof(Int32);
            private Header header;

            public bool HasMetadata
            {
                get => header.HasMetadata;
                set => header.HasMetadata = value;
            }

            public bool HasFollows
            {
                get => header.HasCustom(FLAG_FOLLOWS);
                set => header.SetCustom(FLAG_FOLLOWS, value);
            }

            public Int32 StreamId => header.StreamId;

            public Int32 InitialRequest;

            public int Length => header.Length
                                 + innerLength
                                 + header.MetadataHeaderLength
                                 + (int) Metadata.Length
                                 + (int) Data.Length;

            public ReadOnlySequence<byte> Data
            {
                get; 
            }

            public ReadOnlySequence<byte> Metadata
            {
                get; 
            }

            public RequestStream(
                int streamId,
                ReadOnlySequence<byte> data,
                ReadOnlySequence<byte> metadata = default,
                int initialRequest = 0,
                bool follows = false)
            {
                header = new Header(Types.RequestStream, streamId, 0 < metadata.Length);

                InitialRequest = initialRequest;
                Data = data;
                Metadata = metadata;
                HasFollows = follows;
            }

            public RequestStream(in Header header, ref FrameReader reader)
            {
                this.header = header;

                reader.ReadInt32(out InitialRequest);

                TryReadRemaining(header, innerLength, ref reader, out var metadataLength, out var dataLength);

                if (header.HasMetadata && 0 < metadataLength)
                {
                    reader.Read(metadataLength, out var bytes);
                    Metadata = new ReadOnlySequence<byte>(bytes.ToArray());
                }
                else
                {
                    Metadata = ReadOnlySequence<byte>.Empty;
                }

                if (0 < dataLength)
                {
                    reader.Read(dataLength, out var bytes);
                    Data = new ReadOnlySequence<byte>(bytes.ToArray());
                }
                else
                {
                    Data = ReadOnlySequence<byte>.Empty;
                }

                //if (header.HasMetadata)
                //{
                //	reader.TryReadUInt24BigEndian(out int length);
                //	MetadataLength = length;
                //	DataLength = framelength - header.Length - (sizeof(int) - 1) - MetadataLength;
                //}
                //else
                //{
                //MetadataLength = 0; 
                //DataLength = framelength - header.Length - MetadataLength;
                //}
            }

            public bool Validate(bool canContinue = false)
            {
                if (0 == header.StreamId)
                {
                    return canContinue
                        ? false
                        : throw new ArgumentOutOfRangeException(nameof(StreamId), StreamId,
                            $"Invalid {nameof(RequestStream)} Message.");
                }

                if (Metadata.Length > MaxMetadataLength)
                {
                    return canContinue
                        ? false
                        : throw new ArgumentOutOfRangeException(nameof(Metadata), Metadata.Length,
                            $"Invalid {nameof(RequestStream)} Message.");
                }

                if (InitialRequest <= 0)
                {
                    //SPEC: Value MUST be > 0.
                    return canContinue
                        ? false
                        : throw new ArgumentOutOfRangeException(nameof(InitialRequest), InitialRequest,
                            $"Invalid {nameof(RequestStream)} Message.");
                }

                return true;
            }

            public PipeWriter Write(PipeWriter pipe)
            {
                var writer = FrameWriter.Get(pipe);

                try
                {
                    var count = header.Write(writer, Length);

                    count += writer.WriteInt32(InitialRequest);

                    if (HasMetadata)
                    {
                        //TODO Should this be UInt24? Probably, but not sure if it can actually overflow...
                        count += writer.WriteMetadata(Metadata);
                    }

                    count += writer.Write(Data);

                    writer.Flush();
                }
                finally
                {
                    FrameWriter.Return(writer);
                }

                return pipe;
            }
        }
    }
}