using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    /// <summary>
    /// Interaction logic for FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer
    {
        public FileExplorer()
        {
            InitializeComponent();
        }

        private async void ButtonAddSource_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var viewModel = (FileExplorerViewModel)DataContext;

            dialog.InitialDirectory = viewModel.BaseFolder;

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                bool added = await viewModel.AddFolder(dialog.FileName, true);
                if (!added)
                {
                    MessageBox.Show("Unable to add the selected folder. It is either invalid or unreachable.");
                }
            }
        }
    }
}
