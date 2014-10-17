// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Represent of group of <see cref="Sprite"/>
    /// </summary>
    [DataConverter(AutoGenerate = false, ContentReference = true)]
    public class SpriteGroup : ImageGroup<Sprite>
    {
    }
}