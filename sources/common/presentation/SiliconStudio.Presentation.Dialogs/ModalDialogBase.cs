// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public abstract class ModalDialogBase : IModalDialog
    {
        private readonly Dispatcher dispatcher;
        private readonly Window parentWindow;
        protected CommonFileDialog Dialog;

        protected ModalDialogBase(Dispatcher dispatcher, Window parentWindow)
        {
            this.dispatcher = dispatcher;
            this.parentWindow = parentWindow;
        }

        /// <inheritdoc/>
        public object DataContext { get; set; }

        protected Task<DialogResult> InvokeDialog()
        {
            return dispatcher.InvokeAsync(() => (DialogResult)Dialog.ShowDialog(parentWindow)).Task;
        }

        public abstract Task<DialogResult> Show();
    }
}