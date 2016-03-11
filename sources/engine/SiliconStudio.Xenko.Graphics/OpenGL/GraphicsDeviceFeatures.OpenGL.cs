// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
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
using SiliconStudio.Xenko.Graphics.OpenGL;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Features supported by a <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <remarks>
    /// This class gives also features for a particular format, using the operator this[dxgiFormat] on this structure.
    /// </remarks>
    public partial struct GraphicsDeviceFeatures
    {

        internal GraphicsDeviceFeatures(GraphicsDevice deviceRoot)
        {
            mapFeaturesPerFormat = new FeaturesPerFormat[256];

            HasSRgb = true;

            using (deviceRoot.UseOpenGLCreationContext())
            {
                Vendor = GL.GetString(StringName.Vendor);
                Renderer = GL.GetString(StringName.Renderer);
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                SupportedExtensions = GL.GetString(StringName.Extensions).Split(' ');
#else
                int numExtensions;
                GL.GetInteger(GetPName.NumExtensions, out numExtensions);
                SupportedExtensions = new string[numExtensions];
                for (int extensionIndex = 0; extensionIndex < numExtensions; ++extensionIndex)
                {
                    SupportedExtensions[extensionIndex] = GL.GetString(StringNameIndexed.Extensions, extensionIndex);
                }
#endif
            }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            var isOpenGLES3 = deviceRoot.currentVersionMajor >= 3;

            deviceRoot.HasDepth24 = isOpenGLES3 || SupportedExtensions.Contains("GL_OES_depth24");
            deviceRoot.HasPackedDepthStencilExtension = SupportedExtensions.Contains("GL_OES_packed_depth_stencil");
            deviceRoot.HasExtTextureFormatBGRA8888 = SupportedExtensions.Contains("GL_EXT_texture_format_BGRA8888")
                                       || SupportedExtensions.Contains("GL_APPLE_texture_format_BGRA8888");
            deviceRoot.HasRenderTargetFloat = SupportedExtensions.Contains("GL_EXT_color_buffer_float");
            deviceRoot.HasRenderTargetHalf = SupportedExtensions.Contains("GL_EXT_color_buffer_half_float");
            deviceRoot.HasVAO = isOpenGLES3 || SupportedExtensions.Contains("GL_OES_vertex_array_object");
            deviceRoot.HasTextureRG = isOpenGLES3 || SupportedExtensions.Contains("GL_EXT_texture_rg");

            HasSRgb = isOpenGLES3 || SupportedExtensions.Contains("GL_EXT_sRGB");

            // Compute shaders available in OpenGL ES 3.1
            HasComputeShaders = isOpenGLES3 && deviceRoot.currentVersionMinor >= 1;
            HasDoublePrecision = false;
            
            // TODO: from 3.1: draw indirect, separate shader object
            // TODO: check tessellation & geometry shaders: GL_ANDROID_extension_pack_es31a
#else
            deviceRoot.HasVAO = true;

            deviceRoot.HasDXT = SupportedExtensions.Contains("GL_EXT_texture_compression_s3tc");

            // Compute shaders available in OpenGL 4.3
            HasComputeShaders = deviceRoot.versionMajor >= 4 && deviceRoot.versionMinor >= 3;
            HasDoublePrecision = SupportedExtensions.Contains("GL_ARB_vertex_attrib_64bit");

            // TODO: from 4.0: tessellation, draw indirect
            // TODO: from 4.1: separate shader object
#endif

            deviceRoot.HasDepthClamp = SupportedExtensions.Contains("GL_ARB_depth_clamp");

            HasDriverCommandLists = false;
            HasMultiThreadingConcurrentResources = false;

            // TODO: Enum supported formats in mapFeaturesPerFormat

            // Find shader model based on OpenGL version (might need to check extensions more carefully)
            Profile = OpenGLUtils.GetFeatureLevel(deviceRoot.versionMajor, deviceRoot.versionMinor);
            CurrentProfile = OpenGLUtils.GetFeatureLevel(deviceRoot.currentVersionMajor, deviceRoot.currentVersionMinor);
        }
    }
}
#endif