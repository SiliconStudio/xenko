// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
namespace SiliconStudio.Paradox.Graphics
{
    public partial class BlendState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlendState"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="blendStateDescription">The blend state description.</param>
        internal BlendState(GraphicsDevice graphicsDevice, BlendStateDescription blendStateDescription)
        {
            Description = blendStateDescription;
        }
    }
}
 
#endif 
