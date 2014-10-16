using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Presentation.ViewModel.ActionStack;

// This view model represents a global view model for the application.
// The object that we edit is a SimpleViewModel.

namespace SiliconStudio.Presentation.Sample2
{
    public class RootViewModel : DispatcherViewModel
    {
        public RootViewModel(IDispatcherService dispatcher)
            : base(dispatcher)
        {
            // The action stack will manage undo/redo and dirty flag
            var actionStack = new ViewModelTransactionalActionStack(100, dispatcher);
            // This is a view model to easily visualize the action stack.
            ActionStack = new ActionStackViewModel(actionStack);
            // This is the actual view model we will modify in the UI
            SimpleObject = new SimpleViewModel(actionStack, dispatcher);

            // we have commands to easily bind undo/redo/save from the UI to the action stack
            UndoCommand = new AnonymousCommand(() => actionStack.Undo());
            RedoCommand = new AnonymousCommand(() => actionStack.Redo());
            SaveCommand = new AnonymousCommand(() => ActionStack.NotifySave());
        }

        public SimpleViewModel SimpleObject { get; private set; }

        public ActionStackViewModel ActionStack { get; private set; }

        public CommandBase UndoCommand { get; private set; }

        public CommandBase RedoCommand { get; private set; }

        public CommandBase SaveCommand { get; private set; }
    }
}