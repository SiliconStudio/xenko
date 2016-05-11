// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public abstract class ModalDialogBase : IModalDialogInternal
    {
        private readonly IDispatcherService dispatcher;
        private readonly Window parentWindow;
        protected CommonFileDialog Dialog;

        protected ModalDialogBase(IDispatcherService dispatcher, Window parentWindow)
        {
            this.dispatcher = dispatcher;
            this.parentWindow = parentWindow;
        }

        /// <inheritdoc/>
        public object DataContext { get; set; }

        /// <inheritdoc/>
        public DialogResult Result { get; set; }

        protected Task InvokeDialog()
        {
            return dispatcher.InvokeAsync(() =>
            {
                var result = Dialog.ShowDialog(parentWindow);
                switch (result)
                {
                    case CommonFileDialogResult.None:
                        Result = DialogResult.None;
                        break;
                    case CommonFileDialogResult.Ok:
                        Result = DialogResult.Ok;
                        break;
                    case CommonFileDialogResult.Cancel:
                        Result = DialogResult.Cancel;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        public abstract Task<DialogResult> ShowModal();
    }
}