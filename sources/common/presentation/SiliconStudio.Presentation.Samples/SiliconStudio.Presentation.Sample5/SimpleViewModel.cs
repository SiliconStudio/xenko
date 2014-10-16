using System;
using System.IO;
using System.Linq;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Sample
{
    public class SimpleViewModel : DispatcherViewModel
    {
        private readonly IDialogService dialog;
        private string text;

        public SimpleViewModel(IDispatcherService dispatcher, IDialogService dialog)
            : base(dispatcher)
        {
            this.dialog = dialog;
            OpenFileCommand = new AsyncCommand(OpenFile);
        }

        /// <summary>
        /// Gets a command that will open a file and display its content in the text box.
        /// This command is asynchronous to prevent freezing the UI if the file is long to read.
        /// </summary>
        public CommandBase OpenFileCommand { get; private set; }

        public string Text { get { return text; } set { SetValue(ref text, value); } }

        private async void OpenFile()
        {
            var openDlg = dialog.CreateFileOpenModalDialog();
            openDlg.Filters.Add(new FileDialogFilter("Text files", "*.txt;*.xml;*.html;*.htm;*.cs;*.cpp;*.h;*.csproj,*.vcxproj;*.sln;*.xaml"));
            openDlg.Filters.Add(new FileDialogFilter("All files", "*.*"));
            var result = openDlg.Show();
            if (result != DialogResult.Ok)
            {
                dialog.ShowMessageBox("Operation cancelled", "Sample", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string content;
            try
            {
                using (var reader = new StreamReader(openDlg.FilePaths.First()))
                {
                    content = await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                dialog.ShowMessageBox(string.Format("An exception occured while reading the selected file: {0}", e.Message), "Sample", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Text = content;
        }
    }
}