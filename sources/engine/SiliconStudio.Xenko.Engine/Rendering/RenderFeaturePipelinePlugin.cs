using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class PipelinePlugin<T> : IPipelinePlugin where T : RootRenderFeature
    {
        protected T RenderFeature;
        private bool unregisterRenderFeature;
        private List<RenderStageSelector> renderStageSelectors = new List<RenderStageSelector>();
        private List<RootEffectRenderFeature.ProcessPipelineStateDelegate> postProcessPipelineStates = new List<RootEffectRenderFeature.ProcessPipelineStateDelegate>();

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

        protected virtual T CreateRenderFeature(PipelinePluginContext context)
        {
            return null;
        }

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