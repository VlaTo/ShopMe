using System;

namespace LibraProgramming.Serialization.Hessian.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CollectionElementTypeAttribute : Attribute
    {
        public Type ElementType
        {
            get;
        }

        public CollectionElementTypeAttribute(Type elementType)
        {
            ElementType = elementType;
        }
    }
}