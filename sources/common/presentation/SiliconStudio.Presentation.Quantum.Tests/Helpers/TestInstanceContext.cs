using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public class TestInstanceContext
    {
        private readonly TestContainerContext context;

        public TestInstanceContext(TestContainerContext context, IObjectNode rootNode)
        {
            this.context = context;
            RootNode = rootNode;
            PropertyProvider = new Types.TestPropertyProvider(rootNode);
        }

        public IPropertyProviderViewModel PropertyProvider { get; }

        public IObjectNode RootNode { get; }

        public INodePresenterFactory Factory => context.GraphViewModelService.NodePresenterFactory;

        public GraphViewModel CreateViewModel()
        {
            return GraphViewModel.Create(context.ServiceProvider, new[] { PropertyProvider });
        }
    }
}
