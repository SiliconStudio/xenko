// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public class FolderOpenModalDialog : ModalDialogBase, IFolderOpenModalDialog
    {
        internal FolderOpenModalDialog([NotNull] IDispatcherService dispatcher)
            : base(dispatcher)
        {
            Dialog = new CommonOpenFileDialog { EnsurePathExists = true };
            OpenDlg.IsFolderPicker = true;
        }

        /// <inheritdoc/>
        public string Directory { get; private set; }

        /// <inheritdoc/>
        public string InitialDirectory { get { return OpenDlg.InitialDirectory; } set { OpenDlg.InitialDirectory = value.Replace('/', '\\'); } }

        private CommonOpenFileDialog OpenDlg => (CommonOpenFileDialog)Dialog;

        public override async Task<DialogResult> ShowModal()
        {
            await InvokeDialog();
            Directory = Result != DialogResult.Cancel ? OpenDlg.FileName : null;
            return Result;
        }
    }
}
