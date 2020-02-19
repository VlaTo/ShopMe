using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using LibraProgramming.Serialization.Hessian.Core;
using LibraProgramming.Serialization.Hessian.Core.Extensions;

namespace LibraProgramming.Serialization.Hessian
{
    internal sealed class HessianSerializationScheme
    {
        public Type ObjectType
        {
            get;
        }

        public ISerializationElement Element
        {
            get;
        }

        private HessianSerializationScheme(Type objectType, ISerializationElement element)
        {
            ObjectType = objectType;
            Element = element;
        }

        public static HessianSerializationScheme CreateFromType(Type type, IObjectSerializerFactory factory)
        {
            var catalog = new Dictionary<Type, ISerializationElement>();
            var element = CreateSerializationElement(type, catalog, factory);

            return new HessianSerializationScheme(type, element);
        }

        public void Serialize(HessianOutputWriter writer, object graph, HessianSerializationContext context)
        {
            Element.Serialize(writer, graph, context);
        }

        public object Deserialize(HessianInputReader reader, HessianSerializationContext context)
        {
            return Element.Deserialize(reader, context);
        }

        private static ISerializationElement CreateSerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            if (IsSimpleType(type))
            {
                var serializer = factory.GetSerializer(type);
                return new ValueElement(type, serializer);
            }

            if (IsTypedArray(type))
            {
                return BuildArraySerializationElement(type, catalog, factory);
            }

            if (IsTypedList(type))
            {
                return BuildListSerializationElement(type, catalog, factory);
            }

            if (IsTypedCollection(type))
            {
                return BuildCollectionSerializationElement(type, catalog, factory);
            }

            if (IsTypedEnumerable(type))
            {
                return BuildEnumerableSerializationElement(type, catalog, factory);
            }

            return BuildClassSerializationElement(type, catalog, factory);
        }

        private static ISerializationElement BuildClassSerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            if (catalog.TryGetValue(type, out var existing))
            {
                return existing;
            }

            var contract = type.GetCustomAttribute<DataContractAttribute>();

            if (null == contract)
            {
                throw new HessianSerializerException($"The type: \'{type.Name}\' is not marked as serializable");
            }

            var properties = new List<PropertyElement>();
            var element = new ObjectElement(type, properties);

            catalog.Add(type, element);

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var attribute = property.GetCustomAttribute<DataMemberAttribute>();

                if (null == attribute)
                {
                    continue;
                }

                if (!property.CanRead || !property.CanWrite)
                {
                    continue;
                }

                properties.Add(new PropertyElement(
                    property,
                    CreateSerializationElement(property.PropertyType, catalog, factory)
                ));
            }

            properties.Sort(new ObjectPropertyComparer());

            return element;
        }

        private static ISerializationElement BuildArraySerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            if (1 != type.GetArrayRank())
            {
                throw new HessianSerializerException();
            }

            var elementType = type.GetElementType();

            if (typeof(object) == elementType)
            {
                // untyped
            }

            return new FixedLengthTypedListElement(
                elementType.MakeArrayType(1),
                CreateSerializationElement(elementType, catalog, factory)
            );
        }

        private static ISerializationElement BuildListSerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            var elementTypes = type.GetGenericArguments();
            return new FixedLengthTypedListElement(
                typeof(IList<>).MakeGenericType(elementTypes[0]),
                CreateSerializationElement(elementTypes[0], catalog, factory)
            );
        }

        private static ISerializationElement BuildCollectionSerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            var elementTypes = type.GetGenericArguments();
            return new FixedLengthTypedListElement(
                typeof(ICollection<>).MakeGenericType(elementTypes[0]),
                CreateSerializationElement(elementTypes[0], catalog, factory)
            );
        }

        private static ISerializationElement BuildEnumerableSerializationElement(Type type, IDictionary<Type, ISerializationElement> catalog, IObjectSerializerFactory factory)
        {
            var elementTypes = type.GetGenericArguments();
            return new VariableLengthTypedListElement(
                typeof(IEnumerable<>).MakeGenericType(elementTypes[0]),
                CreateSerializationElement(elementTypes[0], catalog, factory)
            );
        }

        private static bool IsSimpleType(Type type) => type.IsValueType || type.IsEnum || type.IsPrimitive;

        private static bool IsTypedArray(Type type) => type.IsArray && type.HasElementType;

        private static bool IsTypedList(Type type)
        {
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                if (typeof(IList<>) == definition)
                {
                    return true;
                }
            }

            return type.GetInterfaces().Any(IsTypedList);
        }

        private static bool IsTypedCollection(Type type)
        {
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                if (typeof(ICollection<>) == definition)
                {
                    return true;
                }
            }

            return type.GetInterfaces().Any(IsTypedCollection);
        }

        private static bool IsTypedEnumerable(Type type)
        {
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                if (typeof(IEnumerable<>) == definition)
                {
                    return true;
                }
            }

            return type.GetInterfaces().Any(IsTypedEnumerable);
        }
    }
}