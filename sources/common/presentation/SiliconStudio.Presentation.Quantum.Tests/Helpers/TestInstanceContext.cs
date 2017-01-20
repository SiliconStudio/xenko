using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public class TestInstanceContext
    {
        private readonly TestContext context;

        public TestInstanceContext(TestContext context, IContentNode rootNode)
        {
            this.context = context;
            RootNode = rootNode;
            PropertiesProvider = new Types.TestPropertiesProvider(rootNode);
        }

        public IPropertiesProviderViewModel PropertiesProvider { get; }

        public IContentNode RootNode { get; }

        public GraphViewModel CreateViewModel()
        {
            return GraphViewModel.Create(context.ServiceProvider, PropertiesProvider);
        }
    }
}
