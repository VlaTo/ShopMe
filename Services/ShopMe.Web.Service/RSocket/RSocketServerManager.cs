using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using RSocket.Core;

namespace ShopMe.Web.Service.RSocket
{
    internal sealed class RSocketServerManager
    {
        private readonly object gate;
        private readonly Dictionary<RSocketServer, IDisposable> servers;

        public RSocketServerManager()
        {
            gate = new object();
            servers = new Dictionary<RSocketServer, IDisposable>();
        }

        public IDisposable RegisterServer(RSocketServer server)
        {
            if (null == server)
            {
                throw new ArgumentNullException(nameof(server));
            }

            IDisposable disposable;

            lock (gate)
            {
                if (false == servers.TryGetValue(server, out disposable))
                {
                    disposable = Disposable.Create(server, UnregisterServer);
                    servers.Add(server, disposable);
                }
            }

            return disposable;
        }

        private void UnregisterServer(RSocketServer server)
        {
            lock (gate)
            {
                if (servers.Remove(server, out var disposable))
                {
                    disposable.Dispose();
                }
            }
        }
    }
}