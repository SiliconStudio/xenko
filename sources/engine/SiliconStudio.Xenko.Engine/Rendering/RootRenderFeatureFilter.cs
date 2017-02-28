using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines which <see cref="RenderObject"/> gets accepted in a <see cref="RootRenderFeature"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class RootRenderFeatureFilter
    {
        /// <summary>
        /// If the given <paramref name="renderObject"/> is accepted, returns true, otherwise false.
        /// </summary>
        /// <param name="renderObject"></param>
        /// <param name="accept"></param>
        public abstract bool Accept(RenderObject renderObject);
    }
}