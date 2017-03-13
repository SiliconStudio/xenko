using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Implementation of <see cref="IPropertiesProviderViewModel"/> with a given <see cref="IContentNode"/>.
    /// </summary>
    public class SinglePropertyProvider : IPropertiesProviderViewModel
    {
        private readonly IObjectNode rootNode;

        public SinglePropertyProvider(IObjectNode rootNode)
        {
            this.rootNode = rootNode;
        }


        /// <inheritdoc/>
        public bool CanProvidePropertiesViewModel => true;

        /// <inheritdoc/>
        public IObjectNode GetRootNode() => rootNode;


        /// <inheritdoc/>
        public ExpandReferencePolicy ShouldConstructChildren(IGraphNode graphNode, Index index) => ExpandReferencePolicy.Full;

        /// <inheritdoc/>
        public bool ShouldConstructMember(IMemberNode member, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
        public bool ShouldConstructItem(IObjectNode collection, Index index, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
    }
}
