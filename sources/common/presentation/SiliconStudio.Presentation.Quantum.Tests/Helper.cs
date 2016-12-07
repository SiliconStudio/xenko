using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using SiliconStudio.Presentation.Quantum;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Tests
{
    public class TestContext
    {
        public TestContext()
        {
            ServiceProvider = new ViewModelServiceProvider();
            ServiceProvider.RegisterService(new DispatcherService(Dispatcher.CurrentDispatcher));
            ServiceProvider.RegisterService(new ObservableViewModelService());
            NodeContainer = new NodeContainer();
        }

        public ViewModelServiceProvider ServiceProvider { get; }

        public NodeContainer NodeContainer { get; }
    }

    public class TestInstanceContext
    {
        public TestInstanceContext(IGraphNode rootNode)
        {
            RootNode = rootNode;
            PropertiesProvider = new Types.TestPropertiesProvider(rootNode);
        }

        public IPropertiesProviderViewModel PropertiesProvider { get; }

        public IGraphNode RootNode { get; }

        public void CreateViewModel()
        {

        }
    }

    public static class Helper
    {
        public static TestContext CreateTestContext()
        {
            var context = new TestContext();
            return context;
        }

        public static TestInstanceContext CreateInstanceContext(TestContext testContext, object instance)
        {
            var rootNode = testContext.NodeContainer.GetOrCreateNode(instance);
            var context = new TestInstanceContext(rootNode);
            return context;
        }
    }
}
