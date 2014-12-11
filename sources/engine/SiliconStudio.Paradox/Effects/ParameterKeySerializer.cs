// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Effects
{
    public class ParameterKeySerializer<T> : DataSerializer<ParameterKey<T>>
    {
        public override void Serialize(ref ParameterKey<T> obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(obj.Name);
                stream.Write(obj.Length);
            }
            else
            {
                var parameterName = stream.ReadString();
                var parameterLength = stream.ReadInt32();
                obj = (ParameterKey<T>)ParameterKeys.FindByName(parameterName);

                // If parameter could not be found, create one matching this type.
                if (obj == null)
                {
                    var metadata = new ParameterKeyValueMetadata<T>();
                    obj = new ParameterKey<T>(parameterName, parameterLength, metadata);
                    ParameterKeys.Merge(obj, null, parameterName);
                }
            }
        }
    }
}