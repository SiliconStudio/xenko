// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("OfflineRasterizedSpriteFontType")]
    [Display("Offline Rasterized")]
    public class OfflineRasterizedSpriteFontType : SpriteFontTypeBase
    {
        /// <inheritdoc/>
        [DataMember(30)]
        [DataMemberRange(MathUtil.ZeroTolerance, float.MaxValue)]
        [DefaultValue(20)]
        public override float Size { get; set; } = 20;

        /// <summary>
        ///  Gets or sets the text file referencing which characters to include when generating the static fonts (eg. "ABCDEF...")
        /// </summary>
        /// <userdoc>
        /// The path to a file containing the characters to import from the font source file. This property is ignored when 'IsDynamic' is checked.
        /// </userdoc>
        [DataMember(70)]
        public UFile CharacterSet { get; set; } = new UFile("");

        /// <summary>
        /// Gets or set the additional character ranges to include when generating the static fonts (eg. "/CharacterRegion:0x20-0x7F /CharacterRegion:0x123")
        /// </summary>
        /// <userdoc>
        /// The list of series of character to import from the font source file. This property is ignored when 'IsDynamic' is checked.
        /// Note that this property only represents an alternative way of indicating character to import, the result is the same as using the 'CharacterSet' property.
        /// </userdoc>
        [DataMember(80)]
        [NotNullItems]
        public List<CharacterRegion> CharacterRegions { get; set; } = new List<CharacterRegion>();

        /// <inheritdoc/>
        [DataMember(110)]
        [DefaultValue(FontAntiAliasMode.Default)]
        [Display("Anti alias")]
        public override FontAntiAliasMode AntiAlias { get; set; } = FontAntiAliasMode.Default;

        /// <inheritdoc/>
        [DataMember(120)]
        [DefaultValue(true)]
        [Display("Premultiply")]
        public override bool IsPremultiplied { get; set; } = true;
    }
}
