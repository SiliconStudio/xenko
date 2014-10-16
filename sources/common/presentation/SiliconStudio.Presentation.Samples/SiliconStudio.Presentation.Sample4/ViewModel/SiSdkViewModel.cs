using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Sample4.Model;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.Sample4.ViewModel
{
    public class SiSdkViewModel : DirtiableEditableViewModel
    {
        protected readonly ICommandOpManager CmdManager;

        public SiSdkViewModel(ITransactionalActionStack actionStack, IDispatcherService dispatcher, ICommandOpManager cmdManager)
            : base(actionStack, dispatcher)
        {
            CmdManager = cmdManager;
        }

        protected void InvokeSetProperty(object value)
        {
            var param = string.Format("PropertyValue = {0}", value);
            CmdManager.Execute("IFSetProperty", param, true);
        }

        protected override ViewModelActionItem CreatePropertyChangeActionItem(string displayName, string propertyName, object preEditValue)
        {
            var action = new SiSdkPropertyChangeActionItem(displayName, this.Yield(), CmdManager, this, propertyName);
            return action;
        }

        // NOTE: THIS METHOD IS A WORKAROUND TO THE FACT THAT WE DON'T KNOW WHAT THE COMMAND MANAGER MODIFY
        // IT WOULD BE A PROBLEM FOR COMMANDS MORE COMPLEX THAT "SetProperty"
        public void NotifyPropertyChangingFromUndoRedo(string propertyName)
        {
            base.OnPropertyChanging(propertyName);
        }

        // NOTE: THIS METHOD IS A WORKAROUND TO THE FACT THAT WE DON'T KNOW WHAT THE COMMAND MANAGER MODIFY
        // IT WOULD BE A PROBLEM FOR COMMANDS MORE COMPLEX THAT "SetProperty"
        public void NotifyPropertyChangedFromUndoRedo(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}