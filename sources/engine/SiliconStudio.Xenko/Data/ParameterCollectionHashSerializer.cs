// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Data
{
    public class ParameterCollectionHashSerializer : ClassDataSerializer<ParameterCollection>
    {
        private DataSerializer<ParameterKey> parameterKeySerializer;

        public override void Initialize(SerializerSelector serializerSelector)
        {
            parameterKeySerializer = serializerSelector.GetSerializer<ParameterKey>();
        }

        public override void Serialize(ref ParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
        {
            foreach (var parameter in parameterCollection.ParameterKeyInfos)
            {
                // CompilerParameters should only contain permutation parameters
                if (parameter.Key.Type != ParameterKeyType.Permutation)
                    continue;

                parameterKeySerializer.Serialize(parameter.Key, stream);

                var value = parameterCollection.ObjectValues[parameter.BindingSlot];
                parameter.Key.SerializeHash(stream, value);
            }
        }
    }
}
