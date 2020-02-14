using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RSocket.Core.Internal;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        internal ref struct RequestResponse
        {
			public const ushort FLAG_FOLLOWS = 0b___00_10000000;
            private const int InnerLength = 0;

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

            public int Length => header.Length
                                 + InnerLength
                                 + header.MetadataHeaderLength
                                 + (int) Metadata.Length
                                 + (int) Data.Length;

            public ReadOnlySequence<byte> Metadata
            {
                get;
                private set;
            }

            public ReadOnlySequence<byte> Data
            {
                get;
                private set;
            }

			public RequestResponse(
                Int32 streamId,
                ReadOnlySequence<byte> data,
                ReadOnlySequence<byte> metadata = default,
                Int32 initialRequest = 0,
                bool follows = false)
            {
                header = new Header(Types.RequestResponse, streamId, 0 < metadata.Length);
                Metadata = metadata;
                Data = data;
				HasFollows = follows;
            }

            public RequestResponse(in Header header, ref FrameReader reader)
            {
                this.header = header;

                TryReadRemaining(header, InnerLength, ref reader, out var metadataLength, out var dataLength);

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
            }

            public bool Validate(bool canContinue = false)
			{
                if (0 == header.StreamId)
                {
                    return canContinue 
                        ? false 
                        : throw new ArgumentOutOfRangeException(nameof(header.StreamId), header.StreamId, $"Invalid {nameof(RequestResponse)} Message.");
                }

                if (MaxMetadataLength < Metadata.Length)
                {
                    return canContinue 
                        ? false 
                        : throw new ArgumentOutOfRangeException(nameof(Metadata), Metadata.Length, $"Invalid {nameof(RequestResponse)} Message.");
                }

                return true;
            }

            public PipeWriter Write(PipeWriter pipe)
            {
                var writer = FrameWriter.Get(pipe);

                try
                {
                    var written = header.Write(writer, Length);

                    if (HasMetadata)
                    {
                        //TODO Should this be UInt24? Probably, but not sure if it can actually overflow...
                        written += writer.WriteMetadata(Metadata);
                    }

                    written += writer.Write(Data);

                    if (0 < written)
                    {
                        ;
                    }

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