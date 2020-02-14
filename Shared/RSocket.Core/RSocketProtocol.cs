using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using RSocket.Core.Extensions;
using RSocket.Core.Internal;

namespace RSocket.Core
{
    internal static partial class RSocketProtocol
    {
        public const int FRAMELENGTHSIZE = INT24SIZE;
        public const int MESSAGEFRAMESIZE = INT24SIZE;
        public const UInt16 MAJOR_VERSION = 1;
        public const UInt16 MINOR_VERSION = 0;

        private const int INT24SIZE = (sizeof(UInt32) - 1);
        private const int METADATALENGTHSIZE = INT24SIZE;
        private const int MaxMetadataLength = 16777215;

		public static async ValueTask HandleAsync(IRSocketProtocol sink, PipeReader reader, CancellationToken cancellationToken)
        {
            while (false == cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;
                
                if (buffer.IsEmpty && result.IsCompleted)
                {
                    break;
                }

                var position = buffer.Start;
                //Due to the nature of Pipelines as simple binary pipes, all Transport adapters assemble a standard message frame whether or not the underlying transport signals length, EoM, etc.
                var (length, endOfMessage) = buffer.PeekFrame();

                //Don't have a complete message yet. Tell the pipe that we've evaluated up to the current buffer end, but cannot yet consume it.
                if (buffer.Length < length)
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    continue;
                }

                var offset = length + MESSAGEFRAMESIZE;
                var frame = buffer.Slice(position, offset);

                await ProcessAsync(sink, frame, endOfMessage);

                position = buffer.GetPosition(offset, position);

                reader.AdvanceTo(position);
            }

            reader.Complete();
        }

        internal static bool TryReadRemaining(
            in Header header,
            int innerlength,
            ref FrameReader reader,
            out int metadatalength,
            out int datalength)
        {
            if (false == header.HasMetadata)
            {
                metadatalength = 0;
            }
            else if (0 < reader.ReadUInt24(out int length))
            {
                metadatalength = length;
            }
            else
            {
                metadatalength = default;
                datalength = default;
                return false;
            }

            datalength = header.Remaining - innerlength - header.MetadataHeaderLength - metadatalength;

            return true;
        }

        internal static ValueTask ProcessAsync(IRSocketProtocol sink, ReadOnlySequence<byte> frame, bool endOfMessage)
        {
            var reader = FrameReader.Get(ref frame);

            try
            {
                var header = new Header(ref reader);
                return HandleMessageAsync(sink, header, reader, endOfMessage);
            }
            finally
            {
                FrameReader.Return(reader);
            }
        }

        private static ValueTask HandleMessageAsync(IRSocketProtocol sink, Header header, FrameReader reader, bool endOfMessage)
        {
            switch (header.Type)
            {
                case Types.Setup:
                {
                    var message = new Setup(header, ref reader);
                    return sink.SetupAsync(message);
                }

                case Types.RequestResponse:
                {
                    var message = new RequestResponse(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.RequestResponseAsync(message);
                    }

                    break;
                }

                case Types.RequestStream:
                {
                    var message = new RequestStream(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.RequestStreamAsync(message);
                    }

                    break;
                }

                case Types.RequestChannel:
                {
                    var message = new RequestChannel(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.RequestChannelAsync(message);
                    }

                    break;
                }

                case Types.RequestN:
                {
                    var message = new RequestN(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.RequestNAsync(message);
                    }

                    break;
                }

                case Types.Payload:
                {
                    var message = new Payload(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.PayloadAsync(message);
                    }

                    break;
                }

                case Types.RequestFireAndForget:
                {
                    var message = new RequestFireAndForget(header, ref reader);

                    if (message.Validate())
                    {
                        return sink.RequestFireAndForgetAsync(message);
                    }

                    break;
                }

                case Types.Reserved:
                {
                    throw new InvalidOperationException();
                }

                default:
                {
                    if (false == header.CanIgnore)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new ValueTask();
        }

        public enum Types : Int32
        {
            /// <summary>Reserved</summary>
            Reserved = 0x00,

            /// <summary>Setup: Sent by client to initiate protocol processing.</summary>
            Setup = 0x01,

            /// <summary>Lease: Sent by Responder to grant the ability to send requests.</summary>
            Lease = 0x02,

            /// <summary>Keepalive: Connection keepalive.</summary>
            KeepAlive = 0x03,

            /// <summary>Request Response: Request single response.</summary>
            RequestResponse = 0x04,

            /// <summary>Fire And Forget: A single one-way message.</summary>
            RequestFireAndForget = 0x05,

            /// <summary>Request Stream: Request a completable stream.</summary>
            RequestStream = 0x06,

            /// <summary>Request Channel: Request a completable stream in both directions.</summary>
            RequestChannel = 0x07,

            /// <summary>Request N: Request N more items with Reactive Streams semantics.</summary>
            RequestN = 0x08,

            /// <summary>Cancel Request: Cancel outstanding request.</summary>
            Cancel = 0x09,

            /// <summary>Payload: Payload on a stream. For example, response to a request, or message on a channel.</summary>
            Payload = 0x0A,

            /// <summary>Error: Error at connection or application level.</summary>
            Error = 0x0B,

            /// <summary>Metadata: Asynchronous Metadata frame</summary>
            Metadata_Push = 0x0C,

            /// <summary>Resume: Replaces SETUP for Resuming Operation (optional)</summary>
            Resume = 0x0D,

            /// <summary>Resume OK : Sent in response to a RESUME if resuming operation possible (optional)</summary>
            Resume_OK = 0x0E,

            /// <summary>Extension Header: Used To Extend more frame types as well as extensions.</summary>
            Extension = 0x3F,
        }
    }
}