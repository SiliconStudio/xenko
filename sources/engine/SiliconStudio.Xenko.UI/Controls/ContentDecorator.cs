// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A <see cref="ContentControl"/> decorating its <see cref="ContentControl.Content"/> with a background image.
    /// </summary>
    [DataContract(nameof(ContentDecorator))]
    public class ContentDecorator : ContentControl
    {
        /// <summary>
        /// Gets or sets the background image.
        /// </summary>
        /// <userdoc>The background image.</userdoc>
        [DataMember]
        [Display(category: AppearanceCategory, order: 300)]
        [DefaultValue(null)]
        public ISpriteProvider BackgroundImage { get; set; }

        public ContentDecorator()
        {
            DrawLayerNumber += 1; // (decorator design image)
        }
    }
}
