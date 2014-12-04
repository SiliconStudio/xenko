// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
using System;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
using CullFaceMode = OpenTK.Graphics.ES30.StencilFace;
#endif
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    public partial class DepthStencilState
    {
        private DepthFunction depthFunction;

        private DepthStencilState(GraphicsDevice device, DepthStencilStateDescription depthStencilStateDescription)
            : base(device)
        {
            Description = depthStencilStateDescription;

            depthFunction = Description.DepthBufferFunction.ToOpenGLDepthFunction();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();
            return true;
        }

        public void Apply(int stencilReference)
        {
            if (Description.DepthBufferEnable)
            {
                GL.Enable(EnableCap.DepthTest);
                ApplyDepthMask();
                GL.DepthFunc(depthFunction);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            if (Description.StencilEnable)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilMask(Description.StencilMask);

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLCORE
                GL.StencilFunc(Description.FrontFace.StencilFunction.ToOpenGLStencilFunction(), stencilReference, Description.StencilWriteMask); // set both faces
                GL.StencilFuncSeparate(StencilFace.Back, Description.BackFace.StencilFunction.ToOpenGLStencilFunction(), stencilReference, Description.StencilWriteMask); // override back face
                GL.StencilOpSeparate(StencilFace.Front, Description.FrontFace.StencilDepthBufferFail.ToOpenGL(), Description.FrontFace.StencilFail.ToOpenGL(), Description.FrontFace.StencilPass.ToOpenGL());
                GL.StencilOpSeparate(StencilFace.Back, Description.BackFace.StencilDepthBufferFail.ToOpenGL(), Description.BackFace.StencilFail.ToOpenGL(), Description.BackFace.StencilPass.ToOpenGL());
#elif SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                GL.StencilFuncSeparate(CullFaceMode.Front, Description.FrontFace.StencilFunction.ToOpenGLStencilFunction(), stencilReference, Description.StencilWriteMask);
                GL.StencilFuncSeparate(CullFaceMode.Back, Description.BackFace.StencilFunction.ToOpenGLStencilFunction(), stencilReference, Description.StencilWriteMask);
                GL.StencilOpSeparate(CullFaceMode.Front, Description.FrontFace.StencilDepthBufferFail.ToOpenGL(), Description.FrontFace.StencilFail.ToOpenGL(), Description.FrontFace.StencilPass.ToOpenGL());
                GL.StencilOpSeparate(CullFaceMode.Back, Description.BackFace.StencilDepthBufferFail.ToOpenGL(), Description.BackFace.StencilFail.ToOpenGL(), Description.BackFace.StencilPass.ToOpenGL());
#endif
            }
            else
            {
                GL.Disable(EnableCap.StencilTest);
            }
        }

        internal void ApplyDepthMask()
        {
            GL.DepthMask(Description.DepthBufferWriteEnable && GraphicsDevice.hasDepthStencilBuffer);
        }
    }
} 
#endif 
