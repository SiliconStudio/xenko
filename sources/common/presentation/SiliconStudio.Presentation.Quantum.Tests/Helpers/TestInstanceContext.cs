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
            PropertyProvider = new Types.TestPropertyProvider(rootNode);
        }

        public IPropertyProviderViewModel PropertyProvider { get; }

        public IObjectNode RootNode { get; }

        public GraphViewModel CreateViewModel()
        {
            return GraphViewModel.Create(context.ServiceProvider, new[] { PropertyProvider });
        }
    }
}
