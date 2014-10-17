using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.BuildEngine.Editor.View;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Behaviors;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class BuildSessionViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The root step of the build script
        /// </summary>
        public ListBuildStep RootStep { get { return rootStep; } set { SetValue(ref rootStep, value, "RootStep"); } }
        private ListBuildStep rootStep;

        /// <summary>
        /// The root node presented as a collection of ViewModelReference (even though it should contain a single item being the view model of <see cref="RootStep"/>). Used for the TreeViewEx control.
        /// </summary>
        public ObservableCollection<ViewModelReference> RootNodes { get { return rootNodes; } set { SetValue(ref rootNodes, value, "RootNode"); } }
        private ObservableCollection<ViewModelReference> rootNodes;
        
        /// <summary>
        /// The currently selected node
        /// </summary>
        public ViewModelReference SelectedNode { get { return selectedNode; } set { SetValue(ref selectedNode, value, UpdateRelationToSelectedStep, "SelectedNode"); } }
        private ViewModelReference selectedNode;

        /// <summary>
        /// The root node presented as a collection of ViewModelReference (even though it should contain a single item being the view model of <see cref="RootStep"/>). Used for the TreeViewEx control.
        /// </summary>
        public ObservableCollection<object> SelectedNodes { get { return selectedNodes; } set { SetValue(ref selectedNodes, value, "SelectedNodes"); } }
        private ObservableCollection<object> selectedNodes;

        public ICommand AddSelectedBuildStepOrCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        /// <summary>
        /// The absolute path of the saved script file. Can be null if the script has not been saved yet
        /// </summary>
        public string ScriptPath { get { return scriptPath; } set { SetValue(ref scriptPath, value, "ScriptPath", "ScriptFolder", "ScriptName"); } }
        private string scriptPath;

        /// <summary>
        /// The absolute path to the folder of the saved script file. Can be null if the script has not been saved yet
        /// </summary>
        public string ScriptFolder { get { return PathExt.IsValidPath(ScriptPath) ? Path.GetDirectoryName(ScriptPath) : null; } }

        /// <summary>
        /// Name of the script. If the script has already been saved, it is the file name. Otherwise, it is <see cref="UnsavedScriptName"/>
        /// </summary>
        public string ScriptName { get { return PathExt.IsValidPath(ScriptPath) ? Path.GetFileName(ScriptPath) : UnsavedScriptName; } }

        /// <summary>
        /// The source base directory to use for the BuildEngine. Can be absolute or relative to the <see cref="ScriptPath"/>. Every other path must be either absolute or relative to the working directory.
        /// </summary>
        public string SourceBaseDirectory { get { return sourceBaseDirectory; } set { SetValue(ref sourceBaseDirectory, value, "SourceBaseDirectory"); } }
        private string sourceBaseDirectory;

        /// <summary>
        /// The source base directory to use for the BuildEngine. Return <see cref="SourceBaseDirectory"/> if it's absolute, or the absolute path combined from <see cref="ScriptPath"/> and <see cref="SourceBaseDirectory"/> if <see cref="SourceBaseDirectory"/> is relative.
        /// </summary>
        public string AbsoluteSourceBaseDirectory { get { return Path.Combine(ScriptFolder ?? "", SourceBaseDirectory ?? ""); } }

        /// <summary>
        /// The folder where the script is built. Can be absolute or relative to <see cref="SourceBaseDirectory"/>.
        /// </summary>
        public string BuildDirectory { get { return buildDirectory; } set { SetValue(ref buildDirectory, value, "BuildDirectory"); } }
        private string buildDirectory;

        /// <summary>
        /// The folder where the script is built. Return <see cref="BuildDirectory"/> if it's absolute, or the absolute path combined from <see cref="ScriptFolder"/> and <see cref="BuildDirectory"/> if <see cref="BuildDirectory"/> is relative.
        /// </summary>
        public string AbsoluteBuildDirectory { get { return Path.Combine(ScriptFolder ?? "", BuildDirectory ?? ""); } }

        /// <summary>
        /// The folder where the output files should be copied. Can be absolute or relative to <see cref="SourceBaseDirectory"/>.
        /// </summary>
        public string OutputDirectory { get { return outputDirectory; } set { SetValue(ref outputDirectory, value, "OutputDirectory"); } }
        private string outputDirectory;

        /// <summary>
        /// The folder where the output files should be copied. Return <see cref="OutputDirectory"/> if it's absolute, or the absolute path combined from <see cref="ScriptFolder"/> and <see cref="OutputDirectory"/> if <see cref="OutputDirectory"/> is relative.
        /// </summary>
        public string AbsoluteOutputDirectory { get { return Path.Combine(ScriptFolder ?? "", OutputDirectory ?? ""); } }

        /// <summary>
        /// List of folders containing source files. Can be absolute or relative to <see cref="SourceBaseDirectory"/>.
        /// </summary>
        public string MetadataDatabaseDirectory { get { return metadataDatabaseDirectory; } set { SetValue(ref metadataDatabaseDirectory, value, "MetadataDatabaseDirectory"); } }
        private string metadataDatabaseDirectory;

        /// <summary>
        /// The folder where the metadata database files are stored. Return <see cref="MetadataDatabaseDirectory"/> if it's absolute, or the absolute path combined from <see cref="ScriptFolder"/> and <see cref="MetadataDatabaseDirectory"/> if <see cref="MetadataDatabaseDirectory"/> is relative.
        /// </summary>
        public string AbsoluteMetadataDatabaseDirectory { get { return Path.Combine(ScriptFolder ?? "", MetadataDatabaseDirectory ?? ""); } }

        /// <summary>
        /// Indicate wheither the metadata database is currently opened or not.
        /// </summary>
        public bool IsMetadataDatabaseOpened { get { return isMetadataDatabaseOpened; } set { SetValue(ref isMetadataDatabaseOpened, value, () => Task.Run(() => ReadAvailableKeysFromDatabase()), "IsMetadataDatabaseOpened"); } }
        private bool isMetadataDatabaseOpened;

        /// <summary>
        /// List of all currently available metadata keys.
        /// </summary>
        public ObservableCollection<MetadataKeyViewModel> AvailableKeys { get; private set; }

        /// <summary>
        /// List of all currently available metadata keys.
        /// </summary>
        public Dictionary<string, ObservableCollection<ObjectMetadataViewModel>> Metadata { get; private set; }

        /// <summary>
        /// List of folders containing source files. Can be absolute or relative to <see cref="SourceBaseDirectory"/>.
        /// </summary>
        private readonly List<SourceFolder> sourceFolders = new List<SourceFolder>();

        public const string UnsavedScriptName = "Unsaved file";

        #endregion Properties

        #region Private fields
        private readonly BuildEditionViewModel edition;
        private readonly ViewModelContext context;
        private readonly ViewModelContext contextUI = new ViewModelContext();
        private readonly ViewModelState state = new ViewModelState();
        private readonly HashSet<Guid> viewModelGuids = new HashSet<Guid>();

        /// <summary>
        /// Root directories for source folders. Used to refresh the <see cref="FileExplorerViewModel"/>.
        /// </summary>
        private ObservableCollection<DirectoryViewModel> rootDirectories;

        /// <summary>
        /// Dictionary containing for each BuildStep an execution identifier. Two BuildSteps with the same identifiers will be triggered at the same time.
        /// </summary>
        private readonly Dictionary<BuildStep, int> executionGroups = new Dictionary<BuildStep, int>();
        /// <summary>
        /// Dictionary containing the list of all BuildStep waited by each WaitBuildStep.
        /// </summary>
        private readonly Dictionary<WaitBuildStep, List<BuildStep>> waitedBuildStepMap = new Dictionary<WaitBuildStep, List<BuildStep>>();
        /// <summary>
        /// Dictionary containing the list of all WaitBuildStep waited by each BuildStep.
        /// </summary>
        private readonly Dictionary<BuildStep, WaitBuildStep[]> waitPredecessorMap = new Dictionary<BuildStep, WaitBuildStep[]>();

        private readonly QueryMetadataProvider provider = new QueryMetadataProvider();
        /// <summary>
        /// The Build Process, when a build is currently running.
        /// </summary>
        private Process buildProcess;
        #endregion Private fields

        public BuildSessionViewModel(BuildEditionViewModel buildEditionViewModel)
        {
            // Create observable collection in UI thread
            BuildEditionViewModel.Dispatcher.Invoke(() =>
            {
                rootNodes = new ObservableCollection<ViewModelReference>();
                selectedNodes = new ObservableCollection<object>();
                rootDirectories = new ObservableCollection<DirectoryViewModel>();
                AvailableKeys = new ObservableCollection<MetadataKeyViewModel>();
            });

            Metadata = new Dictionary<string, ObservableCollection<ObjectMetadataViewModel>>();

            edition = buildEditionViewModel;
            context = new ViewModelContext(edition.GuidContainer);
            context.ChildrenPropertyEnumerators.Add(new BuildStepPropertiesEnumerator(true));
            context.ChildrenPropertyEnumerators.Add(new BuildStepEditionPropertiesEnumerator());
            context.ChildrenPropertyEnumerators.Add(new CommandPropertyInfoEnumerator());
            context.ChildrenPropertyEnumerators.Add(new DropCommandEnumerator(DropSourceFiles));

            RootNodes.Add(null);

            RootStep = new ListBuildStep();

            rootDirectories.CollectionChanged += RootDirectoriesCollectionChanged;
            AddSelectedBuildStepOrCommand = new AsyncCommand(AddSelectedBuildStep);

            AvailableKeys.CollectionChanged += AvailableKeysCollectionChanged;

            CloseCommand = new AnonymousCommand(() => buildEditionViewModel.CloseSession(this));
            BuildDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        private void RootDirectoriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            sourceFolders.Clear();
            foreach (var rootDir in rootDirectories)
            {
                sourceFolders.Add(new SourceFolder { Name = rootDir.Alias, Path = rootDir.AbsoluteRootPath });
            }
        }

        private void AvailableKeysCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: this method might need to be splitted, we only want to refresh settings here
            edition.RefreshSessionDependentAnchorables();
        }

        public async Task<bool> LoadFromScript(BuildScript script)
        {
            SourceBaseDirectory = script.SourceBaseDirectory;
            BuildDirectory = script.BuildDirectory;
            OutputDirectory = script.OutputDirectory;
            MetadataDatabaseDirectory = script.MetadataDatabaseDirectory;

            string sourceRoot = Path.Combine(Path.GetDirectoryName(script.ScriptPath) ?? "", script.SourceBaseDirectory ?? ""); 
            foreach (var sourceFolder in script.SourceFolders)
            {
                await edition.FileExplorer.AddFolder(Path.Combine(sourceRoot, sourceFolder.Value), sourceFolder.Key);
            }

            string currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = sourceRoot;

            script.Execute(RootStep);

            Environment.CurrentDirectory = currentDirectory;

            if (!string.IsNullOrWhiteSpace(MetadataDatabaseDirectory))
            {
                string metadataFile = Path.Combine(AbsoluteMetadataDatabaseDirectory, QueryMetadataProvider.DefaultDatabaseFilename);

                // TODO: await open
                IsMetadataDatabaseOpened = provider.Open(metadataFile, true);
            }

            Refresh();

            await edition.AssetExplorer.Refresh();

            return true;
        }

        private async Task ReadAvailableKeysFromDatabase()
        {
            if (IsMetadataDatabaseOpened)
            {
                IEnumerable<MetadataKey> keys = await provider.FetchAllKeysAsync();
                BuildEditionViewModel.Dispatcher.Invoke(() =>
                {
                    AvailableKeys.Clear();
                    foreach (MetadataKey key in keys)
                    {
                        AvailableKeys.Add(new MetadataKeyViewModel(key));
                    }
                });
            }
        }

        public IEnumerable<IObjectMetadata> RetrieveMetadata(string objectUrl)
        {
            return IsMetadataDatabaseOpened ? provider.Fetch(objectUrl) : Enumerable.Empty<IObjectMetadata>();
        }

        public bool CreateMetadataKey(MetadataKey newKey)
        {
            if (!newKey.IsValid())
                return false;

            if (provider.Fetch(newKey).Any())
                return false;

            bool result = provider.AddKey(newKey);
            if (result)
                Task.Run(() => ReadAvailableKeysFromDatabase());
            return result;
        }

        public bool DeleteMetadataKey(MetadataKey key)
        {
            bool result = provider.RemoveKey(key);
            if (result)
                Task.Run(() => ReadAvailableKeysFromDatabase());
            return result;
        }

        public bool AddMetadataKey(MetadataKey key, string objectUrl)
        {
            var metadata = new ObjectMetadata(objectUrl, key);
            return provider.Write(metadata);
        }

        public bool RemoveMetadataKey(MetadataKey key, string objectUrl)
        {
            var metadata = new ObjectMetadata(objectUrl, key);
            return provider.Delete(metadata);
        }

        public IObjectMetadata UpdateMetadataValue(IObjectMetadata metadata)
        {
            provider.Write(metadata);
            return provider.Fetch(metadata);
        }

        public static BuildStep CreateStep(Type type)
        {
            if (type.IsSubclassOf(typeof(Command)))
            {
                var command = Activator.CreateInstance(type) as Command;
                return new CommandBuildStep(command);
            }

            return Activator.CreateInstance(type) as BuildStep;
        }

        public void DeleteStep(BuildStep buildStep)
        {
            if (!BuildEditionViewModel.IsBuilding())
            {
                buildStep.Parent.RemoveChild(buildStep);
                Refresh();
            }
        }

        /// <summary>
        /// Save the current session in a script file.
        /// </summary>
        /// <param name="path">The file path to use to save the script. If null, the current path stored in property <see cref="ScriptPath"/> will be used.</param>
        /// <returns>true if the file could be properly saved, false if an error occured.</returns>
        public bool Save(string path = null)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var stream = new StreamWriter(path ?? ScriptPath, false);
                var generator = new ScriptGenerator(BuildDirectory, OutputDirectory, SourceBaseDirectory, sourceFolders, MetadataDatabaseDirectory);
                generator.Generate(path ?? ScriptPath, stream, RootStep);
                stream.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveTemporary(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var stream = new StreamWriter(path, false);
                var generator = new ScriptGenerator(AbsoluteBuildDirectory, AbsoluteOutputDirectory, AbsoluteSourceBaseDirectory, sourceFolders, AbsoluteMetadataDatabaseDirectory);
                generator.Generate(path, stream, RootStep);
                stream.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Refresh()
        {
            ViewModelReference[] previouslySelectedNodes = SelectedNodes.Cast<ViewModelReference>().ToArray();
            
            CreateViewModelRecursively(RootStep, true);

            BuildEditionViewModel.Dispatcher.Invoke(() => { SelectedNodes.Clear(); ObservableViewModelNode.ForceRefresh(contextUI, context, state); });

            foreach (Guid guid in viewModelGuids.Where(guid => !contextUI.ViewModelByGuid.ContainsKey(guid)).ToArray())
            {
                viewModelGuids.Remove(guid);
            }

            foreach (KeyValuePair<Guid, IViewModelNode> viewModel in contextUI.ViewModelByGuid)
            {
                if (!viewModelGuids.Contains(viewModel.Key))
                {
                    var descriptionNode = (((dynamic)viewModel.Value).Description) as ObservableViewModelNode;
                    if (descriptionNode != null)
                    {
                        AddDependencyRecursively(descriptionNode, (ObservableViewModelNode)viewModel.Value);
                    }
                    viewModelGuids.Add(viewModel.Key);
                }
            }

            if (RootNodes[0] == null || RootNodes[0].ViewModel != contextUI.Root)
            {
                RootNodes[0] = new ViewModelReference(contextUI.Root.Guid, contextUI);
                RootNodes[0].UpdateGuid(context);
            }

            // Update relation between build steps
            RebuildExecutionGroupList(RootStep);
            RebuildWaitMaps(RootStep);

            if (this == edition.ActiveSession)
                edition.RefreshSessionDependentAnchorables();

            // Refreshing selection
            var references = new List<ViewModelReference>();
            GetReferencesRecursively(RootNodes.First(), references);

            // DispatcherPriority.Background (or any priority lower than DispatcherPriority.DataBind) is required so the bindings are updated prior to the selection and the newly created items are properly in the items source collections
            BuildEditionViewModel.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    // Selected node might have been deleted
                    if (SelectedNode != null && contextUI.ViewModelByGuid.Values.All(x => x != SelectedNode.ViewModel))
                    {
                        SelectedNode =  null;
                    }

                    foreach (var previouslySelectedNode in previouslySelectedNodes.Where(x => x != null))
                    {
                        var matchingRef = references.FirstOrDefault(x => x.Guid == previouslySelectedNode.Guid);

                        if (matchingRef != null && !SelectedNodes.Contains(matchingRef))
                            SelectedNodes.Add(matchingRef);
                    }
                }));
        }

        public void SelectBuildSteps(BuildStep[] buildSteps)
        {
            BuildEditionViewModel.Dispatcher.Invoke(() =>
            {
                foreach (var nodeToSelect in buildSteps.Select(buildStep => contextUI.ViewModelByGuid.Values.FirstOrDefault(x => x.Content.Value == buildStep)))
                {
                    selectedNodes.Add(nodeToSelect);
                }
            });
        }

        public void ClearBuildResult()
        {
            foreach (IViewModelNode node in GetChildrenRecursively(contextUI.Root))
            {
                if (node.Name == "IsRunning")
                    ((ObservableViewModelNode<bool>)node).TValue = false;

                if (node.Name == "ExecutionStatus")
                    ((ObservableViewModelNode<ResultStatus>)node).TValue = ResultStatus.NotProcessed;
            }
        }

        /// <summary>
        /// Execute the script at the given path, using the <see cref="BuildDirectory"/> path for cache and the  <see cref="OutputDirectory"/> as output.
        /// </summary>
        /// <param name="path">Path of the script to execute. If null, <see cref="ScriptPath"/> is used.</param>
        /// <returns>The script execution task.</returns>
        public Task<int> ExecuteScript(string path = null)
        {
            // We combine/override the source path with the script path because we're saving in the temporary folder so relative paths won't work
            //string sourceArg = SourceBaseDirectory != null ? " -s " + Path.Combine(ScriptFolder ?? "", AbsoluteSourceBaseDirectory) : ScriptPath;
            //string buildArg = BuildDirectory != null ? " -b " + AbsoluteBuildDirectory : "";
            //string outputArg = OutputDirectory != null ? " -o " + AbsoluteOutputDirectory : "";
            string monitorArg = " -monitor-pipe=" + edition.MonitorPipeUrl;
            string debugArgs = "";// " -debug -log ";

            ClearBuildResult();

            buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                FileName = "SiliconStudio.BuildEngine.exe",
                //Arguments = (path ?? ScriptPath) + buildArg + outputArg + sourceArg + monitorArg,
                Arguments = (path ?? ScriptPath) + monitorArg + debugArgs,
                UseShellExecute = false
            } };

            var tcs = new TaskCompletionSource<int>();
            buildProcess.EnableRaisingEvents = true;
            buildProcess.Exited += (s, e) => { tcs.TrySetResult(buildProcess.ExitCode); buildProcess = null; };

            try
            {
                buildProcess.Start();
            }
            catch
            {
                buildProcess = null;
                return Task.FromResult(-1);                
            }
            return tcs.Task;
        }

        public bool StopScriptExecution()
        {
            if (buildProcess != null)
            {
                try
                {
                    buildProcess.Kill();
                }
                catch
                {
                    return false;
                }
                foreach (IViewModelNode node in GetChildrenRecursively(contextUI.Root).Where(node => node.Name == "IsRunning"))
                {
                    ((ObservableViewModelNode<bool>)node).TValue = false;
                }
                return true;
            }
            return false;
        }

        public void GenerateTagsForBuildStep()
        {
            UpdateBuildStepTagsRecursively(RootStep);
            Refresh();
        }

        public void ClearBuildStepTags()
        {
            UpdateBuildStepTagsRecursively(RootStep, true);
            Refresh();
        }

        public void CreateMetadataDatabase()
        {
            var path = MetadataDatabaseDirectory;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("The metadata database directory is not set. Please set this directory before creating the database.", "BuildEngine", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var absolutePath = AbsoluteMetadataDatabaseDirectory;

            if (!PathExt.IsValidPath(absolutePath))
            {
                MessageBox.Show("The metadata database directory is invalid. Please set this directory correctly before creating the database.", "BuildEngine", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);

            if (IsMetadataDatabaseOpened)
            {
                provider.Close();
                IsMetadataDatabaseOpened = false;
            }

            string metadataFile = Path.Combine(AbsoluteMetadataDatabaseDirectory, QueryMetadataProvider.DefaultDatabaseFilename);
            if (File.Exists(metadataFile))
            {
                var result = MessageBox.Show("The file " + metadataFile + " already exists. Do you want to open and use this existing database?", "BuildEngine", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes)
                    return;

                IsMetadataDatabaseOpened = provider.Open(metadataFile, false);
                if (IsMetadataDatabaseOpened)
                {
                    Refresh();
                    return;
                }

                result = MessageBox.Show("Unable to open the file " + metadataFile + ". Do you want to override it with an empty database?", "BuildEngine", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    File.Delete(metadataFile);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to delete the file " + metadataFile + ", it is locked by another process. Aborting.", "BuildEngine", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                    return;
                }
            }

            IsMetadataDatabaseOpened = provider.Create(metadataFile);
            Refresh();
        }

        public void AddNewItemToListProperty(IViewModelNode viewModel, PropertyInfo listProperty)
        {
            var owner = viewModel.Parent.Parent.Content.Value;
            var list = listProperty.GetValue(owner);
            var genericType = list.GetType().GetGenericArguments().First();
            object newItem = genericType == typeof(string) ? "" : Activator.CreateInstance(genericType);
            ((IList)list).Add(newItem);
        }

        public void AddNewItemToListProperty(IViewModelNode viewModel, PropertyInfo listProperty, object value)
        {
            var owner = viewModel.Parent.Parent.Content.Value;
            var list = listProperty.GetValue(owner);
            ((IList)list).Add(value);
        }

        public void RemoveItemToListProperty(IViewModelNode viewModel, PropertyInfo listProperty, PropertyInfoViewModelContent content)
        {
            var owner = viewModel.Parent.Parent.Content.Value;
            var list = (IList)listProperty.GetValue(owner);
            list.RemoveAt((int)content.Index[0]);
        }

        private static IEnumerable<IViewModelNode> GetChildrenRecursively(IViewModelNode parent)
        {
            IEnumerable<IViewModelNode> result = parent.Children.Aggregate<IViewModelNode, IEnumerable<IViewModelNode>>(parent.Children, (current, child) => current.Concat(GetChildrenRecursively(child)));
            foreach (var child in parent.Children)
            {
                var reference = child.Content.Value as ViewModelReference;
                if (reference != null && reference.ViewModel != null)
                {
                    result = result.Concat(GetChildrenRecursively(reference.ViewModel));
                }

                var referenceCollection = child.Content.Value as ObservableCollection<ViewModelReference>;
                if (referenceCollection != null)
                {
                    result = referenceCollection.Where(x => x.ViewModel != null).Aggregate(result, (current, refItem) => current.Concat(GetChildrenRecursively(refItem.ViewModel)));
                }
            }
            return result;
        }

        public void GetReferencesRecursively(ViewModelReference parent, List<ViewModelReference> refList)
        {
            if (refList == null) throw new ArgumentNullException("refList");

            if (parent == null || parent.ViewModel == null)
                return;

            refList.Add(parent);

            foreach (var child in parent.ViewModel.Children)
            {
                var reference = child.Content.Value as ViewModelReference;
                if (reference != null && reference.ViewModel != null)
                {
                    GetReferencesRecursively(reference, refList);
                }

                var referenceCollection = child.Content.Value as ObservableCollection<ViewModelReference>;
                if (referenceCollection != null)
                {
                    foreach (var refItem in referenceCollection)
                    {
                        GetReferencesRecursively(refItem, refList);
                    }
                }
            }
        }

        public IViewModelNode GetBuildStepNode(Guid tag)
        {
            if (tag == Guid.Empty)
                return null;

            IViewModelNode tagNode = GetChildrenRecursively(contextUI.Root).SingleOrDefault(x => x.Name == BuildStepPropertiesEnumerator.TagPropertyName && x.Content.Value is Guid && (Guid)x.Content.Value == tag);
            return tagNode != null ? tagNode.Parent : null;
        }

        public void OnSessionActivated()
        {
            edition.FileExplorer.RootDirectories = rootDirectories;
        }

        private void AddSelectedBuildStep(object objectType)
        {
                var type = objectType as Type;
                if (type != null)
                {
                    BuildStep buildStep = CreateStep(type);
                    RootStep.Add(buildStep);
                    if (SelectedNode != null)
                    {
                        var selectedStep = ((ObservableViewModelNode)SelectedNode.ViewModel).ModelNode.Content.Value as BuildStep;
                        var emptyStep = selectedStep as EmptyBuildStep;
                        if (emptyStep != null)
                        {
                            emptyStep.ReplaceWith(buildStep);
                        }
                        else if (selectedStep.CanAddChildren(new[] { buildStep }))
                        {
                            selectedStep.AddChildren(new[] { buildStep });
                        }
                    }
                    Refresh();
                }
        }

        private static void AddDependencyRecursively(ObservableViewModelNode dependentNode, ObservableViewModelNode viewModelNode)
        {
            foreach (ObservableViewModelNode child in viewModelNode.Children.Where(x => x != dependentNode))
            {
                dependentNode.AddDependency(child);
                AddDependencyRecursively(dependentNode, child);
            }
        }

        private void CreateViewModelRecursively(BuildStep buildStep, bool isRoot, List<Guid> guids = null)
        {
            // Replace null reference to buildstep by a dummy EmptyBuildStep instance
            foreach (PropertyInfo propertyInfo in buildStep.GetBuildStepProperties().Where(x => x.GetValue(buildStep) == null))
            {
                propertyInfo.SetValue(buildStep, new EmptyBuildStep());
            }

            if (guids == null)
            {
                guids = new List<Guid>(context.ViewModelByGuid.Keys);
            }

            IViewModelNode objNode = context.GetOrCreateModelView(buildStep, "BuildStep");
            guids.Remove(objNode.Guid);

            foreach (BuildStep child in buildStep.GetChildSteps())
            {
                CreateViewModelRecursively(child, false, guids);
            }

            if (isRoot)
            {
                context.Root = objNode;
                foreach (Guid guid in guids)
                {
                    context.ViewModelByGuid.Remove(guid);
                }
            }
        }

        private void DropSourceFiles(IViewModelNode viewModel, DropCommandParameters drop)
        {
            if (drop.DataType == "SourceFiles")
            {

                var files = ((IEnumerable)drop.Data).Cast<FileViewModel>().ToArray();
                if (files.Length == 1)
                {
                    var value = files.First();
                    if (drop.TargetIndex == -1)
                    {
                        viewModel.Parent.Content.Value = value != null ? GeneratePathUsingAliasVariable(value) : "";
                    }
                    else
                    {
                        int targetIndex = drop.TargetIndex;
                        var list = viewModel.Parent.Content.Value as IList<string>;
                        if (list != null)
                        {
                            list[targetIndex] = value != null ? GeneratePathUsingAliasVariable(value) : "";
                            Refresh();
                        } 
                    }
                }
                else
                {
                    var args = new OpenDropContextMenuArgs
                    {
                        Control = drop.Sender,
                        ViewModel = viewModel,
                        Items = FindPatterns(files, 12),
                    };
                    if (drop.TargetIndex == -1)
                    {
                        args.Command = new AsyncCommand(x => { viewModel.Parent.Content.Value = x; Refresh(); });
                    }
                    else
                    {
                        int targetIndex = drop.TargetIndex;
                        args.Command = new AsyncCommand(x => { var list = viewModel.Parent.Content.Value as IList<string>; if (list != null) { list[targetIndex] = x.ToString(); Refresh(); } });
                    }
                    edition.OpenDropContextMenu.Execute(args);
                }
            }
        }

        private static void AddStepAndChildren(ICollection<BuildStep> list, BuildStep step)
        {
            list.Add(step);
            foreach (BuildStep child in step.GetChildSteps())
            {
                AddStepAndChildren(list, child);
            }
        }

        /// <summary>
        /// Compute, for each WaitBuildStep, the list of all BuildStep waited by a WaitBuildStep, and the list of all WaitBuildStep waited by a BuildStep
        /// </summary>
        /// <param name="step"></param>
        /// <param name="currentWaitList">Current list of waited WaitBuildStep, used for recursivity. Must be null for the initial call</param>
        private void RebuildWaitMaps(BuildStep step, List<WaitBuildStep> currentWaitList = null)
        {
            // Initial call, reset collections
            if (currentWaitList == null)
            {
                currentWaitList = new List<WaitBuildStep>();
                waitedBuildStepMap.Clear();
                waitPredecessorMap.Clear();
            }
            waitPredecessorMap.Add(step, currentWaitList.ToArray());

            var listBuildStep = step as ListBuildStep;
            if (listBuildStep != null)
            {
                int currentWaitCount = currentWaitList.Count;
                foreach (BuildStep child in listBuildStep.Steps)
                {
                    RebuildWaitMaps(child, currentWaitList);

                    var waitBuildStep = child as WaitBuildStep;
                    if (waitBuildStep != null)
                    {
                        var waitedStepList = waitedBuildStepMap.GetOrCreateValue(waitBuildStep);

                        foreach (BuildStep predecessor in listBuildStep.Steps.TakeWhile(x => x != waitBuildStep))
                        {
                            AddStepAndChildren(waitedStepList, predecessor);
                        }
                        AddStepAndChildren(waitedStepList, waitBuildStep);
                        currentWaitList.Add(waitBuildStep);
                    }
                }
                if (currentWaitList.Count > currentWaitCount)
                {
                    currentWaitList.RemoveRange(currentWaitCount, currentWaitList.Count - currentWaitCount);
                }
            }
            var fileEnumerationBuildStep = step as FileEnumerationBuildStep;
            if (fileEnumerationBuildStep != null)
            {
                RebuildWaitMaps(fileEnumerationBuildStep.Template, currentWaitList);
            }
            var outputEnumerationBuildStep = step as OutputEnumerationBuildStep;
            if (outputEnumerationBuildStep != null)
            {
                RebuildWaitMaps(outputEnumerationBuildStep.Template, currentWaitList);
            }
        }

        private void RebuildExecutionGroupList(BuildStep step)
        {
            int nextGroupId = 1;
            executionGroups.Clear();

            var queue = new Queue<Tuple<BuildStep, int>>();
            queue.Enqueue(Tuple.Create(step, 0));

            while (queue.Count > 0)
            {
                var currentStep = queue.Dequeue();
                executionGroups.Add(currentStep.Item1, currentStep.Item2);

                var listBuildStep = currentStep.Item1 as ListBuildStep;
                if (listBuildStep != null)
                {
                    int childGroup = currentStep.Item2;
                    foreach (BuildStep child in listBuildStep.Steps)
                    {
                        queue.Enqueue(Tuple.Create(child, childGroup));

                        if (child is WaitBuildStep)
                            childGroup = nextGroupId++;
                    }
                }
                var fileEnumerationBuildStep = currentStep.Item1 as FileEnumerationBuildStep;
                if (fileEnumerationBuildStep != null)
                {
                    queue.Enqueue(Tuple.Create(fileEnumerationBuildStep.Template, currentStep.Item2));
                }
                var outputEnumerationBuildStep = currentStep.Item1 as OutputEnumerationBuildStep;
                if (outputEnumerationBuildStep != null)
                {
                    queue.Enqueue(Tuple.Create(outputEnumerationBuildStep.Template, currentStep.Item2));
                }
            }
        }

        private void UpdateRelationToSelectedStep()
        {
            BuildStep selectedBuildStep = SelectedNode != null ? (BuildStep)((ObservableViewModelNode)SelectedNode.ViewModel).ModelNode.Content.Value : null;

            if (selectedBuildStep != null && executionGroups.ContainsKey(selectedBuildStep))
            {
                int selectedExecGroup = executionGroups[selectedBuildStep];
                WaitBuildStep[] selectedWaitPredecessors = waitPredecessorMap[selectedBuildStep];

                foreach (IViewModelNode viewModel in contextUI.ViewModelByGuid.Values)
                {
                    var buildStep = (BuildStep)((ObservableViewModelNode)viewModel).ModelNode.Content.Value;
                    var node = (ObservableViewModelNode<bool>)viewModel.Children.Single(x => x.Name == "IsParallelToSelectedStep");
                    node.TValue = executionGroups[buildStep] == selectedExecGroup && buildStep != selectedBuildStep;
                    node = (ObservableViewModelNode<bool>)viewModel.Children.Single(x => x.Name == "IsPrerequisiteToSelectedStep");
                    node.TValue = selectedWaitPredecessors.Any(x => waitedBuildStepMap[x].Contains(buildStep));
                }
            }
            else
            {
                foreach (IViewModelNode viewModel in contextUI.ViewModelByGuid.Values)
                {
                    var node = (ObservableViewModelNode<bool>)viewModel.Children.Single(x => x.Name == "IsParallelToSelectedStep");
                    node.TValue = false;
                    node = (ObservableViewModelNode<bool>)viewModel.Children.Single(x => x.Name == "IsPrerequisiteToSelectedStep");
                    node.TValue = false;
                }
            }
        }

        private static string ReadLCSFromBacktrack(int[,] backtrack, string list1, string list2, int list1Position, int list2Position)
        {
            string result;
            if ((list1Position < 0) || (list2Position < 0))
            {
                return "";
            }
            if (list1[list1Position] == list2[list2Position])
            {
                result = ReadLCSFromBacktrack(backtrack, list1, list2, list1Position - 1, list2Position - 1);
                result += list1[list1Position];
            }
            else
            {
                if (backtrack[list1Position, list2Position - 1] >= backtrack[list1Position - 1, list2Position])
                {
                    result = ReadLCSFromBacktrack(backtrack, list1, list2, list1Position, list2Position - 1);
                }
                else
                {
                    result = ReadLCSFromBacktrack(backtrack, list1, list2, list1Position - 1, list2Position);
                }
                if (result.Last() != '*')
                    result += '*';
            }
            return result;
        }

        public static string GeneratePathUsingAliasVariable(FileViewModel file)
        {
            return "${SourceFolder:" + file.RootDirectory.Alias + "}" + file.Path.Substring(file.Path.IndexOf('/'));
        }

        private static IEnumerable<string> FindPatterns(IEnumerable<FileViewModel> fileViewModelsEnum, int maxPattern)
        {
            string[] filesEnum = fileViewModelsEnum.Select(GeneratePathUsingAliasVariable).ToArray();
            string pattern = null;
            foreach (var file in filesEnum)
            {
                if (pattern == null)
                {
                    pattern = file;
                }
                else
                {
                    var backtrack = LCS.GetLCS(pattern.ToList(), file.ToList());
                    pattern = ReadLCSFromBacktrack(backtrack, pattern, file, pattern.Length - 1, file.Length - 1);
                }
            }

            var result = new List<string>();

            if (!filesEnum.Any())
            {
                result.Add("*");
                return result;
            }

            if (!string.IsNullOrEmpty(pattern))
            {
                result.Add(pattern);
                --maxPattern;
            }

            bool addExtensionPattern = true;
            string firstFile = filesEnum.First();
            string extension = Path.GetExtension(firstFile);
            foreach (string file in filesEnum.Skip(1))
            {
                if (string.Compare(extension, Path.GetExtension(file), StringComparison.OrdinalIgnoreCase) != 0)
                    addExtensionPattern = false;
            }

            if (addExtensionPattern)
            {
                extension = Path.Combine(Path.GetDirectoryName(firstFile) ?? "", "*" + extension).Replace('\\', '/');
                if (extension != pattern)
                {
                    result.Add(extension);
                    --maxPattern;
                }
            }

            result.AddRange(filesEnum.Take(maxPattern));

            return result;
        }

        private void UpdateBuildStepTagsRecursively(BuildStep step, bool clear = false, List<BuildStep> processedSteps = null)
        {
            if (processedSteps == null)
                processedSteps = new List<BuildStep>();

            step.Tag = clear ? null : (object)Guid.NewGuid();
            processedSteps.Add(step);

            foreach (BuildStep child in step.GetChildSteps().Where(child => !processedSteps.Contains(child)))
            {
                UpdateBuildStepTagsRecursively(child, clear, processedSteps);
            }
        }
    }
}
