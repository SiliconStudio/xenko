// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Used as a replacement for a null material stored in a <see cref="SiliconStudio.Xenko.Engine.ModelComponent"/>, only valid at design time
    /// </summary>
    [DataSerializerGlobal(typeof(NullSerializer), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<MaterialNull>))]
    [DataContract("MaterialNull")]
    public sealed class MaterialNull : Material
    {
        // TODO: This concept might be generalized for other kind of types, it would require to plug a bit more deeply into binary serialization
        // in order to support this (something similar to AttachedReferenceManager but for null reference)

        internal sealed class NullSerializer : DataSerializer<MaterialNull>
        {
            public override void Serialize(ref MaterialNull obj, ArchiveMode mode, SerializationStream stream)
            {
                if (stream.Context.SerializerSelector.HasProfile("AssetClone"))
                {
                    // At design time, when performing a clone, we keep the associated id of this instance.
                    if (mode == ArchiveMode.Serialize)
                    {
                        var id = IdentifiableHelper.GetId(obj);
                        stream.Write(id);
                    }
                    else
                    {
                        var id = stream.Read<Guid>();
                        obj = new MaterialNull();
                        IdentifiableHelper.SetId(obj, id);
                    }
                }
                else
                {
                    // For runtime serialization, a MaterialNull becomes null
                    obj = null;
                }
            }
        }
    }
}