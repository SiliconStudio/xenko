// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a background to an <see cref="Entity"/>.
    /// </summary>
    [DataContract("BackgroundComponent")]
    [Display(96, "Background")]
    [DefaultEntityComponentRenderer(typeof(BackgroundComponentRenderer))]
    [DefaultEntityComponentProcessor(typeof(BackgroundComponentProcessor))]
    public class BackgroundComponent : EntityComponent
    {
        public static PropertyKey<BackgroundComponent> Key = new PropertyKey<BackgroundComponent>("Key", typeof(BackgroundComponent));

        /// <summary>
        /// Gets or sets the texture to use as background
        /// </summary>
        /// <userdoc>The texture to use as background</userdoc>
        [DataMember(10)]
        [Display("Texture")]
        public Texture Texture { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}