using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SiliconStudio.BuildEngine.Editor.ViewModel;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public struct OpenDropContextMenuArgs
    {
        public object Control { get; set; }
        public IEnumerable<string> Items { get; set; }
        public ICommand Command { get; set; }
        public IViewModelNode ViewModel { get; set; }
    }

    /// <summary>
    /// Interaction logic for PropertyGridUserControl.xaml
    /// </summary>
    public partial class PropertyGridUserControl
    {
        private readonly ContextMenu dropContextMenu;

        public PropertyGridUserControl()
        {
            InitializeComponent();
            dropContextMenu = FindResource("DropContextMenu") as ContextMenu;
        }

        private void PropertyGridUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (PropertyGridViewModel)DataContext;

            viewModel.Edition.OpenDropContextMenu = new AnonymousCommand(x => BuildEditionViewModel.Dispatcher.Invoke(() =>
                {
                    var args = (OpenDropContextMenuArgs)x;

                    dropContextMenu.DataContext = args;
                    dropContextMenu.ItemsSource = args.Items;
                    ((Control)args.Control).ContextMenu = dropContextMenu;
                    dropContextMenu.IsOpen = true;
                }));
        }
    }
}
