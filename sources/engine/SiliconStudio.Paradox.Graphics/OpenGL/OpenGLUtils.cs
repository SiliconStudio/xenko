// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Graphics.OpenGL
{
    /// <summary>
    /// Converts between feature level and opengl versions
    /// </summary>
    internal static class OpenGLUtils
    {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        public static IEnumerable<int> GetGLVersions(GraphicsProfile[] graphicsProfiles)
        {
            if (graphicsProfiles != null && graphicsProfiles.Length > 0)
            {
                foreach (var profile in graphicsProfiles)
                {
                    if (profile >= GraphicsProfile.Level_10_0)
                        yield return 3;
                    else
                        yield return 2;
                }
            }
            else
            {
                yield return 2;
            }
        }

        public static void GetGLVersion(GraphicsProfile graphicsProfile, out int major, out int minor)
        {
            switch (graphicsProfile)
            {
                case GraphicsProfile.Level_9_1:
                case GraphicsProfile.Level_9_2:
                case GraphicsProfile.Level_9_3:
                    major = 2;
                    minor = 0;
                    return;
                case GraphicsProfile.Level_10_0:
                case GraphicsProfile.Level_10_1:
                    major = 3;
                    minor = 0;
                    return;
                case GraphicsProfile.Level_11_0:
                case GraphicsProfile.Level_11_1:
                case GraphicsProfile.Level_11_2:
                    major = 3;
                    minor = 1;
                    return;
                default:
                    throw new ArgumentOutOfRangeException("graphicsProfile");
            }
        }

        public static GraphicsProfile GetFeatureLevel(int major, int minor)
        {
            if (major >= 3)
            {
                if (minor >= 1)
                    return GraphicsProfile.Level_11_0; // missing tessellation and geometry shaders
                return GraphicsProfile.Level_10_0;
            }
            return GraphicsProfile.Level_9_1;
        }
#else
        public static void GetGLVersion(GraphicsProfile graphicsProfile, out int major, out int minor)
        {
            switch (graphicsProfile)
            {
                case GraphicsProfile.Level_9_1:
                case GraphicsProfile.Level_9_2:
                case GraphicsProfile.Level_9_3:
                    major = 3;
                    minor = 3;
                    return;
                case GraphicsProfile.Level_10_0:
                case GraphicsProfile.Level_10_1:
                    major = 4;
                    minor = 3;
                    return;
                case GraphicsProfile.Level_11_0:
                case GraphicsProfile.Level_11_1:
                case GraphicsProfile.Level_11_2:
                    major = 4;
                    minor = 4;
                    return;
                default:
                    throw new ArgumentOutOfRangeException("graphicsProfile");
            }
        }

        public static GraphicsProfile GetFeatureLevel(int major, int minor)
        {
            if (major >= 4)
            {
                if (minor >= 4)
                    return GraphicsProfile.Level_11_0;
                if (minor >= 3)
                    return GraphicsProfile.Level_10_0;
            }
            return GraphicsProfile.Level_9_1;
        }
#endif
    }
}
#endif