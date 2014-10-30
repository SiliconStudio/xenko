// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.UI.Controls
{
    /// <summary>
    /// Represents a Windows button control, which reacts to the Click event.
    /// </summary>
    [DebuggerDisplay("Button - Name={Name}")]
    public class Button : ButtonBase
    {
        /// <summary>
        /// The key to the NotPressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<UIImage> NotPressedImagePropertyKey = new PropertyKey<UIImage>("NotPressedImageKey", typeof(Button), DefaultValueMetadata.Static<UIImage>(null), ObjectInvalidationMetadata.New<UIImage>(OnAspectImageInvalidated));

        /// <summary>
        /// The key to the PressedImage dependency property.
        /// </summary>
        public static readonly PropertyKey<UIImage> PressedImagePropertyKey = new PropertyKey<UIImage>("PressedImageKey", typeof(Button), DefaultValueMetadata.Static<UIImage>(null), ObjectInvalidationMetadata.New<UIImage>(OnAspectImageInvalidated));

        /// <summary>
        /// The key to the MouseOverImage dependency property.
        /// </summary>
        public static readonly PropertyKey<UIImage> MouseOverImagePropertyKey = new PropertyKey<UIImage>("MouseOverImageKey", typeof(Button), DefaultValueMetadata.Static<UIImage>(null), ObjectInvalidationMetadata.New<UIImage>(OnAspectImageInvalidated));

        public Button()
        {
            DrawLayerNumber += 1; // (button design image)
            Padding = new Thickness(10, 5, 10, 7);
        }

        private static void OnAspectImageInvalidated(object propertyOwner, PropertyKey<UIImage> propertyKey, UIImage propertyOldValue)
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
        public UIImage PressedImage
        {
            get { return DependencyProperties.Get(PressedImagePropertyKey); }
            set { DependencyProperties.Set(PressedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when not pressed
        /// </summary>
        public UIImage NotPressedImage
        {
            get { return DependencyProperties.Get(NotPressedImagePropertyKey); }
            set { DependencyProperties.Set(NotPressedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when the mouse is over it
        /// </summary>
        public UIImage MouseOverImage
        {
            get { return DependencyProperties.Get(MouseOverImagePropertyKey); }
            set { DependencyProperties.Set(MouseOverImagePropertyKey, value); }
        }
    }
}