using System;
using System.Buffers;
using System.IO.Pipelines;
using RSocket.Core.Internal;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        internal ref struct Payload
        {
			public const ushort FLAG_FOLLOWS = 0b____00_10000000;
			public const ushort FLAG_COMPLETE = 0b___00_01000000;
			public const ushort FLAG_NEXT = 0b_______00_00100000;
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

            public bool IsComplete
            {
                get => header.HasCustom(FLAG_COMPLETE);
                set => header.SetCustom(FLAG_COMPLETE, value);
            }

            public bool IsNext
            {
                get => header.HasCustom(FLAG_NEXT);
                set => header.SetCustom(FLAG_NEXT, value);
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

            public Payload(
                int streamId,
                ReadOnlySequence<byte> data = default,
                ReadOnlySequence<byte> metadata = default,
                bool follows = false,
                bool complete = false,
                bool next = false)    //TODO Parameter ordering, isn't Next much more likely than C or F?
            {
                header = new Header(Types.Payload, streamId, 0 < metadata.Length);

                Metadata = metadata;
                Data = data;

				//TODO Assign HasMetadata based on this??? And everywhere.
				HasFollows = follows;
				IsComplete = complete;
				IsNext = next;
			}

			public Payload(in Header header, ref FrameReader reader)
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

                //if (header.HasMetadata)
                //{
                //	reader.TryReadUInt24BigEndian(out int length);
                //	MetadataLength = length;
                //	DataLength = framelength - header.Length - (sizeof(int) - 1) - MetadataLength;
                //}
                //else { MetadataLength = 0; DataLength = framelength - header.Length - MetadataLength; }
            }

            public bool Validate(bool canContinue = false)
			{
                if (Metadata.Length > MaxMetadataLength)
                {
                    return canContinue 
                        ? false 
                        : throw new ArgumentOutOfRangeException(nameof(Metadata), Metadata.Length, $"Invalid {nameof(Payload)} Message.");
                }

				//if (DataLength > MaxDataLength) { return canContinue ? false : throw new ArgumentOutOfRangeException(nameof(DataLength), DataLength, $"Invalid {nameof(Payload)} Message."); }
				//if (metadatalength != framelength - (Header.Length + payloadlength)) { }    //SPEC: Metadata Length MUST be equal to the Frame Length minus the sum of the length of the Frame Header and the length of the Frame Payload, if present.
				//Not sure how to assert this. If the frame has a length, then the payload length is computed from the frame length, resulting in a tautology. If the frame has no length, then the payload length is just what is, so the "frame length" is just this formula in reverse. So can it ever be false?

                if (false == IsComplete && false == IsNext)
                {
                    //SPEC: A PAYLOAD MUST NOT have both (C)complete and (N)ext empty (false).                    
                    throw new InvalidOperationException($"{nameof(Payload)} Messages must have {nameof(IsNext)} or {nameof(IsComplete)}.");
                }   

				return true;
			}

            public PipeWriter Write(PipeWriter pipe)
            {
                var writer = FrameWriter.Get(pipe);

                try
                {
                    var count = header.Write(writer, Length);

                    if (HasMetadata)
                    {
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