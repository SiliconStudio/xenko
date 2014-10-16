using System.Collections.Generic;
using SiliconStudio.Presentation.Sample4.Model;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.Sample4.ViewModel
{
    public class SiSdkCommandActionItem : ViewModelActionItem
    {
        private readonly ICommandOpManager cmdManager;

        public SiSdkCommandActionItem(string name, IEnumerable<IDirtiableViewModel> dirtiables, ICommandOpManager cmdManager)
            : base(name, dirtiables)
        {
            this.cmdManager = cmdManager;
        }

        protected override void FreezeMembers()
        {
            // TODO: do nothing? Notify the cmdManager that it can free some memory?
        }

        protected override void UndoAction()
        {
            cmdManager.Undo();
        }

        protected override void RedoAction()
        {
            cmdManager.Redo();
        }
    }
}