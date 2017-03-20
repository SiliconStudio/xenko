using SiliconStudio.Presentation.Quantum.Presenters;
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
        bool IPropertiesProviderViewModel.ShouldConstructMember(IMemberNode member) => true;

        bool IPropertiesProviderViewModel.ShouldConstructItem(IObjectNode collection, Index index) => true;
    }
}
