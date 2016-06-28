// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A <see cref="ContentControl"/> decorating its <see cref="ContentControl.Content"/> with a background image.
    /// </summary>
    public class ContentDecorator : ContentControl
    {
        /// <summary>
        /// The key to the NotPressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<ISpriteProvider> BackgroundImagePropertyKey = new PropertyKey<ISpriteProvider>("BackgroundImageKey", typeof(ContentDecorator), DefaultValueMetadata.Static<ISpriteProvider>(null));
        
        /// <summary>
        /// Gets or sets the image that the button displays when pressed
        /// </summary>
        public ISpriteProvider BackgroundImage
        {
            get { return DependencyProperties.Get(BackgroundImagePropertyKey); }
            set { DependencyProperties.Set(BackgroundImagePropertyKey, value); }
        }

        public ContentDecorator()
        {
            DrawLayerNumber += 1; // (decorator design image)
        }
    }
}
