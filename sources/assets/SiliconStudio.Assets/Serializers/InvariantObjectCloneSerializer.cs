// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A fake serializer used for cloning invariant objects. 
    /// Instead of actually cloning the invariant object, this serializer store it in a list of the context and restore when deserializing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataSerializerGlobal(typeof(InvariantObjectCloneSerializer<string>), Profile = "AssetClone")]
    public class InvariantObjectCloneSerializer<T> : DataSerializer<T>
    {
        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
            var invariantObjectList = stream.Context.Get(AssetCloner.InvariantObjectListProperty);
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(invariantObjectList.Count);
                invariantObjectList.Add(obj);
            }
            else
            {
                var index = stream.Read<Int32>();
                obj = (T)invariantObjectList[index];
            }
        }
    }
}