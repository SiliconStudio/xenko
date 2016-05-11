// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a Windows button control, which reacts to the Click event.
    /// </summary>
    [DataContract(nameof(Button))]
    [DebuggerDisplay("Button - Name={Name}")]
    public class Button : ButtonBase
    {
        private ISpriteProvider pressedImage;
        private ISpriteProvider notPressedImage;
        private ISpriteProvider mouseOverImage;

        public Button()
        {
            DrawLayerNumber += 1; // (button design image)
            Padding = new Thickness(10, 5, 10, 7);
        }

        /// <summary>
        /// Function triggered when one of the <see cref="PressedImage"/> and <see cref="NotPressedImage"/> images are invalidated.
        /// This function can be overridden in inherited classes.
        /// </summary>
        protected virtual void OnAspectImageInvalidated()
        {
        }

        /// <summary>
        /// Gets or sets the image that the button displays when pressed.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider PressedImage
        {
            get { return pressedImage; }
            set
            {
                if (pressedImage == value)
                    return;

                pressedImage = value;
                OnAspectImageInvalidated();
            }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when not pressed.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider NotPressedImage
        {
            get { return notPressedImage; }
            set
            {
                if (notPressedImage == value)
                    return;

                notPressedImage = value;
                OnAspectImageInvalidated();
            }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when the mouse is over it.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider MouseOverImage
        {
            get { return mouseOverImage; }
            set
            {
                if (mouseOverImage == value)
                    return;

                mouseOverImage = value;
                OnAspectImageInvalidated();
            }
        }
    }
}
