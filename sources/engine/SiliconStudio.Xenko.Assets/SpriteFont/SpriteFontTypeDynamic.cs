// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("SpriteFontTypeDynamic")]
    [Display("Runtime Rasterized")]
    public class SpriteFontTypeDynamic : SpriteFontTypeBase
    {
        /// <inheritdoc/>
        [DataMember(110)]
        [DefaultValue(FontAntiAliasMode.Default)]
        [Display("Anti alias")]
        public override FontAntiAliasMode AntiAlias { get; set; } = FontAntiAliasMode.Default;
    }
}
