using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Helper base class for writing <see cref="IPipelinePlugin"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PipelinePlugin<T> : IPipelinePlugin where T : RootRenderFeature
    {
        protected T RenderFeature;
        private bool unregisterRenderFeature;
        private List<RenderStageSelector> renderStageSelectors = new List<RenderStageSelector>();
        private List<RootEffectRenderFeature.ProcessPipelineStateDelegate> postProcessPipelineStates = new List<RootEffectRenderFeature.ProcessPipelineStateDelegate>();

        /// <inheritdoc/>
        public virtual void Load(PipelinePluginContext context)
        {
            RenderFeature = context.RenderSystem.RenderFeatures.OfType<T>().FirstOrDefault();
            if (RenderFeature == null)
            {
                RenderFeature = CreateRenderFeature(context);
                if (RenderFeature == null)
                    throw new InvalidOperationException($"RenderFeature {typeof(T).Name} didn't exist or could not be created when processing pipeline plugin ${GetType().Name}");

                // Register top level render feature
                context.RenderSystem.RenderFeatures.Add(RenderFeature);
                unregisterRenderFeature = true;
            }
        }

        /// <summary>
        /// If requested <see cref="RootRenderFeature"/> doesn't exist, provide a way to instantiate it.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual T CreateRenderFeature(PipelinePluginContext context)
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual void Unload(PipelinePluginContext context)
        {
            // Clear RenderStageSelector
            foreach (var renderStageSelector in renderStageSelectors)
                RenderFeature.RenderStageSelectors.Remove(renderStageSelector);
            renderStageSelectors.Clear();

            // Clear PostProcessPipelineState
            foreach (var postProcessPipelineState in postProcessPipelineStates)
                ((RootEffectRenderFeature)(RootRenderFeature)RenderFeature).PostProcessPipelineState -= postProcessPipelineState;
            postProcessPipelineStates.Clear();

            if (unregisterRenderFeature)
                context.RenderSystem.RenderFeatures.Remove(RenderFeature);
            unregisterRenderFeature = false;
            RenderFeature = null;
        }

        protected void RegisterRenderStageSelector(RenderStageSelector renderStageSelector)
        {
            RenderFeature.RenderStageSelectors.Add(renderStageSelector);
            renderStageSelectors.Add(renderStageSelector);
        }

        protected void RegisterPostProcessPipelineState(RootEffectRenderFeature.ProcessPipelineStateDelegate postProcessPipelineState)
        {
            ((RootEffectRenderFeature)(RootRenderFeature)RenderFeature).PostProcessPipelineState += postProcessPipelineState;
            postProcessPipelineStates.Add(postProcessPipelineState);
        }
    }
}