using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace SiliconStudio.BuildEngine.Editor.View
{
    /// <summary>
    /// Interaction logic for AssetExplorerUserControl.xaml
    /// </summary>
    public partial class AssetExplorerUserControl
    {
        public static readonly DependencyProperty SelectedAssetsProperty = DependencyProperty.Register("SelectedAssets", typeof(ObservableCollection<object>), typeof(AssetExplorerUserControl), new PropertyMetadata(new ObservableCollection<object>()));

                /// <summary>
        /// A bindable collection of the files selected in the file list.
        /// </summary>
        /// <remarks>This property is not readonly in order to use bindings but it shall not be written. See https://connect.microsoft.com/VisualStudio/feedback/details/540833/ </remarks>
        public ObservableCollection<object> SelectedAssets
        {
            get { return (ObservableCollection<object>)GetValue(SelectedAssetsProperty); }
            set { throw new InvalidOperationException("The SelectedAssets collection is not writable."); }
        }

        /// <summary>
        /// Indicate that the selection is being synchronized between <see cref="SelectedAssets"/> and the <see cref="ListBox.SelectedItems"/> property of the file ListBox.
        /// </summary>
        private bool synchronizingSelection;

        public AssetExplorerUserControl()
        {
            InitializeComponent();

            SelectedAssets.CollectionChanged += SelectedAssetCollectionChanged;
        }

        private async void ButtonAddSource_OnClick(object sender, RoutedEventArgs e)
        {
            //var dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            //var result = dialog.ShowDialog();
            //if (result == CommonFileDialogResult.Ok)
            //{
            //    var viewModel = (AssetExplorerViewModel)DataContext;
            //    bool added = await viewModel.AddFolder(dialog.FileName, true);
            //    if (!added)
            //    {
            //        MessageBox.Show("Unable to add the selected folder. It is either invalid or unreachable.");
            //    }
            //}
        }

        private void SelectedAssetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (synchronizingSelection)
                return;

            synchronizingSelection = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                        AssetList.SelectedItems.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                        AssetList.SelectedItems.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AssetList.SelectedItems.Clear();
                    break;
            }
            synchronizingSelection = false;
        }

        private void AssetListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (synchronizingSelection)
                return;

            synchronizingSelection = true;
            foreach (object item in e.AddedItems)
                SelectedAssets.Add(item);
            foreach (object item in e.RemovedItems)
                SelectedAssets.Remove(item);
            synchronizingSelection = false;
        }
}
}
