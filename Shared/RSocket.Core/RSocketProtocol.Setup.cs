using System;
using System.Buffers;
using System.Data;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RSocket.Core.Extensions;
using RSocket.Core.Internal;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        public ref struct ProtocolVersion
        {
            public UInt16 Major;

            public UInt16 Minor;

            public ProtocolVersion(ushort major, ushort minor)
            {
                Major = major;
                Minor = minor;
            }
            public ProtocolVersion(ref FrameReader reader)
            {
                reader.ReadUInt16(out Major);
                reader.ReadUInt16(out Minor);
            }

            public int Write(FrameWriter writer)
            {
                var count = writer.WriteUInt16(Major);
                
                count += writer.WriteUInt16(Minor);

                return count;
            }
        }

        public ref struct Setup
        {
            public const ushort FLAG_METADATA = 0b__01_00000000;
            public const ushort FLAG_RESUME = 0b____00_10000000;
            public const ushort FLAG_LEASE = 0b_____00_01000000;

            private Header header;
            private readonly TimeSpan keepAlive;
            private readonly TimeSpan lifetime;
            private string metadataMimeType;
            private string dataMimeType;

            public ProtocolVersion Version;

            //public UInt16 MajorVersion;

            //public UInt16 MinorVersion;

            public ReadOnlySequence<byte> ResumeToken { get; }

            public bool HasResume
            {
                get => header.HasCustom(FLAG_RESUME);
                set => header.SetCustom(FLAG_RESUME, value);
            }

            public bool HasMetadata
            {
                get => header.HasMetadata;
                set => header.HasMetadata = value;
            }

            public int Length => header.Length
                                 + InnerLength
                                 + header.MetadataHeaderLength
                                 + (int) Metadata.Length
                                 + (int) Data.Length;

            internal int InnerLength => sizeof(UInt16) + sizeof(UInt16) + sizeof(Int32) + sizeof(Int32)
                                        + (HasResume ? (int) ResumeToken.Length : 0)
                                        + sizeof(byte) + Encoding.ASCII.GetByteCount(metadataMimeType)
                                        + sizeof(byte) + Encoding.ASCII.GetByteCount(dataMimeType);

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

            public Setup(
                TimeSpan keepAlive,
                TimeSpan lifetime,
                string metadataMimeType = null,
                string dataMimeType = null,
                ReadOnlySequence<byte> data = default,
                ReadOnlySequence<byte> metadata = default)
                : this(keepAlive, lifetime, metadataMimeType ?? String.Empty, dataMimeType ?? String.Empty,null, data, metadata)
            {
            }

            public Setup(
                TimeSpan keepAlive,
                TimeSpan lifetime,
                string metadataMimeType,
                string dataMimeType,
                byte[] resumeToken = default,
                ReadOnlySequence<byte> data = default,
                ReadOnlySequence<byte> metadata = default)
            {
                header = new Header(Types.Setup, hasMetadata: false == metadata.IsEmpty);

                this.keepAlive = keepAlive;
                this.lifetime = lifetime;
                this.metadataMimeType = metadataMimeType;
                this.dataMimeType = dataMimeType;

                Version = new ProtocolVersion(1, 0);
                Metadata = metadata;
                Data = data;
                ResumeToken = null != resumeToken
                    ? new ReadOnlySequence<byte>(resumeToken)
                    : ReadOnlySequence<byte>.Empty;
                HasResume = resumeToken != null && resumeToken.Length > 0;
            }

            public Setup(in Header header, ref FrameReader reader)
			{
				this.header = header;

                Version = new ProtocolVersion(ref reader);

                reader.ReadTimeSpan(out keepAlive);
                reader.ReadTimeSpan(out lifetime);

                if (header.HasCustom(FLAG_RESUME))
                {
                    reader.ReadToken(out var resumeToken);
                    ResumeToken = new ReadOnlySequence<byte>(resumeToken);
                }
                else
                {
                    ResumeToken = ReadOnlySequence<byte>.Empty;
                }

                reader.ReadMimeType(out metadataMimeType);
                reader.ReadMimeType(out dataMimeType);

                Metadata = ReadOnlySequence<byte>.Empty;
                Data = ReadOnlySequence<byte>.Empty;

                TryReadRemaining(header, InnerLength, ref reader, out var metadataLength, out var dataLength);

                if (header.HasMetadata && 0 < metadataLength)
                {
                    reader.Read(metadataLength, out var bytes);
                    Metadata = new ReadOnlySequence<byte>(bytes.ToArray());
                }

                if (0 < dataLength)
                {
                    reader.Read(dataLength, out var bytes);
                    Data = new ReadOnlySequence<byte>(bytes.ToArray());
                }
            }

            public PipeWriter Write(PipeWriter pipe)
            {
                var writer = FrameWriter.Get(pipe);

                try
                {
                    var count = header.Write(writer, Length);

                    count += Version.Write(writer);
                    count += writer.WriteTimeSpan(keepAlive);
                    count += writer.WriteTimeSpan(lifetime);

                    if (HasResume)
                    {
                        count += writer.WriteToken(ResumeToken);
                    }

                    count += writer.WriteMimeType(metadataMimeType);
                    count += writer.WriteMimeType(dataMimeType);

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