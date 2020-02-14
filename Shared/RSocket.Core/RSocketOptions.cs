using System;

namespace RSocket.Core
{
    public class RSocketOptions
    {
        public const int DefaultInitialRequest = Int32.MinValue;

        public static readonly RSocketOptions Default;

        public TimeSpan KeepAlive
        {
            get; 
            set;
        }

        public TimeSpan Lifetime
        {
            get; 
            set;
        }

        public string MetadataMimeType
        {
            get;
            set;
        }

        public string DataMimeType
        {
            get; 
            set;
        }

        public int InitialRequestCredit
        {
            get; 
            set;
        }

        public int NextRequestCredit
        {
            get; 
            set;
        }

        public int GetInitialRequest(int initialRequest)
        {
            return DefaultInitialRequest == initialRequest ? InitialRequestCredit : initialRequest;
        }

        static RSocketOptions()
        {
            Default = new RSocketOptions
            {
                KeepAlive = TimeSpan.FromMinutes(1.0d),
                Lifetime = TimeSpan.FromMinutes(3.0d),
                DataMimeType = "binary",
                MetadataMimeType = "binary",
                InitialRequestCredit = 10,
                NextRequestCredit = 10
            };
        }
    }
}