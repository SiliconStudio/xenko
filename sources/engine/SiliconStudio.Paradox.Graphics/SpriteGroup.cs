// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Represent of group of <see cref="Sprite"/>
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<SpriteGroup>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<SpriteGroup>))]
    public class SpriteGroup : ImageGroup<Sprite>
    {
    }
}