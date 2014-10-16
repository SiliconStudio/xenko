
using SiliconStudio.Presentation.View;

namespace SiliconStudio.Presentation.Sample2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // The dispatcher service takes care of updating properties, executing commands, etc. in the UI thread.
            var dispatcherService = new DispatcherService(Dispatcher);
            // We instantiate a view model and set it as data context of this window.
            DataContext = new RootViewModel(dispatcherService);
        }
    }
}
