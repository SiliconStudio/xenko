// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("SpriteFontTypeSignedDistanceField")]
    [Display("Signed Distance Field")]
    public class SpriteFontTypeSignedDistanceField : SpriteFontTypeBase
    {
        public SpriteFontTypeSignedDistanceField()
        {
            CharacterRegions = new List<CharacterRegion>() { new CharacterRegion(' ', (char)127) };
        }

        /// <inheritdoc/>
        [DataMember(30)]
        [DefaultValue(16.0f)]
        public override float Size { get; set; } = 16.0f;

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
    }
}
