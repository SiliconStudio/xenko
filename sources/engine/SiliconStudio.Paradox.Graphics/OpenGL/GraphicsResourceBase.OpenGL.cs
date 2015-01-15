// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public partial class GraphicsResourceBase
    {
        internal int resourceId;

        private void Initialize()
        {
        }
        
        internal int ResourceId
        {
            get { return resourceId; }
        }

        /// <summary>
        /// Called when graphics device has been detected to be internally destroyed.
        /// </summary>
        protected internal virtual void OnDestroyed()
        {
            // If GL context is lost, set resource id to 0, as OpenGL destroys everything with it
            resourceId = 0;
        }

        /// <summary>
        /// Called when graphics device has been recreated.
        /// </summary>
        /// <returns>True if item transitionned to a <see cref="GraphicsResourceLifetimeState.Active"/> state.</returns>
        protected internal virtual bool OnRecreate()
        {
            return false;
        }

        protected virtual void DestroyImpl()
        {
        }
    }
}
 
#endif
