using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public class TestInstanceContext
    {
        private readonly TestContext context;

        public TestInstanceContext(TestContext context, IObjectNode rootNode)
        {
            this.context = context;
            RootNode = rootNode;
            PropertiesProvider = new Types.TestPropertiesProvider(rootNode);
        }

        public IPropertiesProviderViewModel PropertiesProvider { get; }

        public IObjectNode RootNode { get; }

        public GraphViewModel CreateViewModel()
        {
            return GraphViewModel.Create(context.ServiceProvider, PropertiesProvider);
        }
    }
}
