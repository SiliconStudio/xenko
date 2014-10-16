using System.Collections.Generic;
using SiliconStudio.Presentation.Sample4.Model;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Sample4.ViewModel
{
    public class SiSdkPropertyChangeActionItem : SiSdkCommandActionItem
    {
        private readonly SiSdkViewModel viewModel;
        private readonly string propertyName;

        public SiSdkPropertyChangeActionItem(string name, IEnumerable<IDirtiableViewModel> dirtiables, ICommandOpManager cmdManager, SiSdkViewModel viewModel, string propertyName)
            : base(name, dirtiables, cmdManager)
        {
            this.viewModel = viewModel;
            this.propertyName = propertyName;
        }

        protected override void UndoAction()
        {
            viewModel.NotifyPropertyChangingFromUndoRedo(propertyName);
            base.UndoAction();
            viewModel.NotifyPropertyChangedFromUndoRedo(propertyName);
        }

        protected override void RedoAction()
        {
            viewModel.NotifyPropertyChangingFromUndoRedo(propertyName);
            base.RedoAction();
            viewModel.NotifyPropertyChangedFromUndoRedo(propertyName);
        }
    }
}