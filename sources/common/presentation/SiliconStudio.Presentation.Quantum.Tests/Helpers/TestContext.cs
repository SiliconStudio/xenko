using System.Windows.Threading;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public class TestContext
    {
        public TestContext()
        {
            GraphViewModelService = new GraphViewModelService();
            ServiceProvider = new ViewModelServiceProvider();
            ServiceProvider.RegisterService(new DispatcherService(Dispatcher.CurrentDispatcher));
            ServiceProvider.RegisterService(GraphViewModelService);
            NodeContainer = new NodeContainer();
        }

        public ViewModelServiceProvider ServiceProvider { get; }

        public GraphViewModelService GraphViewModelService { get; }

        public NodeContainer NodeContainer { get; }

        public TestInstanceContext CreateInstanceContext(object instance)
        {
            var rootNode = NodeContainer.GetOrCreateNode(instance);
            var context = new TestInstanceContext(this, rootNode);
            return context;
        }
    }
}
