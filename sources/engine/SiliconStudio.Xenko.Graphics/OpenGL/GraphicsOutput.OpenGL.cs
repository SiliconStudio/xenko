// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL

using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsOutput
    {
        public DisplayMode FindClosestMatchingDisplayMode(GraphicsProfile[] targetProfiles, DisplayMode mode)
        {
            return mode;
        }

        public IntPtr MonitorHandle
        {
            get { return IntPtr.Zero; }
        }

        private void InitializeSupportedDisplayModes()
        {
        }

        private void InitializeCurrentDisplayMode()
        {
            currentDisplayMode = null;
        }
    }
}
#endif