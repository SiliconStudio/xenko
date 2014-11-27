// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Features supported by a <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <remarks>
    /// This class gives also features for a particular format, using the operator this[dxgiFormat] on this structure.
    /// </remarks>
    public partial struct GraphicsDeviceFeatures
    {
        internal string Vendor;
        internal string Renderer;
        internal IList<string> SupportedExtensions;

        internal GraphicsDeviceFeatures(GraphicsDevice deviceRoot)
        {
            // Find shader model based on OpenGL version (might need to check extensions more carefully)
            if (deviceRoot.versionMajor > 4)
                Profile = GraphicsProfile.Level_11_0;
            else if (deviceRoot.versionMajor > 3 && deviceRoot.versionMinor > 3)
                Profile = GraphicsProfile.Level_10_0;
            else
                Profile = GraphicsProfile.Level_9_1;

            mapFeaturesPerFormat = new FeaturesPerFormat[256];

            IsProfiled = false;

            using (deviceRoot.UseOpenGLCreationContext())
            {
                Vendor = GL.GetString(StringName.Vendor);
                Renderer = GL.GetString(StringName.Renderer);
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                SupportedExtensions = GL.GetString(StringName.Extensions).Split(' ');
#else
                int numExtensions;
                GL.GetInteger(GetPName.NumExtensions, out numExtensions);
                SupportedExtensions = new string[numExtensions];
                for (int extensionIndex = 0; extensionIndex < numExtensions; ++extensionIndex)
                {
                    SupportedExtensions[extensionIndex] = GL.GetString(StringName.Extensions, extensionIndex);
                }
#endif
            }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            deviceRoot.HasDepth24 = SupportedExtensions.Contains("GL_OES_depth24");
            deviceRoot.HasPackedDepthStencilExtension = SupportedExtensions.Contains("GL_OES_packed_depth_stencil");
            deviceRoot.HasExtTextureFormatBGRA8888 = SupportedExtensions.Contains("GL_EXT_texture_format_BGRA8888")
                                       || SupportedExtensions.Contains("GL_APPLE_texture_format_BGRA8888");
            deviceRoot.HasVAO = SupportedExtensions.Contains("GL_OES_vertex_array_object");
#else
            deviceRoot.HasVAO = true;
#endif

            // Compute shaders available in OpenGL 4.3
            HasComputeShaders = deviceRoot.versionMajor > 4 && deviceRoot.versionMinor > 3;
            HasDoublePrecision = SupportedExtensions.Contains("GL_ARB_vertex_attrib_64bit");
            HasDriverCommandLists = false;
            HasMultiThreadingConcurrentResources = false;

            // TODO: Enum supported formats in mapFeaturesPerFormat
        }
    }
}
#endif