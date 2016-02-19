// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
using CullFaceMode = OpenTK.Graphics.ES30.StencilFace;
#endif
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public class DepthStencilState
    {
        internal bool DepthBufferWriteEnable;

        private bool depthBufferEnable;
        private bool stencilEnable;
        private byte stencilWriteMask;
        private byte stencilMask;

        private DepthFunction depthFunction;

        private StencilFunction frontFaceStencilFunction;
        private StencilOp frontFaceDepthFailOp;
        private StencilOp frontFaceFailOp;
        private StencilOp frontFacePassOp;

        private StencilFunction backFaceStencilFunction;
        private StencilOp backFaceDepthFailOp;
        private StencilOp backFaceFailOp;
        private StencilOp backFacePassOp;

        private int stencilReference;

        internal DepthStencilState(DepthStencilStateDescription depthStencilStateDescription, bool hasDepthStencilBuffer)
        {
            depthBufferEnable = depthStencilStateDescription.DepthBufferEnable;
            DepthBufferWriteEnable = depthStencilStateDescription.DepthBufferWriteEnable && hasDepthStencilBuffer;

            stencilEnable = depthStencilStateDescription.StencilEnable;
            stencilMask = depthStencilStateDescription.StencilMask;
            stencilWriteMask = depthStencilStateDescription.StencilWriteMask;

            depthFunction = depthStencilStateDescription.DepthBufferFunction.ToOpenGLDepthFunction();

            frontFaceStencilFunction = depthStencilStateDescription.FrontFace.StencilFunction.ToOpenGLStencilFunction();
            frontFaceDepthFailOp = depthStencilStateDescription.FrontFace.StencilDepthBufferFail.ToOpenGL();
            frontFaceFailOp = depthStencilStateDescription.FrontFace.StencilFail.ToOpenGL();
            frontFacePassOp = depthStencilStateDescription.FrontFace.StencilPass.ToOpenGL();

            backFaceStencilFunction = depthStencilStateDescription.BackFace.StencilFunction.ToOpenGLStencilFunction();
            backFaceDepthFailOp = depthStencilStateDescription.BackFace.StencilDepthBufferFail.ToOpenGL();
            backFaceFailOp = depthStencilStateDescription.BackFace.StencilFail.ToOpenGL();
            backFacePassOp = depthStencilStateDescription.BackFace.StencilPass.ToOpenGL();
        }

        public void Apply(int stencilReference)
        {
            if (depthBufferEnable)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(depthFunction);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            GL.DepthMask(DepthBufferWriteEnable);

            if (stencilEnable)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilMask(stencilMask);

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLCORE
                GL.StencilFuncSeparate(StencilFace.Front, frontFaceStencilFunction, stencilReference, stencilWriteMask); // set both faces
                GL.StencilFuncSeparate(StencilFace.Back, backFaceStencilFunction, stencilReference, stencilWriteMask); // override back face
                GL.StencilOpSeparate(StencilFace.Front, frontFaceDepthFailOp, frontFaceFailOp, frontFacePassOp);
                GL.StencilOpSeparate(StencilFace.Back, backFaceDepthFailOp, backFaceFailOp, backFacePassOp);
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                GL.StencilFuncSeparate(CullFaceMode.Front, frontFaceStencilFunction, stencilReference, stencilWriteMask);
                GL.StencilFuncSeparate(CullFaceMode.Back, backFaceStencilFunction, stencilReference, stencilWriteMask);
                GL.StencilOpSeparate(CullFaceMode.Front, frontFaceDepthFailOp, frontFaceFailOp, frontFacePassOp);
                GL.StencilOpSeparate(CullFaceMode.Back, backFaceDepthFailOp, backFaceFailOp, backFacePassOp);
#endif
            }
            else
            {
                GL.Disable(EnableCap.StencilTest);
            }
        }
    }
} 
#endif 
