using System;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ShopMe.Web.Service.Extensions
{
    internal static class LoggerExtensions
    {
        public static void ReportIncomingConnection(this ILogger logger, ConnectionInfo connection)
        {
            logger.LogInformation(
                $"Incoming connection \"{connection.Id}\" peer {connection.RemoteIpAddress}:{connection.RemotePort}"
            );
        }

        public static void ReportIncomingMessage(this ILogger logger, string message, string messagePrefix = null, int number = default)
        {
            var text = new StringBuilder();

            text.Append("Incoming message: ");

            if (false == String.IsNullOrEmpty(messagePrefix))
            {
                text.AppendFormat("[{0}] ", messagePrefix);
            }

            text.AppendFormat("\"{0}\" ({1})", message, number);

            logger.LogInformation(text.ToString());
        }

        public static void ReportOutgoingMessage(this ILogger logger, string message, string messagePrefix = null, int number = default)
        {
            var text = new StringBuilder();

            text.Append("Outgoing message: ");

            if (false == String.IsNullOrEmpty(messagePrefix))
            {
                text.AppendFormat("[{0}] ", messagePrefix);
            }

            text.AppendFormat("\"{0}\" ({1})", message, number);

            logger.LogInformation(text.ToString());
        }

        public static void ReportSocketAccepted(this ILogger logger, string connectionId, WebSocket socket)
        {
            logger.LogInformation($"Websocket accepted on \"{connectionId}\" sub-protocol \"{socket.SubProtocol}\"");
        }

        public static void ReportSocketClose(this ILogger logger, WebSocket socket)
        {
            logger.LogInformation("Websocket closed");
        }

        public static void ReportSocketError(this ILogger logger, WebSocket socket, Exception exception)
        {
            logger.LogCritical(exception, "Websocket closed");
        }

        public static void SocketWaitingForSend(this ILogger logger, WebSocket socket)
        {
        }

        public static void SocketWaitingForClose(this ILogger logger, WebSocket socket)
        {
        }
    }
}