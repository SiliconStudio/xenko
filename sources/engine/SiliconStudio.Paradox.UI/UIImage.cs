// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Class holding all the data required to define an UI image.
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<UIImage>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<UIImage>), Profile = "Asset")]
    public class UIImage : ImageFragment
    {
        internal Vector4 BordersInternal;

        private Vector2 imageIdealSize;

        /// <summary>
        /// Create an instance of <see cref="UIImage"/> with a unique random name.
        /// </summary>
        public UIImage()
            :this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Create an empty <see cref="UIImage"/>
        /// </summary>
        /// <param name="imageName">The name of the UI image</param>
        public UIImage(string imageName)
            : base(imageName)
        {
        }

        /// <summary>
        /// Create an instance of <see cref="UIImage"/> having a unique name from a single <see cref="Texture"/> and initialize the <see cref="Region"/> to the size of the texture.
        /// </summary>
        /// <param name="texture">The texture to use as color</param>
        public UIImage(Texture texture)
            : this(Guid.NewGuid().ToString(), texture, null)
        {
        }

        /// <summary>
        /// Create an instance of <see cref="UIImage"/> from a single color/alpha <see cref="Texture"/> and 
        /// initialize the <see cref="Region"/> to the size of the texture.
        /// </summary>
        /// <param name="imageName">The name of the UI image</param>
        /// <param name="texture">The texture to use as color</param>
        public UIImage(string imageName, Texture texture)
            : this(imageName, texture, null)
        {
        }

        /// <summary>
        /// Create an instance of <see cref="UIImage"/> that takes its color components from the <paramref name="color"/> texture and
        /// its alpha component from the r component of the <paramref name="alpha"/> texture.
        /// The <see cref="Region"/> is initialized with the size of the textures.
        /// </summary>
        /// <param name="imageName">The name of the UI image</param>
        /// <param name="color">The texture to use as color</param>
        /// <param name="alpha">The texture to use as alpha</param>
        /// <exception cref="ArgumentNullException">The provided textures cannot be null</exception>
        /// <exception cref="ArgumentException">The provided textures must have the same size</exception>
        public UIImage(string imageName, Texture color, Texture alpha)
            : base(imageName, color, alpha)
        {
            UpdateIdealSize();
        }

        /// <summary>
        /// Gets or sets size of the unstretchable borders of source image in pixels.
        /// </summary>
        /// <remarks>Borders size are ordered as follows X->Left, Y->Right, Z ->Top, W -> Bottom.</remarks>
        public Vector4 Borders
        {
            get { return BordersInternal; }
            set
            {
                if(value == BordersInternal)
                    return;

                BordersInternal = value;
                HasBorders = BordersInternal.Length() > MathUtil.ZeroTolerance;

                var handler = BorderChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the value indicating if the image has unstretchable borders.
        /// </summary>
        public bool HasBorders { get; private set; }

        /// <summary>
        /// Region of the texture (in pixels) defining the image. 
        /// </summary>
        public override RectangleF Region
        {
            set
            {
                base.Region = value;
                UpdateIdealSize();
            }
        }
        
        /// <summary>
        /// Gets or sets the rotation to apply to the texture region when rendering the <see cref="UIImage"/>
        /// </summary>
        public override ImageOrientation Orientation
        {
            set
            {
                base.Orientation = value;
                UpdateIdealSize();
            }
        }

        /// <summary>
        /// Gets the ideal size of the image in virtual pixel measure.
        /// </summary>
        public Vector2 ImageIdealSize
        {
            get { return imageIdealSize; }
            private set
            {
                if (value == imageIdealSize)
                    return;

                imageIdealSize = value;

                var handler = IdealSizeChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        private void UpdateIdealSize()
        {
            if (Orientation == ImageOrientation.AsIs)
                ImageIdealSize = new Vector2(RegionInternal.Width, RegionInternal.Height);
            else
                ImageIdealSize = new Vector2(RegionInternal.Height, RegionInternal.Width);
        }

        internal event EventHandler<EventArgs> BorderChanged;
        internal event EventHandler<EventArgs> IdealSizeChanged;
    }
}