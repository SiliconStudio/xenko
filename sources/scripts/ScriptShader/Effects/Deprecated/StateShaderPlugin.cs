// Copyright (c) 2011 Silicon Studio

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Basic shader plugin built directly from shader source file.
    /// </summary>
    public class StateShaderPlugin : ShaderPlugin<RenderPassPlugin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateShaderPlugin"/> class.
        /// </summary>
        public StateShaderPlugin()
        {
        }
        public bool UseBlendState { get; set; }

        public bool UseRasterizerState { get; set; }

        public bool UseDepthStencilState { get; set; }

        public override void SetupResources(EffectMesh effectMesh)
        {
            if (UseBlendState)
                Effect.Parameters.RegisterParameter(BlendStateKey);
            
            if (UseRasterizerState)
                Effect.Parameters.RegisterParameter(RasterizerStateKey);

            if (UseDepthStencilState)
                Effect.Parameters.RegisterParameter(DepthStencilStateKey);
        }
   }
}
