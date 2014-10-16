using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Sample4.Model;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;

// This view model represents a global view model for the application.
// The object that we edit is a SimpleViewModel.
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.Sample4.ViewModel
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

            var commandManager = new FakeCommandOpManager();
            var simpleModel = commandManager.GetSimpleModel();

            // This is the actual view model we will modify in the UI
            SimpleObject = new SimpleViewModel(actionStack, dispatcher, commandManager, simpleModel);

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