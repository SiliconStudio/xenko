// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK.Graphics;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics.OpenGL
{
    /// <summary>
    /// Converts between feature level and opengl versions
    /// </summary>
    internal static class OpenGLUtils
    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
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
            return GraphicsProfile.Level_9_3;
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
#if SILICONSTUDIO_PLATFORM_ANDROID
        public static GLVersion GetGLVersion(GraphicsProfile graphicsProfile)
        {
            switch (graphicsProfile)
            {
                case GraphicsProfile.Level_9_1:
                case GraphicsProfile.Level_9_2:
                case GraphicsProfile.Level_9_3:
                    return GLVersion.ES2;
                case GraphicsProfile.Level_10_0:
                case GraphicsProfile.Level_10_1:
                    return GLVersion.ES3;
                case GraphicsProfile.Level_11_0:
                case GraphicsProfile.Level_11_1:
                case GraphicsProfile.Level_11_2:
                    return GLVersion.ES31;
                default:
                    throw new ArgumentOutOfRangeException("graphicsProfile");
            }
        }
#endif

        private readonly static Regex MatchOpenGLVersion = new Regex(@"OpenGL\s+ES\s+([0-9\.]+)");

        public static bool GetCurrentGLVersion(out int versionMajor, out int versionMinor)
        {
            versionMajor = 0;
            versionMinor = 0;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            var versionVendorText = GL.GetString(StringName.Version);
            var match = MatchOpenGLVersion.Match(versionVendorText);
            if (!match.Success)
                return false;

            var versionText = match.Groups[1].Value;
            var dotIndex = versionText.IndexOf(".");

            if (!int.TryParse(dotIndex != -1 ? versionText.Substring(0, dotIndex) : versionText, out versionMajor))
            {
                return false;
            }

            if (dotIndex == -1)
            {
                return true;
            }
            return int.TryParse(versionText.Substring(dotIndex + 1), out versionMinor);
#else
            GL.GetInteger(GetPName.MajorVersion, out versionMajor);
            GL.GetInteger(GetPName.MinorVersion, out versionMajor);
            return true;
#endif
        }
    }
}
#endif