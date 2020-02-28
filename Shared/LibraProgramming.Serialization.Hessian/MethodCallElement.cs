using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace LibraProgramming.Serialization.Hessian
{
    internal class MethodCallElement : ISerializationElement
    {
        public Type ObjectType => throw new NotSupportedException();

        public MethodInfo MethodInfo
        {
            get;
        }

        public IList<ISerializationElement> Arguments
        {
            get;
        }

        public MethodCallElement(MethodInfo methodInfo)
        {
            Arguments = new List<ISerializationElement>();
            MethodInfo = methodInfo;
        }

        public void Serialize(HessianOutputWriter writer, object graph, HessianSerializationContext context)
        {
            if (null == graph)
            {
                writer.WriteNull();
                return;
            }

            using (writer.BeginCall(MethodInfo.Name))
            {
                var parameters = (Array) graph;

                for (var index = 0; index < Arguments.Count; index++)
                {
                    Arguments[index].Serialize(writer, parameters.GetValue(index), context);
                }
            }

            /*var index = context.Instances.IndexOf(graph);

            if (index > -1)
            {
                writer.WriteInstanceReference(index);
                return;
            }

            context.Instances.Add(graph);

            index = context.Classes.IndexOf(ObjectType);

            if (0 > index)
            {

            }*/

            /*using (writer.BeginArray(ObjectType.Name))
            {
                foreach (var item in (IEnumerable)graph)
                {
                    Element.Serialize(writer, item, context);
                }
            }*/
        }

        public object Deserialize(HessianInputReader reader, HessianSerializationContext context)
        {
            throw new NotImplementedException();
        }
    }
}