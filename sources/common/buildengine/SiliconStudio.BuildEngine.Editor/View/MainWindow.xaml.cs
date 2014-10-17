using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.DragNDrop;
using System.Windows.Input;

using Microsoft.WindowsAPICodePack.Dialogs;

using SiliconStudio.BuildEngine.Editor.ViewModel;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ExpressionDark;

namespace SiliconStudio.BuildEngine.Editor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public BuildEditionViewModel BuildEdition { get; private set; }

        private readonly PluginResolver plugins = new PluginResolver();
        private Point dragStartPoint;

        public ICommand NewSessionCommand { get; set; }
        public ICommand OpenSessionCommand { get; set; }
        public ICommand SaveSessionCommand { get; set; }

        public ICommand CompileScriptCommand { get; set; }
        public ICommand StopScriptCompilationCommand { get; set; }
        public ICommand ClearBuildResultCommand { get; set; }

        public MainWindow()
        {
            NewSessionCommand = new AsyncCommand(NewSession);
            OpenSessionCommand = new AsyncCommand(async () => await OpenSession());
            SaveSessionCommand = new AsyncCommand(SaveSession);

            CompileScriptCommand = new AsyncCommand(async () => await CompileScript());
            StopScriptCompilationCommand = new AsyncCommand(async () => await StopScriptCompilation());
            ClearBuildResultCommand = new AsyncCommand(ClearBuildResult);

            InitializeComponent();

            this.FixExpressionDarkTheme();

            // Plugin initialization
            plugins.AddPluginFolder(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "", "BuildPlugins"));
            plugins.Register();

            BuildEdition = new BuildEditionViewModel();
            BuildEditionViewModel.Dispatcher = Dispatcher;
            BuildEdition.FetchAvailableBuildStepsAndCommands(plugins);

            DataContext = BuildEdition;

            NewSession();
        }

        private void NewSession(object fileName = null)
        {
            BuildSessionViewModel session = BuildEdition.CreateSession();
            session.ScriptPath = (string)fileName;
            BuildEdition.ActiveSession = session;
        }

        private async Task OpenSession()
        {
            var openDlg = new CommonOpenFileDialog { Filters = { new CommonFileDialogFilter("Paradox project", "*.pdxproj") } };
            if (InvokeShowDialog(openDlg) == CommonFileDialogResult.Ok)
            {
                string sdkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..");

                BuildScript script = BuildScript.LoadFromFile(sdkDir, openDlg.FileName);
                if (!script.Compile(plugins))
                {
                    string errors = string.Join(Environment.NewLine, script.GetErrors());
                    MessageBox.Show("The script file contains errors and cannot be compiled." + Environment.NewLine + errors);
                    return;
                }
                NewSession(openDlg.FileName);
                try
                {
                    await BuildEdition.ActiveSession.LoadFromScript(script);
                }
                catch (Exception)
                {
                    MessageBox.Show("The script file contains errors and cannot be compiled.");
                    CloseSession(BuildEdition.ActiveSession);
                }
            }
        }

        private void SaveSession()
        {
            if (BuildEdition.ActiveSession != null)
            {
                if (string.IsNullOrEmpty(BuildEdition.ActiveSession.ScriptPath))
                {
                    var saveDlg = new CommonSaveFileDialog { Filters = { new CommonFileDialogFilter("Build script", "*.pdxproj") } };
                    if (InvokeShowDialog(saveDlg) == CommonFileDialogResult.Ok)
                    {
                        BuildEdition.ActiveSession.ScriptPath = saveDlg.FileName;
                    }
                }
                if (!string.IsNullOrEmpty(BuildEdition.ActiveSession.ScriptPath))
                {
                    BuildEdition.ActiveSession.Save(BuildEdition.ActiveSession.ScriptPath);
                }
            }
        }

        private void CloseSession(object session)
        {
            BuildEdition.CloseSession((BuildSessionViewModel)session);
        }

        private async Task CompileScript()
        {
            if (string.IsNullOrEmpty(BuildEdition.ActiveSession.SourceBaseDirectory))
            {
                MessageBox.Show("No source folder has been defined yet. Please edit the script settings", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(BuildEdition.ActiveSession.BuildDirectory))
            {
                MessageBox.Show("No build folder has been defined yet. Please edit the script settings", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (BuildEdition.IsBuildRunning)
                return;

            BuildEdition.IsBuildRunning = true;
            BuildEdition.ActiveSession.GenerateTagsForBuildStep();

            string tempPath = Path.Combine(Path.GetTempPath(), "SiliconStudio", "BuildEngine", Guid.NewGuid() + ".pdxproj");
            BuildEdition.ActiveSession.SaveTemporary(tempPath);

            await BuildEdition.ActiveSession.ExecuteScript(tempPath);
            BuildEdition.IsBuildRunning = false;
            BuildEdition.ActiveSession.ClearBuildStepTags();
            await BuildEdition.AssetExplorer.Refresh();
        }

        private async Task StopScriptCompilation()
        {
            BuildEdition.ActiveSession.StopScriptExecution();
            BuildEdition.IsBuildRunning = false;
            BuildEdition.ActiveSession.ClearBuildStepTags();
            await BuildEdition.AssetExplorer.Refresh();
        }

        private void ClearBuildResult()
        {
            if (BuildEdition.ActiveSession != null)
            {
                BuildEdition.ActiveSession.ClearBuildResult();
            }
        }

        private CommonFileDialogResult InvokeShowDialog(CommonFileDialog dialog)
        {
            return Dispatcher.CheckAccess() ? dialog.ShowDialog() : Dispatcher.Invoke(() => dialog.ShowDialog());
        }

        private bool mouseMoveRegistered;

        private void NewStepItem_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BuildEdition.IsBuildRunning)
                return;

            dragStartPoint = e.GetPosition(null);
            var elem = sender as FrameworkElement;
            if (elem != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount >= 2)
                {
                    BuildEdition.ActiveSession.AddSelectedBuildStepOrCommand.Execute(elem.DataContext);
                }
                if (!mouseMoveRegistered)
                {
                    elem.PreviewMouseMove += NewStepItem_OnMouseMove;
                    mouseMoveRegistered = true;
                }
            }
            else if (elem != null)
            {
                if (mouseMoveRegistered)
                {
                    elem.PreviewMouseMove -= NewStepItem_OnMouseMove;
                    mouseMoveRegistered = false;
                }
            }
        }

        private void NewStepItem_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var elem = sender as FrameworkElement;
            if (elem != null)
            {
                if (mouseMoveRegistered)
                {
                    elem.PreviewMouseMove -= NewStepItem_OnMouseMove;
                    mouseMoveRegistered = false;
                }
            }
        }

        private void NewStepItem_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = dragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var frameworkElem = ((FrameworkElement)e.OriginalSource);
                var content = new DragContent();
                content.Add(frameworkElem.DataContext);
                DragDrop.DoDragDrop(frameworkElem, new DataObject("NewBuildStep", content), DragDropEffects.Move);
            }
        }
    }
}
