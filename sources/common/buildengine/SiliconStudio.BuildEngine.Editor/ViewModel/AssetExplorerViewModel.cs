using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.Core.IO;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    class AssetFolderViewModelComparer : IComparer<AssetFolderViewModel>
    {
        public int Compare(AssetFolderViewModel x, AssetFolderViewModel y)
        {
            // ReSharper disable StringCompareToIsCultureSpecific
            if (x == null && y == null)
                return 0;

            if (x != null && y != null)
                return x.Name.CompareTo(y.Name);

            return x == null ? -1 : 1;
            // ReSharper restore StringCompareToIsCultureSpecific
        }
    }

    class AssetViewModelComparer : IComparer<AssetViewModel>
    {
        public int Compare(AssetViewModel x, AssetViewModel y)
        {
            // ReSharper disable StringCompareToIsCultureSpecific
            if (x == null && y == null)
                return 0;

            if (x != null && y != null)
                return x.Name.CompareTo(y.Name);

            return x == null ? -1 : 1;
            // ReSharper restore StringCompareToIsCultureSpecific
        }
    }

    public class AssetFolderViewModel : ViewModelBase
    {
        public SortedObservableCollection<AssetFolderViewModel> SubDirectories { get { return subDirectories; } }
        private readonly SortedObservableCollection<AssetFolderViewModel> subDirectories = new SortedObservableCollection<AssetFolderViewModel>(new AssetFolderViewModelComparer());

        /// <summary>
        /// Name of the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parent directory, or null if it is a root directory.
        /// </summary>
        public AssetFolderViewModel Parent { get; set; }

        /// <summary>
        /// Path of the directory, using aliases if available.
        /// </summary>
        public string Path { get { return (Parent != null && Parent.Parent != null ? Parent.Path + '/' : "") + (Parent != null ? Name : ""); } }

        /// <summary>
        /// Indicate wheither this directory is a root directory.
        /// </summary>
        public bool IsRoot { get { return Parent == null; } }

        /// <summary>
        /// Indicate wheither this directory is expanded in the view.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Absolute path to the root folder. This property is valid only for root directories.
        /// </summary>
        public string AbsoluteRootPath { get { if (!IsRoot) throw new InvalidOperationException("AbsoluteRootPath is valid only for root directories."); return absoluteRootPath; } set { absoluteRootPath = value; OnPropertyChanged("AbsoluteRootPath"); } }
        private string absoluteRootPath;

        public AssetFolderViewModel(AssetFolderViewModel parent, string name)
        {
            Parent = parent;
            Name = name ?? "";
            IsExpanded = Parent == null;
        }

        public AssetFolderViewModel FindSubDirectory(string name)
        {
            int index = SubDirectories.BinarySearch(name, (d, s) => string.Compare(d.Name, s, StringComparison.Ordinal));
            return index != -1 ? SubDirectories[index] : null;
        }

        public async Task Refresh(IDataExplorer explorer)
        {
            // Directory update
            IEnumerable<string> subdirs = await explorer.EnumerateDirectories(Path);
            var subdirList = new List<string>(subdirs);
            subdirList.Sort();

            // Removing deleted folders
            for (int i = 0; i < SubDirectories.Count; ++i)
            {
                if (subdirList.BinarySearch(SubDirectories[i].Name) == -1)
                    SubDirectories.RemoveAt(i--);
            }

            // Adding missing folders
            foreach (string subdir in subdirList.Where(x => FindSubDirectory(x) == null))
            {
                var newSubDir = new AssetFolderViewModel(this, subdir);
                BuildEditionViewModel.Dispatcher.Invoke(() => SubDirectories.Add(newSubDir));
                await newSubDir.Refresh(explorer);
            }
        }
    }

    public class AssetViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public AssetFolderViewModel Directory { get; set; }

        public AssetFolderViewModel RootDirectory { get { var result = Directory; while (result.Parent != null) result = result.Parent; return result; } }

        public string Path { get { return Directory.Path + '/' + Name; } }

        public AssetViewModel(AssetFolderViewModel parent, string name)
        {
            Directory = parent;
            Name = name;
        }
    }

    public class AssetExplorerViewModel : AnchorableViewModel
    {
        public ObservableCollection<AssetFolderViewModel> RootDirectories { get { return rootDirectories; } set { rootDirectories = value; OnPropertyChanged("RootDirectories"); } }
        private ObservableCollection<AssetFolderViewModel> rootDirectories = new ObservableCollection<AssetFolderViewModel>();

        public AssetFolderViewModel SelectedDirectory { get; set; }
        private readonly IDataExplorer explorer = new AssetExplorer();

        public ObservableCollection<AssetFolderViewModel> SelectedDirectories { get { return selectedDirectories; } }
        private readonly ObservableCollection<AssetFolderViewModel> selectedDirectories = new ObservableCollection<AssetFolderViewModel>();

        public List<AssetViewModel> AllAssets = new List<AssetViewModel>();

        public SortedObservableCollection<AssetViewModel> VisibleAssets { get { return visibleAssets; } set { visibleAssets = value; OnPropertyChanged("VisibleAssets"); } }
        private SortedObservableCollection<AssetViewModel> visibleAssets = new SortedObservableCollection<AssetViewModel>(new AssetViewModelComparer());

        public string AssetRootFolder { get { return Edition.ActiveSession != null ? Edition.ActiveSession.AbsoluteOutputDirectory : ""; } }

        // TODO: make an interface to replace BuildEditionViewModel so this control can be used for other purposes.
        public AssetExplorerViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "Asset explorer";
            DefaultPane = "DefaultFileExplorerPane";
            RootDirectories.Add(new AssetFolderViewModel(null, "assets"));
            selectedDirectories.CollectionChanged += SelectedDirectoriesChanged;
            Edition.PropertyChanged += EditionPropertyChanged;
        }

        private async void EditionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveSession")
            {
                if (Edition.ActiveSession != null)
                    Edition.ActiveSession.PropertyChanged += EditionPropertyChanged;
                await Refresh();
            }
            if (e.PropertyName == "AbsoluteOutputDirectory")
                await Refresh();
        }

        private async void SelectedDirectoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await RefreshVisibleAssets(SelectedDirectories.Where(x => x != null).ToArray());
        }

        public async Task Refresh()
        {
            BuildEditionViewModel.Dispatcher.Invoke(() => RootDirectories.First().SubDirectories.Clear());
            string assetIndexFile = Path.Combine(AssetRootFolder, "db/index_assets");
            if (!File.Exists(assetIndexFile))
                return;

            ((FileSystemProvider)VirtualFileSystem.ApplicationData).ChangeBasePath(AssetRootFolder);
            ((AssetExplorer)explorer).LoadIndexMap("/data/db/");
            await RootDirectories.First().Refresh(explorer);
            BuildEditionViewModel.Dispatcher.Invoke(() => VisibleAssets.Clear());
        }

        private async Task RefreshVisibleAssets(IEnumerable<AssetFolderViewModel> selectedFolders)
        {
            BuildEditionViewModel.Dispatcher.Invoke(() => VisibleAssets.Clear());
            foreach (AssetFolderViewModel selectedDirectory in selectedFolders)
            {
                IEnumerable<string> assets = await explorer.EnumerateData(selectedDirectory.Path, false);
                foreach (string asset in assets)
                {
                    AssetFolderViewModel directory = selectedDirectory;
                    string ass = asset;
                    BuildEditionViewModel.Dispatcher.Invoke(() => VisibleAssets.Add(new AssetViewModel(directory, ass)));
                }
            }
        }

    }
}
