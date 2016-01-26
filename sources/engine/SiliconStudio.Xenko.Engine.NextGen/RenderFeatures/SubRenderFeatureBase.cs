using SiliconStudio.Xenko.Shaders.Compiler;

namespace RenderArchitecture
{
    public abstract class SubRenderFeature : RenderFeature
    {
        /// <summary>
        /// Gets root render feature
        /// </summary>
        protected RootRenderFeature RootRenderFeature;

        /// <summary>
        /// Attach this <see cref="SubRenderFeature"/> to a <see cref="RenderArchitecture.RootRenderFeature"/>.
        /// </summary>
        /// <param name="rootRenderFeature"></param>
        internal void AttachRootRenderFeature(RootRenderFeature rootRenderFeature)
        {
            RootRenderFeature = rootRenderFeature;
        }
    }
}