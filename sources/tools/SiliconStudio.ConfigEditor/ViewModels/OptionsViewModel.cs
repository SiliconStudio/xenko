using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.IO;
using System.Windows;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Paradox.ConfigEditor.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        public Options Options { get; private set; }

        public OptionsViewModel()
        {
            Options = Options.Load() ?? new Options();

            ParadoxPath = Options.ParadoxPath;
            ParadoxConfigFilename = Options.ParadoxConfigFilename;

            CheckParadoxPath();
            CheckParadoxConfigFilename();

            BrowsePathCommand = new AnonymousCommand(BrowsePath);
            BrowseConfigFileCommand = new AnonymousCommand(BrowseConfigFile);
        }

        public void SetOptionsWindow(Window window)
        {
            CloseCommand = new AnonymousCommand(window.Close);
        }

        public ICommand CloseCommand { get; private set; }
        public ICommand BrowsePathCommand { get; private set; }
        public ICommand BrowseConfigFileCommand { get; private set; }

        private void BrowsePath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Paradox base directory",
                ShowNewFolderButton = true,
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ParadoxPath = dialog.SelectedPath;
        }

        private void BrowseConfigFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select the Paradox configuration file",
                Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Multiselect = false,
                CheckFileExists = true,
            };

            if (dialog.ShowDialog() == true)
                ParadoxConfigFilename = dialog.FileName;
        }

        private string paradoxPath;
        public string ParadoxPath
        {
            get { return paradoxPath; }
            set
            {
                if (SetValue(ref paradoxPath, value, "ParadoxPath"))
                    CheckParadoxPath();
            }
        }

        private bool isParadoxPathValid;
        public bool IsParadoxPathValid
        {
            get { return isParadoxPathValid; }
            set { SetValue(ref isParadoxPathValid, value, "IsParadoxPathValid"); }
        }

        private void CheckParadoxPath()
        {
            IsParadoxPathValid = Directory.Exists(ParadoxPath);
        }

        private string paradoxConfigFilename;
        public string ParadoxConfigFilename
        {
            get { return paradoxConfigFilename; }
            set
            {
                if (SetValue(ref paradoxConfigFilename, value, "ParadoxConfigFilename"))
                    CheckParadoxConfigFilename();
            }
        }

        private bool isParadoxConfigFilenameValid;
        public bool IsParadoxConfigFilenameValid
        {
            get { return isParadoxConfigFilenameValid; }
            set { SetValue(ref isParadoxConfigFilenameValid, value, "IsParadoxConfigFilenameValid"); }
        }

        private void CheckParadoxConfigFilename()
        {
            if (string.IsNullOrWhiteSpace(ParadoxConfigFilename))
            {
                IsParadoxConfigFilenameValid = true;
                return;
            }

            var tempFilename = ParadoxConfigFilename;

            if (Path.IsPathRooted(tempFilename) == false)
                tempFilename = Path.Combine(ParadoxPath, ParadoxConfigFilename);

            IsParadoxConfigFilenameValid = File.Exists(tempFilename);
        }

        private ICommand acceptCommand;
        public ICommand AcceptCommand
        {
            get
            {
                if (acceptCommand == null)
                    acceptCommand = new AnonymousCommand(Accept);
                return acceptCommand;
            }
        }

        private void Accept()
        {
            if (string.IsNullOrWhiteSpace(ParadoxPath))
            {
                MessageBox.Show("Invalid Paradox Path, this field must not be empty.", "Paradox Path Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Directory.Exists(ParadoxPath) == false)
            {
                string message = string.Format("Invalid Paradox Path, the directory '{0}' does not exit.", ParadoxPath);
                MessageBox.Show(message, "Paradox Path Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Options.ParadoxPath = ParadoxPath;
            Options.ParadoxConfigFilename = ParadoxConfigFilename;

            Options.Save();

            var handler = OptionsChanged;
            if (handler != null)
                handler();

            CloseCommand.Execute(null); // this just closes the Options window
        }

        public event Action OptionsChanged;
    }
}
