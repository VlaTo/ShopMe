using System.IO.Pipelines;

namespace RSocket.Core.Transports
{
	//IDuplexPipe is defined in System.IO.Pipelines, but they provide no default implementation!!! So something like the below occurs all over the place: SignalR, etc. Also, Input and Output are awful names, but I agree they're hard to name.

	//              BACK             //
	//        output    input        //
	//        ---------------        //
	//        writer   reader        //
	//          ||       /\	         //
	//          ||       ||          //
	//          ||       ||          //
	//          ||       ||          //
	//          \/       ||          //
	//        reader   writer        //
	//        ---------------        //
	//        input    output        //
	//             FRONT	         //

	public class DuplexPipe : IDuplexPipe
	{
        public static readonly PipeOptions DefaultOptions;
        public static readonly PipeOptions ImmediateOptions;

        public PipeReader Input
        {
            get;
        }

        public PipeWriter Output
        {
            get;
        }

        public DuplexPipe(PipeWriter writer, PipeReader reader)
        {
            Input = reader;
            Output = writer;
        }

        static DuplexPipe()
        {
            DefaultOptions = new PipeOptions(
                writerScheduler: PipeScheduler.ThreadPool,
                readerScheduler: PipeScheduler.ThreadPool,
                useSynchronizationContext: false,
                pauseWriterThreshold: 0,
                resumeWriterThreshold: 0
            );
            ImmediateOptions = new PipeOptions(
                writerScheduler: PipeScheduler.Inline,
                readerScheduler: PipeScheduler.Inline,
                useSynchronizationContext: true,
                pauseWriterThreshold: 0,
                resumeWriterThreshold: 0
            );
        }

        public static (IDuplexPipe Front, IDuplexPipe Back) CreatePair(PipeOptions frontToBackOptions, PipeOptions backToFrontOptions)
		{
			var frontToBack = new Pipe(backToFrontOptions ?? DefaultOptions);
            var backToFront = new Pipe(frontToBackOptions ?? DefaultOptions);

            return (
                new DuplexPipe(frontToBack.Writer, backToFront.Reader),
                new DuplexPipe(backToFront.Writer, frontToBack.Reader)
            );
        }
	}
}
