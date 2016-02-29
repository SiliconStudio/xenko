// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Data
{
    public class ParameterCollectionHashSerializer : ClassDataSerializer<NextGenParameterCollection>, IDataSerializerInitializer
    {
        private DataSerializer<ParameterKey> parameterKeySerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            parameterKeySerializer = serializerSelector.GetSerializer<ParameterKey>();
        }

        public override void Serialize(ref NextGenParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
        {
            foreach (var parameter in parameterCollection.ParameterKeyInfos)
            {
                if (parameter.Key.Type != ParameterKeyType.Permutation)
                    throw new InvalidOperationException("CompilerParameters should only contain permutation parameters");

                parameterKeySerializer.Serialize(parameter.Key, stream);

                var value = parameterCollection.ObjectValues[parameter.BindingSlot];
                parameter.Key.SerializeHash(stream, value);
            }
        }
    }
}