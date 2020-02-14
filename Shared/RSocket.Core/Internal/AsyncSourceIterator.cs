using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class AsyncSourceIterator<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> source;

        public AsyncSourceIterator(IAsyncEnumerable<T> source)
        {
            this.source = source;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new AsyncSourceEnumerator(source, cancellationToken);
        }

        // private AsyncSourceEnumerator class
        private sealed class AsyncSourceEnumerator : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerable<T> source;
            private readonly CancellationToken cancellationToken;

            private IAsyncEnumerator<T> enumerator;
            private bool disposed;

            public T Current
            {
                get
                {
                    EnsureEnumerator();
                    EnsureNotDisposed();

                    return enumerator.Current;
                }
            }

            public AsyncSourceEnumerator(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
            {
                this.source = source;
                this.cancellationToken = cancellationToken;
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                EnsureNotDisposed();

                if (null == enumerator)
                {
                    enumerator = source.GetAsyncEnumerator(cancellationToken);
                }

                var success = await MoveNextInternalAsync();

                return success;
            }

            public ValueTask DisposeAsync() => DisposeAsync(true);

            private ValueTask<bool> MoveNextInternalAsync() => enumerator.MoveNextAsync();

            private void EnsureEnumerator()
            {
                if (null == enumerator)
                {
                    throw new InvalidOperationException();
                }
            }

            private void EnsureNotDisposed()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("AsyncIteratorEnumerator");
                }
            }

            private async ValueTask DisposeAsync(bool dispose)
            {
                if (disposed)
                {
                    return;
                }

                try
                {
                    if (dispose)
                    {
                        if (null != enumerator)
                        {
                            await enumerator.DisposeAsync();
                        }
                    }
                }
                finally
                {
                    disposed = true;
                }
            }
        }
    }
}