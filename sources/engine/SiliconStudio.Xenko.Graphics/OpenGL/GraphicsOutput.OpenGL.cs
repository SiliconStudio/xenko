// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
