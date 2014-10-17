// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Provides methods to retrieve and manipulate graphics adapters.
    /// </summary>
    public partial class GraphicsAdapter
    {
        internal GraphicsAdapter()
        {
            outputs = new [] { new GraphicsOutput() };
        }

        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            // TODO: Check OpenGL version?
            // TODO: ES specific code?
            return true;
        }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return "Default OpenGL Adapter"; }
        }

        /// <summary>
        /// Determines if this instance of GraphicsAdapter is the default adapter.
        /// </summary>
        public bool IsDefaultAdapter
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the vendor identifier.
        /// </summary>
        /// <value>
        /// The vendor identifier.
        /// </value>
        public int VendorId
        {
            get { return 0; }
        }
    }
}

#endif
