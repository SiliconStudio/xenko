using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Implementation of <see cref="IPropertiesProviderViewModel"/> with a given <see cref="IGraphNode"/>.
    /// </summary>
    public class SinglePropertyProvider : IPropertiesProviderViewModel
    {
        private readonly IGraphNode rootNode;

        public SinglePropertyProvider(IGraphNode rootNode)
        {
            this.rootNode = rootNode;
        }


        /// <inheritdoc/>
        public bool CanProvidePropertiesViewModel => true;

        /// <inheritdoc/>
        public IGraphNode GetRootNode() => rootNode;

        /// <inheritdoc/>
        public bool ShouldExpandReference(MemberContent member, ObjectReference reference) => true;

        /// <inheritdoc/>
        public bool ShouldConstructMember(MemberContent content) => true;
    }
}