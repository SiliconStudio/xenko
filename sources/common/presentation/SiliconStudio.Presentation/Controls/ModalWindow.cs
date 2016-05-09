using System;
using System.Threading.Tasks;
using System.Windows;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.Windows;

namespace SiliconStudio.Presentation.Controls
{
    public abstract class ModalWindow : Window, IModalDialogInternal
    {
        async Task<DialogResult> IModalDialog.Show()
        {
            await WindowManager.ShowTopModal(this);
            return Result;
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
