using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Input;
using System.Windows.Threading;
using SiliconStudio.BuildEngine.Editor.Service;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum.Legacy;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class BuildEditionViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The list of all existing sessions
        /// </summary>
        public ObservableCollection<BuildSessionViewModel> Sessions { get; private set; }

        /// <summary>
        /// The currently active session
        /// </summary>
        public BuildSessionViewModel ActiveSession { get { return activeSession; } set { SetValue(ref activeSession, value, "ActiveSession"); } }
        private BuildSessionViewModel activeSession;

        /// <summary>
        /// The list of all available build steps and commands to add to the active session
        /// </summary>
        public ObservableCollection<Type> AvailableBuildStepsAndCommands { get; private set; }

        /// <summary>
        /// Indicate wheither a build is currently running, in which case most of the edition option should be frozen
        /// </summary>
        public bool IsBuildRunning { get { return isBuildRunning; } set { SetValue(ref isBuildRunning, value, "IsBuildRunning"); } }
        private bool isBuildRunning;

        public ObservableCollection<object> SelectedFiles { get { return selectedFiles; } set { SetValue(ref selectedFiles, value, "SelectedFiles"); } }
        private ObservableCollection<object> selectedFiles;

        public ObservableCollection<object> SelectedAssets { get { return selectedAssets; } set { selectedAssets = value; OnPropertyChanged("SelectedAssets"); } }
        private ObservableCollection<object> selectedAssets;
   
        #endregion Properties

        #region Internal fields
        /// <summary>
        /// The single global context used to construct all view models
        /// </summary>
        internal ViewModelGuidContainer GuidContainer;

        /// <summary>
        /// An unique url to use for the monitor pipe
        /// </summary>
        internal string MonitorPipeUrl = "net.pipe://localhost/Paradox.BuildEngine.Monitor." + Guid.NewGuid();
        #endregion Internal fields

        #region Private fields
        /// <summary>
        /// The singleton instance of the BuildEditionViewModel
        /// </summary>
        private static BuildEditionViewModel instance;

        private readonly BuildMonitorRemote buildMonitorRemote;
        #endregion Private fields

        #region AvalonDock specific
        public ToolboxViewModel Toolbox { get; set; }
        public PropertyGridViewModel Properties { get; set; }
        public FileExplorerViewModel FileExplorer { get; set; }
        public AssetExplorerViewModel AssetExplorer { get; set; }
        public BuildSettingsViewModel BuildSettings { get; set; }
        public MetadataViewModel Metadata { get; set; }
        public IEnumerable<object> Anchorables { get { yield return Toolbox; yield return Properties; yield return BuildSettings; yield return FileExplorer; yield return AssetExplorer; yield return Metadata; } }
        #endregion

        public ICommand OpenDropContextMenu { get; set; }

        public static Dispatcher Dispatcher;

        public BuildEditionViewModel()
        {
            if (instance != null)
                throw new InvalidOperationException("The singleton class BuildEditionViewModel has already been instanced.");

            instance = this;
            GuidContainer = new ViewModelGuidContainer();
            AvailableBuildStepsAndCommands = new ObservableCollection<Type>();
            Sessions = new ObservableCollection<BuildSessionViewModel>();

            #region AvalonDock specific
            Toolbox = new ToolboxViewModel(this);
            Properties = new PropertyGridViewModel(this);
            FileExplorer = new FileExplorerViewModel(this);
            AssetExplorer = new AssetExplorerViewModel(this);
            BuildSettings = new BuildSettingsViewModel(this);
            Metadata = new MetadataViewModel(this);
            #endregion

            PropertyChanged += BuildEditionViewModelPropertyChanged;
            buildMonitorRemote = new BuildMonitorRemote(this);
            var host = new ServiceHost(buildMonitorRemote);
            host.AddServiceEndpoint(typeof(IBuildMonitorRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, MonitorPipeUrl);
            host.Open();
        }

        private void BuildEditionViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ActiveSession != null && e.PropertyName == "ActiveSession")
                ActiveSession.OnSessionActivated();
        }

        public BuildSessionViewModel CreateSession()
        {
            var newSession = new BuildSessionViewModel(this);
            newSession.Refresh();
            Dispatcher.Invoke(() => Sessions.Add(newSession));
            return newSession;
        }

        public void CloseSession(BuildSessionViewModel session)
        {
            Sessions.Remove(session);
            if (ActiveSession == session)
            {
                ActiveSession = Sessions.LastOrDefault();
            }
        }

        public static BuildSessionViewModel GetActiveSession()
        {
            if (instance == null)
                throw new InvalidOperationException("The singleton class BuildEditionViewModel has not been instanced yet.");
            if (instance.ActiveSession == null)
                throw new InvalidOperationException("The singleton class BuildEditionViewModel has no active session.");

            return instance.ActiveSession;
        }

        public static bool IsBuilding()
        {
            return instance.IsBuildRunning;
        }

        public void RefreshSessionDependentAnchorables()
        {
            Dispatcher.Invoke(() => BuildSettings.Refresh());
        }

        public void FetchAvailableBuildStepsAndCommands(PluginResolver plugins)
        {
            var steps = new List<Type>();
            var commands = new List<Type>();

            var assembly = Assembly.Load(new AssemblyName("SiliconStudio.BuildEngine.Common"));
            FetchAvailableBuildStepsAndCommandsInAssembly(assembly, steps, commands);

            foreach (string assemblyName in plugins.PluginAssemblyLocations)
            {
                try
                {
                    assembly = plugins.LoadAssembly(assemblyName);
                    FetchAvailableBuildStepsAndCommandsInAssembly(assembly, steps, commands);
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                    // Invalid assembly   
                }
            }

            AvailableBuildStepsAndCommands.Clear();

            foreach (var step in steps)
                AvailableBuildStepsAndCommands.Add(step);

            foreach (var command in commands)
                AvailableBuildStepsAndCommands.Add(command);
        }

        private static void FetchAvailableBuildStepsAndCommandsInAssembly(Assembly assembly, ICollection<Type> steps, ICollection<Type> commands)
        {
            foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && type != typeof(CommandBuildStep)))
            {
                // TODO/Benlitz: Remove this once EnumerableBuildStep has been made abstract
                if (type == typeof(EnumerableBuildStep))
                    continue;

                if (type.IsSubclassOf(typeof(BuildStep)))
                {
                    steps.Add(type);
                }
                if (type.IsSubclassOf(typeof(Command)))
                {
                    commands.Add(type);
                }
            }
        }
    }
}
