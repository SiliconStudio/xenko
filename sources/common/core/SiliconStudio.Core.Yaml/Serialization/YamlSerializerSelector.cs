using System;
using System.Collections.Generic;
using System.Threading;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml.Serialization
{
    public class YamlSerializerSelector
    {
        private readonly Dictionary<Type, IYamlSerializable> serializers = new Dictionary<Type, IYamlSerializable>();
        private readonly List<IYamlSerializableFactory> factories = new List<IYamlSerializableFactory>();
        private readonly ReaderWriterLockSlim serializerLock = new ReaderWriterLockSlim();

        public YamlSerializerSelector()
        {

        }

        public void AddSerializer(Type type, IYamlSerializable serializer)
        {
            serializers[type] = serializer;
        }

        public void AddSerializerFactory(IYamlSerializableFactory factory)
        {
            factories.Add(factory);
        }

        internal IYamlSerializable GetSerializer(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            IYamlSerializable serializer;

            // First try, with just a read lock
            serializerLock.EnterReadLock();
            var found = serializers.TryGetValue(typeDescriptor.Type, out serializer);
            serializerLock.ExitReadLock();

            if (!found)
            {
                // Not found, let's take exclusive lock and try again
                serializerLock.EnterWriteLock();
                if (!serializers.TryGetValue(typeDescriptor.Type, out serializer))
                {
                    foreach (var factory in factories)
                    {
                        serializer = factory.TryCreate(context, typeDescriptor);
                        if (serializer != null)
                        {
                            serializers.Add(typeDescriptor.Type, serializer);
                            break;
                        }
                    }
                }
                serializerLock.ExitWriteLock();
            }

            if (serializer == null)
            {
                throw new InvalidOperationException($"Unable to find a serializer for the type [{typeDescriptor.Type}]");
            }

            return serializer;
        }
    }
}
