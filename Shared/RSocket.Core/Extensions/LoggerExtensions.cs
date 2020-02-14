using System;
using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace RSocket.Core.Extensions
{
    internal static class LoggerExtensions
    {
        [Conditional("DEBUG")]
        public static void FrameReceived(this ILogger logger, WebSocketReceiveResult result)
        {
            logger.LogDebug($"Frame received (count = {result.Count}, last = {result.EndOfMessage})");
        }

        [Conditional("DEBUG")]
        public static void SocketDataReceived(this ILogger logger, WebSocketReceiveResult result)
        {
            logger.LogDebug($"WebSocket data received (count = {result.Count}, end-of-message = {result.EndOfMessage})");
        }

        [Conditional("DEBUG")]
        public static void SocketDataSend(this ILogger logger, ReadOnlyMemory<byte> data, bool endOfMessage)
        {
            logger.LogDebug($"WebSocket data sent (count = {data.Length}, end-of-message = {endOfMessage})");
        }

        [Conditional("DEBUG")]
        public static void WaitingForSend(this ILogger logger)
        {
            logger.LogDebug("Waiting for send");
        }

        [Conditional("DEBUG")]
        public static void WaitingForClose(this ILogger logger, WebSocket socket)
        {
            logger.LogDebug("Waiting for close");
        }

        [Conditional("DEBUG")]
        public static void CloseTimedOut(this ILogger logger, WebSocket socket)
        {
            logger.LogDebug("WebSocket close timed out");
        }

        [Conditional("DEBUG")]
        public static void ErrorWritingFrame(this ILogger logger, Exception exception)
        {
            logger.LogError(exception, "Error writing frame");
        }

        [Conditional("DEBUG")]
        public static void FrameSent(this ILogger logger, WebSocketReceiveResult result)
        {
            logger.LogDebug($"Frame received (count = {result.Count}, last = {result.EndOfMessage})");
        }
    }
}