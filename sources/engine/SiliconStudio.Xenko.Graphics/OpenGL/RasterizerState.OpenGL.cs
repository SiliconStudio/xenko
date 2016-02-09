// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    class RasterizerState
    {
        private bool scissorTestEnable;

        private bool needCulling;
        private CullFaceMode cullMode;
        private int depthBias;
        private float slopeScaleDepthBias;
        private FrontFaceDirection frontFaceDirection;

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private PolygonMode polygonMode;
#endif

        internal RasterizerState(RasterizerStateDescription rasterizerStateDescription)
        {
            scissorTestEnable = rasterizerStateDescription.ScissorTestEnable;

            needCulling = rasterizerStateDescription.CullMode != CullMode.None;
            cullMode = GetCullMode(rasterizerStateDescription.CullMode);

            frontFaceDirection =
                rasterizerStateDescription.FrontFaceCounterClockwise
                ? FrontFaceDirection.Cw
                : FrontFaceDirection.Ccw;

            depthBias = rasterizerStateDescription.DepthBias;
            slopeScaleDepthBias = rasterizerStateDescription.SlopeScaleDepthBias;

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            polygonMode = rasterizerStateDescription.FillMode == FillMode.Solid ? PolygonMode.Fill : PolygonMode.Line;
#endif

            // TODO: DepthBiasClamp and various other properties are not fully supported yet
            if (rasterizerStateDescription.DepthBiasClamp != 0.0f) throw new NotSupportedException();
        }

        public void Apply()
        {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GL.PolygonMode(MaterialFace.FrontAndBack, polygonMode);
#endif
            GL.PolygonOffset(depthBias, slopeScaleDepthBias);

            GL.FrontFace(frontFaceDirection);

            if (needCulling)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(cullMode);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }

            if (scissorTestEnable)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);
        }

        private static CullFaceMode GetCullMode(CullMode cullMode)
        {
            switch (cullMode)
            {
                case CullMode.Front:
                    return CullFaceMode.Front;
                case CullMode.Back:
                    return CullFaceMode.Back;
                default:
                    return CullFaceMode.Back; // not used if CullMode.None
            }
        }
    }
} 
#endif 
