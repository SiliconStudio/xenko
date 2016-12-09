using System.Windows.Threading;
using SiliconStudio.Presentation.Quantum;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Tests.Helpers
{
    public class TestContext
    {
        public TestContext()
        {
            ObservableViewModelService = new ObservableViewModelService();
            ServiceProvider = new ViewModelServiceProvider();
            ServiceProvider.RegisterService(new DispatcherService(Dispatcher.CurrentDispatcher));
            ServiceProvider.RegisterService(ObservableViewModelService);
            NodeContainer = new NodeContainer();
        }

        public ViewModelServiceProvider ServiceProvider { get; }

        public ObservableViewModelService ObservableViewModelService { get; }

        public NodeContainer NodeContainer { get; }

        public TestInstanceContext CreateInstanceContext(object instance)
        {
            var rootNode = NodeContainer.GetOrCreateNode(instance);
            var context = new TestInstanceContext(this, rootNode);
            return context;
        }
    }
}
