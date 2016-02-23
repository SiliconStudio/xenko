// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Data
{
    public class ParameterCollectionHashSerializer : ClassDataSerializer<ParameterCollection>, IDataSerializerInitializer
    {
        private DataSerializer<ParameterKey> parameterKeySerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            parameterKeySerializer = serializerSelector.GetSerializer<ParameterKey>();
        }

        public override void Serialize(ref ParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
        {
            foreach (var parameter in parameterCollection.InternalValues)
            {
                parameterKeySerializer.Serialize(parameter.Key, stream);
                parameter.Value.SerializeHash(stream);
            }
        }
    }
}