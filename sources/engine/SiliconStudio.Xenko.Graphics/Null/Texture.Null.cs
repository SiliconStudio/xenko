// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Base class for texture resources.
    /// </summary>
    public partial class Texture
    {
        /// <summary>
        /// Size of texture in pixel.
        /// </summary>
        private int TexturePixelSize
        {
            get
            {
                NullHelper.ToImplement();
                return 16;
            }
        }

        private const int TextureSubresourceAlignment = 4;
        private const int TextureRowPitchAlignment = 1;

        internal bool HasStencil;

        public void Recreate(DataBox[] dataBoxes = null)
        {
            InitializeFromImpl(dataBoxes);
        }

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            NullHelper.ToImplement();
            return false;
        }

        private void InitializeFromImpl(DataBox[] dataBoxes = null)
        {
            NullHelper.ToImplement();
        }

        private void OnRecreateImpl()
        {
            NullHelper.ToImplement();
        }

        private bool IsFlipped()
        {
            NullHelper.ToImplement();
            return false;
        }

        internal static PixelFormat ComputeShaderResourceFormatFromDepthFormat(PixelFormat format)
        {
            NullHelper.ToImplement();
            return format;
        }
    }
} 
#endif
