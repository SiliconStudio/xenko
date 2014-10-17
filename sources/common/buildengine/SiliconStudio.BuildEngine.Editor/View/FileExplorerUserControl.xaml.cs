using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    /// <summary>
    /// Interaction logic for FileExplorerUserControl.xaml
    /// </summary>
    public partial class FileExplorerUserControl
    {
        public static readonly DependencyProperty SelectedFilesProperty = DependencyProperty.Register("SelectedFiles", typeof(ObservableCollection<object>), typeof(FileExplorerUserControl), new PropertyMetadata(new ObservableCollection<object>()));

        /// <summary>
        /// A bindable collection of the files selected in the file list.
        /// </summary>
        /// <remarks>This property is not readonly in order to use bindings but it shall not be written. See https://connect.microsoft.com/VisualStudio/feedback/details/540833/ </remarks>
        public ObservableCollection<object> SelectedFiles
        {
            get { return (ObservableCollection<object>)GetValue(SelectedFilesProperty); }
            set { throw new InvalidOperationException("The SelectedFiles collection is not writable."); }
        }

        /// <summary>
        /// Indicate that the selection is being synchronized between <see cref="SelectedFiles"/> and the <see cref="ListBox.SelectedItems"/> property of the file ListBox.
        /// </summary>
        private bool synchronizingSelection;

        public FileExplorerUserControl()
        {
            InitializeComponent();

            SelectedFiles.CollectionChanged += SelectedFileCollectionChanged;
        }

        private async void ButtonAddSource_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var viewModel = (FileExplorerViewModel)DataContext;
                bool added = await viewModel.AddFolder(dialog.FileName, true);
                if (!added)
                {
                    MessageBox.Show("Unable to add the selected folder. It is either invalid or unreachable.");
                }
            }
        }

        private void SelectedFileCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizingSelection)
                return;

            synchronizingSelection = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                        FileList.SelectedItems.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                        FileList.SelectedItems.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    FileList.SelectedItems.Clear();
                    break;
            }
            synchronizingSelection = false;
        }

        private void FileListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (synchronizingSelection)
                return;

            synchronizingSelection = true;
            foreach (object item in e.AddedItems)
                SelectedFiles.Add(item);
            foreach (object item in e.RemovedItems)
                SelectedFiles.Remove(item);
            synchronizingSelection = false;
        }
    }
}
