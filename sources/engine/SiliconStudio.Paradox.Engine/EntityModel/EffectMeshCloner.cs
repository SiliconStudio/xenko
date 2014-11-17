// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    internal class EffectMeshCloner : DataSerializer<EffectMesh>
    {
        public override void Serialize(ref EffectMesh obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(obj.Effect);
                stream.Write(obj.Mesh);
            }
            else
            {
                var effect = stream.Read<Effect>();
                var effectMeshData = stream.Read<Mesh>();
                obj = new EffectMesh(effect, effectMeshData);
            }
        }
    }
}