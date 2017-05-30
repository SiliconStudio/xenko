// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using SiliconStudio.Presentation.Interop;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.Windows;

namespace SiliconStudio.Presentation.Controls
{
    public abstract class ModalWindow : Window, IModalDialogInternal
    {
        public virtual async Task<DialogResult> ShowModal()
        {
			Loaded += (sender, e) =>
            {
                // Disable minimize on modal windows
                var handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                {
                    NativeHelper.DisableMinimizeButton(handle);
                }
            };
            Owner = WindowManager.MainWindow?.Window ?? WindowManager.BlockingWindows.LastOrDefault()?.Window;
            WindowStartupLocation = Owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            await Dispatcher.InvokeAsync(ShowDialog);
            return Result;
        }

        public void RequestClose(DialogResult result)
        {
            Result = result;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Result == Services.DialogResult.None)
                Result = Services.DialogResult.Cancel;
        }

        public DialogResult Result { get; set; } = Services.DialogResult.None;
    }
}
