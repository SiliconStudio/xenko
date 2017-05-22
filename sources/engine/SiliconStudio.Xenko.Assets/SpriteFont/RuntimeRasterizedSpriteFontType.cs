// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("RuntimeRasterizedSpriteFontType")]
    [Display("Runtime Rasterized")]
    public class RuntimeRasterizedSpriteFontType : SpriteFontTypeBase
    {
        /// <inheritdoc/>
        [DataMember(30)]
        [DataMemberRange(MathUtil.ZeroTolerance, 2)]
        [DefaultValue(20)]
        [Display("Default Size")]
        public override float Size { get; set; } = 20;

        /// <inheritdoc/>
        [DataMember(110)]
        [DefaultValue(FontAntiAliasMode.Default)]
        [Display("Anti alias")]
        public override FontAntiAliasMode AntiAlias { get; set; } = FontAntiAliasMode.Default;
    }
}
