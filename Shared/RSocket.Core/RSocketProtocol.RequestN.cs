using System;
using System.Buffers;
using System.IO.Pipelines;
using RSocket.Core.Internal;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        internal ref struct RequestN
		{
            private const int InnerLength = sizeof(Int32);

			private Header header;

            public Int32 StreamId => header.StreamId;

			public Int32 RequestNumber;

            public int Length => header.Length + InnerLength;

            public bool HasMetadata
            {
                get => header.HasMetadata; 
                set => header.HasMetadata = value;
            }

			public RequestN(
                Int32 streamId,
                Int32 initialRequest = 0,
                bool follows = false)
            {
                header = new Header(Types.RequestN, streamId);

				RequestNumber = initialRequest;
			}

			public RequestN(in Header header, ref FrameReader reader)
			{
				this.header = header;

				reader.ReadInt32(out RequestNumber);
			}

			public bool Validate(bool canContinue = false)
			{
                if (0 == header.StreamId)
                {
                    return canContinue
                        ? false
                        : throw new ArgumentOutOfRangeException(nameof(header.StreamId), header.StreamId, $"Invalid {nameof(RequestN)} Message.");
                }

                if (HasMetadata)
                {
                    return canContinue ? false : throw new ArgumentOutOfRangeException(nameof(HasMetadata), HasMetadata, $"Invalid {nameof(RequestN)} Message.");
                }

                if (RequestNumber <= 0)
                {
                    //SPEC: Value MUST be > 0.
                    return canContinue ? false : throw new ArgumentOutOfRangeException(nameof(RequestNumber), RequestNumber, $"Invalid {nameof(RequestN)} Message.");
                }   
				
                return true;
			}

            public PipeWriter Write(PipeWriter pipe)
            {
                var writer = FrameWriter.Get(pipe);

                try
                {
                    var count = header.Write(writer, Length);
                    
                    count += writer.WriteInt32(RequestNumber);
                    
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