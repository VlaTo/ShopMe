using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using LibraProgramming.Serialization.Hessian.Core;

namespace LibraProgramming.Serialization.Hessian
{
    public sealed class HessianCall : HessianPacket
    {
        private readonly MethodInfo methodInfo;
        private readonly HessianSerializerSettings settings;
        private HessianSerializationScheme scheme;

        public HessianCall(MethodInfo methodInfo, HessianSerializerSettings settings = null)
        {
            this.methodInfo = methodInfo;
            this.settings = settings ?? DefaultHessianSerializerSettings.Instance;
        }

        public void WriteCall(Stream stream, params object[] arguments)
        {
            if (null == stream)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var serializationScheme = GetSerializationScheme();

            using (var writer = new HessianOutputWriter(stream))
            {
                var context = new HessianSerializationContext();
                serializationScheme.Serialize(writer, arguments, context);
            }
        }

        private HessianSerializationScheme GetSerializationScheme()
        {
            if (null == scheme)
            {
                var factory = new HessianObjectSerializerFactory();
                scheme = HessianSerializationScheme.CreateFromMethod(methodInfo, factory);
            }

            return scheme;
        }
    }
}