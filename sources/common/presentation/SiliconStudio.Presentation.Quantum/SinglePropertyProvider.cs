using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Implementation of <see cref="IPropertyProviderViewModel"/> with a given <see cref="IContentNode"/>.
    /// </summary>
    public class SinglePropertyProvider : IPropertyProviderViewModel
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
        bool IPropertyProviderViewModel.ShouldConstructMember(IMemberNode member) => true;

        bool IPropertyProviderViewModel.ShouldConstructItem(IObjectNode collection, Index index) => true;
    }
}
