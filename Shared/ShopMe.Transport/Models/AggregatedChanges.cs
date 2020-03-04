using System;
using System.Runtime.Serialization;

namespace ShopMe.Transport.Models
{
    [DataContract]
    public sealed class ShopListInfo    
    {
        [DataMember(Name = "id")]
        public long Id
        {
            get; 
            set;
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get; 
            set;
        }
    }

    [DataContract]
    public sealed class ChangesCollection
    {
        [DataMember(Name = "added")]
        public ShopListInfo[] Added
        {
            get; 
            set;
        }

        [DataMember(Name = "changed")]
        public ShopListInfo[] Updated
        {
            get; 
            set;
        }

        [DataMember(Name = "removed")]
        public long[] Deleted
        {
            get; 
            set;
        }

        public ChangesCollection()
        {
            Added = Array.Empty<ShopListInfo>();
            Updated = Array.Empty<ShopListInfo>();
            Deleted = Array.Empty<long>();
        }
    }

    [DataContract]
    public sealed class AggregatedChanges
    {
        [DataMember(Name = "refresh")]
        public string RequestRefreshToken
        {
            get; 
            set;
        }

        public ChangesCollection Changes
        {
            get; 
            set;
        }

        public AggregatedChanges()
        {
            Changes = new ChangesCollection();
        }
    }
}