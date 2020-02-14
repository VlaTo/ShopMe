using System;
using System.Collections.Generic;

namespace RSocket.Core.Transports
{
    public sealed class WebSocketOptions
    {
        public TimeSpan CloseTimeout
        {
            get;
        }

        public Func<IList<string>, string> SubProtocolSelector
        {
            get;
        }

        public WebSocketOptions(TimeSpan closeTimeout, Func<IList<string>, string> subProtocolSelector = null)
        {
            CloseTimeout = closeTimeout;
            SubProtocolSelector = subProtocolSelector;
        }
    }
}