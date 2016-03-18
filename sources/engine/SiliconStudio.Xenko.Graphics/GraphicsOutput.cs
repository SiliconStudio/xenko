// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsOutput : ComponentBase
    {
        private readonly object lockModes = new object();
        private readonly GraphicsAdapter adapter;
        private DisplayMode currentDisplayMode;
        private DisplayMode[] supportedDisplayModes;
        private readonly Rectangle desktopBounds;

        /// <summary>
        /// Default constructor to initialize fields that are not explicitly set to avoid warnings at compile time.
        /// </summary>
        internal GraphicsOutput()
        {
            adapter = null;
            supportedDisplayModes = null;
            desktopBounds = Rectangle.Empty;
        }

        /// <summary>
        /// Gets the current display mode.
        /// </summary>
        /// <value>The current display mode.</value>
        public DisplayMode CurrentDisplayMode
        {
            get
            {
                lock (lockModes)
                {
                    if (currentDisplayMode == null)
                    {
                        InitializeCurrentDisplayMode();
                    }
                }
                return currentDisplayMode;
            }
        }

        /// <summary>
        /// Returns a collection of supported display modes for this <see cref="GraphicsOutput"/>.
        /// </summary>
        public DisplayMode[] SupportedDisplayModes
        {
            get
            {
                lock (lockModes)
                {
                    if (supportedDisplayModes == null)
                    {
                        InitializeSupportedDisplayModes();
                    }
                }
                return supportedDisplayModes;
            }
        }

        /// <summary>
        /// Gets the desktop bounds of the current output.
        /// </summary>
        public Rectangle DesktopBounds
        {
            get
            {
                return desktopBounds;
            }
        }

        /// <summary>
        /// Gets the adapter this output is attached.
        /// </summary>
        /// <value>The adapter.</value>
        public GraphicsAdapter Adapter
        {
            get { return adapter; }
        }
    }
}
