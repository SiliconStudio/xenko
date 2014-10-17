using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    class DirectoryViewModelComparer : IComparer<DirectoryViewModel>
    {
        public int Compare(DirectoryViewModel x, DirectoryViewModel y)
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

    class FileViewModelComparer : IComparer<FileViewModel>
    {
        public int Compare(FileViewModel x, FileViewModel y)
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

    public class DirectoryViewModel : ViewModelBase
    {
        public SortedObservableCollection<DirectoryViewModel> SubDirectories { get { return subDirectories; } }
        private readonly SortedObservableCollection<DirectoryViewModel> subDirectories = new SortedObservableCollection<DirectoryViewModel>(new DirectoryViewModelComparer());

        /// <summary>
        /// Name of the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional alias for the directory name. If different from null, it will replace the directory name when querying <see cref="Path"/>.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Parent directory, or null if it is a root directory.
        /// </summary>
        public DirectoryViewModel Parent { get; set; }

        /// <summary>
        /// Path of the directory, using aliases if available.
        /// </summary>
        public string Path { get { return (Parent != null ? Parent.Path + '/' : "") + (Alias ?? Name); } }

        /// <summary>
        /// Path of the directory, not using aliases.
        /// </summary>
        public string PhysicalPath { get { return Parent != null ? Parent.PhysicalPath + '/' + Name : AbsoluteRootPath; } }

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
        public string AbsoluteRootPath { get { if (!IsRoot) throw new InvalidOperationException("AbsoluteRootPath is valid only for root directories."); return absoluteRootPath; } set { SetValue(ref absoluteRootPath, value, "AbsoluteRootPath"); } }
        private string absoluteRootPath;

        public DirectoryViewModel(DirectoryViewModel parent, string name)
        {
            Parent = parent;
            Name = name ?? "";
            IsExpanded = Parent == null;
        }

        public DirectoryViewModel FindSubDirectory(string name)
        {
            int index = SubDirectories.BinarySearch(name, (d, s) => string.Compare(d.Name, s, StringComparison.Ordinal));
            return index != -1 ? SubDirectories[index] : null;
        }

        public async Task Refresh(IDataExplorer explorer)
        {
            // Directory update
            IEnumerable<string> subdirs = await explorer.EnumerateDirectories(PhysicalPath);
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
                var newSubDir = new DirectoryViewModel(this, subdir);
                SubDirectories.Add(newSubDir);
                await newSubDir.Refresh(explorer);
            }
        }
    }

    public class FileViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public DirectoryViewModel Directory { get; set; }

        public DirectoryViewModel RootDirectory { get { var result = Directory; while (result.Parent != null) result = result.Parent; return result; } }

        public string Path { get { return Directory.Path + '/' + Name; } }

        public FileViewModel(DirectoryViewModel parent, string name)
        {
            Directory = parent;
            Name = name;
        }
    }

    public class FileExplorerViewModel : AnchorableViewModel
    {
        public ObservableCollection<DirectoryViewModel> RootDirectories { get { return rootDirectories; } set { SetValue(ref rootDirectories, value, "RootDirectories"); } }
        private ObservableCollection<DirectoryViewModel> rootDirectories = new ObservableCollection<DirectoryViewModel>();

        public DirectoryViewModel SelectedDirectory { get; set; }
        private readonly IDataExplorer explorer = new FileExplorer();

        public ObservableCollection<DirectoryViewModel> SelectedDirectories { get { return selectedDirectories; } }
        private readonly ObservableCollection<DirectoryViewModel> selectedDirectories = new ObservableCollection<DirectoryViewModel>();

        public SortedObservableCollection<FileViewModel> Files { get { return files; } set { SetValue(ref files, value, "Files"); } }
        private SortedObservableCollection<FileViewModel> files = new SortedObservableCollection<FileViewModel>(new FileViewModelComparer());

        public string BaseFolder { get { return Edition.ActiveSession != null ? Edition.ActiveSession.AbsoluteSourceBaseDirectory : ""; } }

        // TODO: make an interface to replace BuildEditionViewModel so this control can be used for other purposes.
        public FileExplorerViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "File explorer";
            DefaultPane = "DefaultFileExplorerPane";
            selectedDirectories.CollectionChanged += SelectedDirectoriesChanged;
        }

        private async void SelectedDirectoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: the null check is temporary, it should be fixed in the TreeViewEx controll (SelectedDirectory shall not contains null entry)            
            //await RefreshFiles(SelectedDirectories.Select(x => x.Path);
            await RefreshFiles(SelectedDirectories.Select(x => x != null ? x.Path : null).Where(x => x != null));
        }

        public async Task<bool> AddFolder(string absolutePath, bool generateAlias)
        {
            string rootDir = Path.GetFileName(absolutePath);

            if (rootDir != null)
            {
                string alias = null;
                if (generateAlias)
                {
                    alias = rootDir;
                    int counter = 1;
                    foreach (string rootDirName in RootDirectories.Select(x => x.Alias))
                    {
                        if (rootDirName.ToUpperInvariant() == alias.ToUpperInvariant())
                        {
                            alias = rootDir + "_" + counter;
                            counter++;
                        }
                    }
                }
                var newDir = new DirectoryViewModel(null, rootDir) { Alias = alias, AbsoluteRootPath = absolutePath };
                await newDir.Refresh(explorer);
                BuildEditionViewModel.Dispatcher.Invoke(() => RootDirectories.Add(newDir));
                return true;
            }

            return false;
        }

        public async Task<bool> AddFolder(string absolutePath, string alias)
        {
            string rootDir = Path.GetFileName(absolutePath);

            if (rootDir != null)
            {
                var newDir = new DirectoryViewModel(null, rootDir) { Alias = alias, AbsoluteRootPath = absolutePath };
                await newDir.Refresh(explorer);
                BuildEditionViewModel.Dispatcher.Invoke(() => RootDirectories.Add(newDir));
                return true;
            }

            return false;
        }

        public async Task RefreshFiles(IEnumerable<string> relativePaths)
        {
            Files.Clear();
            foreach (DirectoryViewModel directory in relativePaths.Select(GetDirectory).Where(x => x != null))
            {
                IEnumerable<string> fileNames = await explorer.EnumerateData(directory.PhysicalPath, false);
                foreach (string fileName in fileNames)
                {
                    Files.Add(new FileViewModel(directory, fileName));
                }
            }
        }

        private DirectoryViewModel GetDirectory(string relativePath)
        {
            string[] dirs = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (dirs.Length == 0)
                return null;

            DirectoryViewModel result = RootDirectories.SingleOrDefault(x => x.Alias == dirs.First()) ?? RootDirectories.SingleOrDefault(x => x.Name == dirs.First());

            for (int i = 1; i < dirs.Length && result != null; ++i)
            {
                result = result.FindSubDirectory(dirs[i]);
            }

            return result;
        }
    }
}
