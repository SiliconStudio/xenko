using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class MetadataViewModel : AnchorableViewModel
    {
        public ObservableViewModelNode MetadataRootNode { get { return (ObservableViewModelNode)contextUI.Root; } }

        private readonly ViewModelContext context;
        private readonly ViewModelContext contextUI = new ViewModelContext();
        private readonly ViewModelState state = new ViewModelState();

        private ObservableCollection<object> selectedFiles;
        private FileViewModel selectedFile;

        public ObservableCollection<ObjectMetadataViewModel> Metadata = new ObservableCollection<ObjectMetadataViewModel>();

        public ICommand AddKeyCommand { get; set; }
 
        public MetadataViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "Metadata";
            DefaultPane = "DefaultPropertiesPane";

            context = new ViewModelContext(edition.GuidContainer);

            if (edition.SelectedFiles != null)
            {
                selectedFiles = Edition.SelectedFiles;
                selectedFiles.CollectionChanged += SelectedFilesCollectionChanged;
            }
            edition.PropertyChanged += BuildEditionPropertyChanged;
            AddKeyCommand = new AsyncCommand(param => {
                if (param != null && selectedFile != null)
                {
                    BuildEditionViewModel.GetActiveSession().AddMetadataKey(((MetadataKeyViewModel)param).Key, BuildSessionViewModel.GeneratePathUsingAliasVariable(selectedFile));
                    Refresh();
                }
            });
            Refresh();
        }

        private void BuildEditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedFiles")
            {
                if (selectedFiles != null)
                    selectedFiles.CollectionChanged -= SelectedFilesCollectionChanged;

                selectedFiles = Edition.SelectedFiles;

                if (selectedFiles != null)
                    selectedFiles.CollectionChanged += SelectedFilesCollectionChanged;
            }
        }

        private void SelectedFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (selectedFiles != null && selectedFiles.Count == 1)
            {
                selectedFile = (FileViewModel)selectedFiles.First();
                string selectedFilePath = BuildSessionViewModel.GeneratePathUsingAliasVariable(selectedFile);
                IEnumerable<IObjectMetadata> result = BuildEditionViewModel.GetActiveSession().RetrieveMetadata(selectedFilePath);
                Metadata.Clear();

                foreach (IObjectMetadata metadata in result)
                {
                    Metadata.Add(new ObjectMetadataViewModel(metadata));
                }

                var root = new ViewModelNode("Metadata", new NullViewModelContent());
                context.RegisterViewModel(root);
                foreach (ObjectMetadataViewModel metadata in Metadata)
                {
                    IViewModelNode metadataNode = ViewModelConstructor.AddValueNode(root, metadata.Key.Name, metadata);
                    IViewModelNode property = ViewModelConstructor.AddPropertyNode(metadataNode, "MetadataValue");
                    ObjectMetadataViewModel metadataClosure = metadata;
                    ViewModelConstructor.AddCommandNode(property, "Remove", (viewModel, parameter) => BuildEditionViewModel.GetActiveSession().RemoveMetadataKey(metadataClosure.Key, selectedFilePath));
                }

                context.Root = root;

                OnPropertyChanging("MetadataRootNode");
                ObservableViewModelNode.ForceRefresh(contextUI, context, state);
                OnPropertyChanged("MetadataRootNode");
            }
        }
    }
}
