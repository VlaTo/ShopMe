using System;
using System.Runtime.Serialization;

namespace ShopMe.Models
{
    [Serializable]
    [DataContract(Name = "list")]
    public class ShopListInfo
    {
        [DataMember(Name = "title")]
        public string Title
        {
            get;
            set;
        }
    }
}
