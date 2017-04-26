// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Dialogs
{
    public abstract class ModalDialogBase : IModalDialogInternal
    {
        private readonly IDispatcherService dispatcher;
        protected CommonFileDialog Dialog;

        protected ModalDialogBase([NotNull] IDispatcherService dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            this.dispatcher = dispatcher;
        }

        /// <param name="result"></param>
        /// <inheritdoc/>
        public void RequestClose(DialogResult result)
        {
            throw new NotSupportedException("RequestClose is not supported for this dialog.");
        }

        /// <inheritdoc/>
        public object DataContext { get; set; }

        /// <inheritdoc/>
        public DialogResult Result { get; set; }

        [NotNull]
        protected Task InvokeDialog()
        {
            return dispatcher.InvokeAsync(() =>
            {
                var result = Dialog.ShowDialog();
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

        [NotNull]
        public abstract Task<DialogResult> ShowModal();
    }
}
