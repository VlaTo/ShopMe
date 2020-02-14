using RSocket.Core.Internal;
using System;
using System.Diagnostics;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        [Flags]
        public enum HeaderFlags : UInt16
        {
            Ignore = 0b_____10_00000000,
            MetaData = 0b___01_00000000,
            CustomMask = 0b_00_11111111,
            PrefixMask = 0b_11_00000000
        }

        [DebuggerTypeProxy(typeof(HeaderDebugProxy)), DebuggerDisplay("StreamId: {StreamId} {Type}")]
        public ref struct Header
        {
			public const Int32 DEFAULT_STREAM = 0;
			internal const int FRAMETYPE_OFFSET = 10;
			internal const ushort FRAMETYPE_TYPE = 0b_111111 << FRAMETYPE_OFFSET;
			internal const ushort FLAGS = 0b__________11_11111111;
			internal const ushort FLAG_IGNORE = 0b____10_00000000;
			internal const ushort FLAG_METADATA = 0b__01_00000000;

            private HeaderFlags flags;

            public bool CanIgnore
            {
                get => Has(HeaderFlags.Ignore);
                set => Set(HeaderFlags.Ignore, value);
            }

            public bool HasMetadata
            {
                get => Has(HeaderFlags.MetaData);
                set => Set(HeaderFlags.MetaData, value);
            }

			public int MetadataHeaderLength => HasMetadata ? METADATALENGTHSIZE : 0;
			
            public int Length => sizeof(Int32) + sizeof(UInt16);

            public int FrameLength;

			public int Remaining =>  FrameLength - Length;       //TODO Temporary refactoring

			// fields
            public Int32 StreamId;
            public Types Type;

			public Header(Types type, Int32 streamId = 0, bool hasMetadata = false)
            {
                flags = 0;
				Type = type;
				StreamId = streamId;
                FrameLength = 0;
				HasMetadata = hasMetadata;
            }

			public Header(ref FrameReader reader)
            {
                reader.ReadInt24(out FrameLength);
				reader.ReadInt32(out StreamId);
				reader.ReadUInt16(out UInt16 mask);

                (Type, flags) = ReadTypeAndFlags(mask);
            }

            public bool Has(HeaderFlags value) => value == (flags & value);

            public bool HasCustom(UInt16 value) => value == ((UInt16) flags & (value & (UInt16) HeaderFlags.CustomMask));
            
            public void Set(HeaderFlags value, bool set)
            {
                if (set)
                {
                    flags |= value;
                }
                else
                {
                    flags &= ~value;
                }
            }

            public void SetCustom(UInt16 mask, bool set)
            {
                if (set)
                {
                    flags |= (HeaderFlags)(mask & (UInt16)HeaderFlags.CustomMask);
                }
                else
                {
                    flags &= (HeaderFlags) ~(mask & (UInt16) HeaderFlags.CustomMask);
                }
            }

            public int Write(FrameWriter writer, int length)
			{
				writer.WriteInt24(length);     //Not included in total length.
				writer.WriteInt32(StreamId);
                writer.WriteUInt16((UInt16) (((int) Type << FRAMETYPE_OFFSET) & FRAMETYPE_TYPE | (UInt16)(flags & (HeaderFlags.PrefixMask | HeaderFlags.CustomMask))));

                return Length;
			}

            private static (Types Type, HeaderFlags Flags) ReadTypeAndFlags(ushort flags)
            {
                return (
                    (Types) ((flags & FRAMETYPE_TYPE) >> FRAMETYPE_OFFSET),
                    (HeaderFlags) (flags & (UInt16) (HeaderFlags.PrefixMask | HeaderFlags.CustomMask))
                );
            }

            private sealed class HeaderDebugProxy
            {
                public int StreamId { get; }

                public HeaderDebugProxy(ref Header header)
                    : this(header.StreamId)
                {
                }

                private HeaderDebugProxy(int streamId)
                {
                    StreamId = streamId;
                }
            }
		}
	}
}