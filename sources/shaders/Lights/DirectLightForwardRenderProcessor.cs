// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Class DirectLightForwardRenderProcessor.
    /// </summary>
    public class DirectLightForwardRenderProcessor : DirectLightRenderProcessorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLightForwardRenderProcessor"/> class.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        public DirectLightForwardRenderProcessor(ModelRenderer modelRenderer)
            : base(modelRenderer)
        {
            RegisterLightGroupProcessor<LightDirectional>(new LightDirectionalGroupRenderProcessor());
        }
    }
}
