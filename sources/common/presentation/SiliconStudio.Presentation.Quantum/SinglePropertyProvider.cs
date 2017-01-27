using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Implementation of <see cref="IPropertiesProviderViewModel"/> with a given <see cref="IContentNode"/>.
    /// </summary>
    public class SinglePropertyProvider : IPropertiesProviderViewModel
    {
        private readonly IContentNode rootNode;

        public SinglePropertyProvider(IContentNode rootNode)
        {
            this.rootNode = rootNode;
        }


        /// <inheritdoc/>
        public bool CanProvidePropertiesViewModel => true;

        /// <inheritdoc/>
        public IContentNode GetRootNode() => rootNode;


        /// <inheritdoc/>
        public ExpandReferencePolicy ShouldExpandReference(IMemberNode member, ObjectReference reference) => ExpandReferencePolicy.Full;

        /// <inheritdoc/>
        public bool ShouldConstructMember(IMemberNode content, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
    }
}