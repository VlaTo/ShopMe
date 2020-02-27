using System;
using System.Runtime.Serialization;

namespace ShopMe.Models
{
    [Serializable]
    [DataContract(Name = "list")]
    public class ShopListDescription
    {
        [DataMember(Name = "title")]
        public string Title
        {
            get;
            set;
        }
    }
}
