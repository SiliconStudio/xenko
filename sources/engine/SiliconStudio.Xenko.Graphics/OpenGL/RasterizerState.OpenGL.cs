// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
using System;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    public partial class RasterizerState
    {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        private PolygonMode polygonMode;
#endif

        private RasterizerState(GraphicsDevice device, RasterizerStateDescription rasterizerStateDescription) : base(device)
        {
            Description = rasterizerStateDescription;

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            polygonMode = Description.FillMode == FillMode.Solid ? PolygonMode.Fill : PolygonMode.Line;
#endif
            
            // TODO: DepthBiasClamp and various other properties are not fully supported yet
            if (Description.DepthBiasClamp != 0.0f) throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();
            return true;
        }

        public void Apply()
        {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            GL.PolygonMode(MaterialFace.FrontAndBack, polygonMode);
#endif
            GL.PolygonOffset(Description.DepthBias, Description.SlopeScaleDepthBias);
            
            if (Description.CullMode == CullMode.None)
                GL.Disable(EnableCap.CullFace);
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(GetCullMode(Description.CullMode));
            }


            if (Description.ScissorTestEnable)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);
        }

        public static CullFaceMode GetCullMode(CullMode cullMode)
        {
            switch (cullMode)
            {
                case CullMode.Front:
                    return CullFaceMode.Front;
                case CullMode.Back:
                    return CullFaceMode.Back;
                case CullMode.None:
                default:
                    return CullFaceMode.FrontAndBack;
            }
        }
    }
} 
#endif 
