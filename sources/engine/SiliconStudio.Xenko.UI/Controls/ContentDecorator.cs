// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A <see cref="ContentControl"/> decorating its <see cref="ContentControl.Content"/> with a background image.
    /// </summary>
    [DataContract]
    public class ContentDecorator : ContentControl
    {
        /// <summary>
        /// The key to the NotPressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<Sprite> BackgroundImagePropertyKey = new PropertyKey<Sprite>("BackgroundImageKey", typeof(ContentDecorator), DefaultValueMetadata.Static<Sprite>(null));
        
        /// <summary>
        /// Gets or sets the image that the button displays when pressed
        /// </summary>
        public Sprite BackgroundImage
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
