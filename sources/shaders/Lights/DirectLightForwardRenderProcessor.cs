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
        /// <param name="services">The services.</param>
        /// <param name="modelRenderer">The model renderer.</param>
        public DirectLightForwardRenderProcessor(IServiceRegistry services, ModelRenderer modelRenderer)
            : base(services, modelRenderer)
        {
            RegisterLightGroupProcessor<LightDirectional>(new LightDirectionalGroupRenderProcessor());
        }
    }
}
