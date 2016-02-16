// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Data
{
    [DataSerializerGlobal(null, typeof(Dictionary<ParameterKey, object>))]
    public partial class ParameterCollectionSerializer : ClassDataSerializer<ParameterCollection>
    {
        public override void Serialize(ref ParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var parameters = new Dictionary<ParameterKey, object>();
                foreach (var parameter in parameterCollection.InternalValues)
                {
                    parameters.Add(parameter.Key, parameter.Value.Object);
                }
                stream.Write(parameters);
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                var parameters = stream.Read<Dictionary<ParameterKey, object>>();
                foreach (var parameter in parameters)
                {
                    var parameterValue = parameter.Value;
                    parameterCollection.SetObject(parameter.Key, parameterValue);
                }
            }
        }
    }
}