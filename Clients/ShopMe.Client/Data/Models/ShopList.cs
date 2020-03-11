using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ShopMe.Client.Data.Models
{
    [DataContract]
    [Table("shop_list")]
    public sealed class ShopList
    {
        [DataMember(Name = "id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id
        {
            get; 
            set;
        }

        [DataMember(Name = "title")]
        [DataType(DataType.Text)]
        [StringLength(120)]
        public string Title
        {
            get; 
            set;
        }

        [DataMember(Name = "active")]
        public bool IsActive
        {
            get; 
            set;
        }

        [DataMember(Name = "created")]
        [DataType(DataType.DateTime)]
        public DateTime Created
        {
            get; 
            set;
        }
    }
}