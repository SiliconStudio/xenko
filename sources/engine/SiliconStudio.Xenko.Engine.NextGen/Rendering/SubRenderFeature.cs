using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class SubRenderFeature : RenderFeature
    {
        /// <summary>
        /// Gets root render feature
        /// </summary>
        protected RootRenderFeature RootRenderFeature;

        /// <summary>
        /// Attach this <see cref="SubRenderFeature"/> to a <see cref="RootRenderFeature"/>.
        /// </summary>
        /// <param name="rootRenderFeature"></param>
        internal void AttachRootRenderFeature(RootRenderFeature rootRenderFeature)
        {
            RootRenderFeature = rootRenderFeature;
            RenderSystem = rootRenderFeature.RenderSystem;
        }
    }
}