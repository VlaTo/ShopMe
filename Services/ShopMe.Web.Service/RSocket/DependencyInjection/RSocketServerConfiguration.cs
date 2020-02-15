using RSocket.Core;

namespace ShopMe.Web.Service.RSocket.DependencyInjection
{
    internal class RSocketServerConfiguration
    {
        public IRSocketTransport Transport
        {
            get;
        }

        public RSocketOptions Options
        {
            get;
        }

        public RSocketServerConfiguration(IRSocketTransport transport)
        {
            Transport = transport;
            Options = new RSocketOptions();
        }
    }
}