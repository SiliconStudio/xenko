// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public abstract class ModalDialogBase : IModalDialogInternal
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

        /// <inheritdoc/>
        DialogResult IModalDialogInternal.Result { get; set; }

        protected Task InvokeDialog()
        {
            return dispatcher.InvokeAsync(() =>
            {
                var result1 = Dialog.ShowDialog(parentWindow);
                switch (result1)
                {
                    case CommonFileDialogResult.None:
                        ((IModalDialogInternal)this).Result = DialogResult.None;
                        break;
                    case CommonFileDialogResult.Ok:
                        ((IModalDialogInternal)this).Result = DialogResult.Ok;
                        break;
                    case CommonFileDialogResult.Cancel:
                        ((IModalDialogInternal)this).Result = DialogResult.Cancel;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).Task;
        }

        public abstract Task<DialogResult> Show();
    }
}