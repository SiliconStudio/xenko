// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public class FileSaveModalDialog : ModalDialogBase, IFileSaveModalDialog
    {
        internal FileSaveModalDialog(Dispatcher dispatcher, Window parentWindow)
            : base(dispatcher, parentWindow)
        {
            Dialog = new CommonSaveFileDialog();
            Filters = new List<FileDialogFilter>();
        }

        /// <inheritdoc/>
        public IList<FileDialogFilter> Filters { get; set; }

        /// <inheritdoc/>
        public string FilePath { get; private set; }

        /// <inheritdoc/>
        public string InitialDirectory { get { return SaveDlg.InitialDirectory; } set { SaveDlg.InitialDirectory = value; } }

        /// <inheritdoc/>
        public string DefaultFileName { get { return SaveDlg.DefaultFileName; } set { SaveDlg.DefaultFileName = value; } }

        /// <inheritdoc/>
        public string DefaultExtension { get { return SaveDlg.DefaultExtension; } set { SaveDlg.DefaultExtension = value; } }

        private CommonSaveFileDialog SaveDlg => (CommonSaveFileDialog)Dialog;

        /// <inheritdoc/>
        public override DialogResult Show()
        {
            SaveDlg.Filters.Clear();
            foreach (var filter in Filters.Where(x => !string.IsNullOrEmpty(x.ExtensionList)))
            {
                SaveDlg.Filters.Add(new CommonFileDialogFilter(filter.Description, filter.ExtensionList));
            }
            SaveDlg.AlwaysAppendDefaultExtension = true;
            var result = InvokeDialog();
            FilePath = result != DialogResult.Cancel ? SaveDlg.FileName : null;
            return result;
        }
    }
}
