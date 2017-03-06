// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DescriptorPool
    {

        /// <summary>
        /// Initializes new instance of <see cref="DescriptorPool"/> that can handle the various 
        /// <see cref="DescriptorTypeCount"/> from <param name="counts"/> for <param name="graphicsDevice"/>.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="counts">Various Type and corresponding count for descriptors that this instance will handle.</param>
        private DescriptorPool(GraphicsDevice graphicsDevice, DescriptorTypeCount[] counts) : base(graphicsDevice)
        {
            NullHelper.ToImplement();
        }

        /// <summary>
        /// Reset the pool.
        /// </summary>
        public void Reset()
        {
            NullHelper.ToImplement();
        }

        /// <summary>
        /// Recreate the pool.
        /// </summary>
        private void Recreate()
        {
            NullHelper.ToImplement();
        }
    }
}

#endif
