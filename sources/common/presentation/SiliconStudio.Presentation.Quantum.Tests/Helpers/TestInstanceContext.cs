using SiliconStudio.Presentation.Quantum;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Tests.Helpers
{
    public class TestInstanceContext
    {
        private readonly TestContext context;

        public TestInstanceContext(TestContext context, IGraphNode rootNode)
        {
            this.context = context;
            RootNode = rootNode;
            PropertiesProvider = new Types.TestPropertiesProvider(rootNode);
        }

        public IPropertiesProviderViewModel PropertiesProvider { get; }

        public IGraphNode RootNode { get; }

        public ObservableViewModel CreateViewModel()
        {
            return ObservableViewModel.Create(context.ServiceProvider, PropertiesProvider);
        }
    }
}
