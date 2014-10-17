// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Effects.Data
{
    public partial class ParameterCollectionSerializer : ClassDataSerializer<ParameterCollection>
    {
        public override void Serialize(ref ParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var parameters = new ParameterCollectionData();
                foreach (var parameter in parameterCollection.InternalValues)
                {
                    if (parameterCollection.IsValueOwner(parameter.Value))
                        parameters.Add(parameter.Key, parameter.Value.Object);
                }
                stream.Write(parameters);
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                var parameters = stream.Read<ParameterCollectionData>();
                foreach (var parameter in parameters)
                {
                    var parameterValue = parameter.Value;
                    if (parameterValue is ContentReference)
                        parameterValue = ((ContentReference)parameterValue).ObjectValue;
                    parameterCollection.SetObject(parameter.Key, parameterValue);
                }
            }
        }
    }
}