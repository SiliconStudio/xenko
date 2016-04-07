// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a Windows button control, which reacts to the Click event.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("Button - Name={Name}")]
    public class Button : ButtonBase
    {
        /// <summary>
        /// The key to the NotPressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<Sprite> NotPressedImagePropertyKey = new PropertyKey<Sprite>("NotPressedImageKey", typeof(Button), DefaultValueMetadata.Static<Sprite>(null), ObjectInvalidationMetadata.New<Sprite>(OnAspectImageInvalidated));

        /// <summary>
        /// The key to the PressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<Sprite> PressedImagePropertyKey = new PropertyKey<Sprite>("PressedImageKey", typeof(Button), DefaultValueMetadata.Static<Sprite>(null), ObjectInvalidationMetadata.New<Sprite>(OnAspectImageInvalidated));

        /// <summary>
        /// The key to the MouseOverImage dependency property.
        /// </summary>
        public static readonly PropertyKey<Sprite> MouseOverImagePropertyKey = new PropertyKey<Sprite>("MouseOverImageKey", typeof(Button), DefaultValueMetadata.Static<Sprite>(null), ObjectInvalidationMetadata.New<Sprite>(OnAspectImageInvalidated));

        public Button()
        {
            DrawLayerNumber += 1; // (button design image)
            Padding = new Thickness(10, 5, 10, 7);
        }

        private static void OnAspectImageInvalidated(object propertyOwner, PropertyKey<Sprite> propertyKey, Sprite propertyOldValue)
        {
            var button = (Button)propertyOwner;
            button.OnAspectImageInvalidated();
        }

        /// <summary>
        /// Function triggered when one of the <see cref="PressedImage"/> and <see cref="NotPressedImage"/> images are invalidated.
        /// This function can be overridden in inherited classes.
        /// </summary>
        protected virtual void OnAspectImageInvalidated()
        {
        }
        
        /// <summary>
        /// Gets or sets the image that the button displays when pressed
        /// </summary>
        public Sprite PressedImage
        {
            get { return DependencyProperties.Get(PressedImagePropertyKey); }
            set { DependencyProperties.Set(PressedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when not pressed
        /// </summary>
        public Sprite NotPressedImage
        {
            get { return DependencyProperties.Get(NotPressedImagePropertyKey); }
            set { DependencyProperties.Set(NotPressedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when the mouse is over it
        /// </summary>
        public Sprite MouseOverImage
        {
            get { return DependencyProperties.Get(MouseOverImagePropertyKey); }
            set { DependencyProperties.Set(MouseOverImagePropertyKey, value); }
        }
    }
}
