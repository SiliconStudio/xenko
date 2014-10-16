using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Quantum;
using SiliconStudio.Presentation.Sample3.Cplusplus;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;

// This view model represents a global view model for the application.
// The object that we edit is a SimpleViewModel.
using SiliconStudio.Presentation.ViewModel.ActionStack;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Sample3
{
    public class RootViewModel : DispatcherViewModel
    {
        private readonly ViewModelTransactionalActionStack actionStack;
        private readonly ModelContainer modelContainer = new ModelContainer();
        private string nativeValueString;
        private MyNativeClassWrapper wrapper;

        public RootViewModel(IDispatcherService dispatcher)
            : base(dispatcher)
        {
            // The action stack will manage undo/redo and dirty flag
            actionStack = new ViewModelTransactionalActionStack(100, dispatcher);
            // This is a view model to easily visualize the action stack.
            ActionStack = new ActionStackViewModel(actionStack);
            // This will create a dynamic view model
            InitializeViewModel();

            // we have commands to easily bind undo/redo/save from the UI to the action stack
            UndoCommand = new AnonymousCommand(() => actionStack.Undo());
            RedoCommand = new AnonymousCommand(() => actionStack.Redo());
            SaveCommand = new AnonymousCommand(() => ActionStack.NotifySave());
            PrintNativeValuesCommand = new AnonymousCommand(() => NativeValueString = wrapper.PrintNativeValues());
        }

        public ObservableViewModel SimpleObject { get; private set; }

        public ActionStackViewModel ActionStack { get; private set; }

        public string NativeValueString { get { return nativeValueString; } set { SetValue(ref nativeValueString, value); } }

        public CommandBase PrintNativeValuesCommand { get; private set; }

        public CommandBase UndoCommand { get; private set; }

        public CommandBase RedoCommand { get; private set; }

        public CommandBase SaveCommand { get; private set; }

        public DirtiableEditableViewModel Dirtiable { get; private set; }

        private void InitializeViewModel()
        {
            // Let's just use a simple dirtiable view model just to have the modified/saved in this sample
            Dirtiable = new DirtiableEditableViewModel(actionStack, Dispatcher);
            // We create a wrapped object - this is not a view model, it just wraps a C++ object
            wrapper = MyNativeClassWrapper.CreateInstance();
            // Here we generate nodes from our object. These nodes are serializable and could be used for inter-process binding
            // If the following part of the Quantum framework can be rewritten in C++, no C++/CLI wrapper would be needed at all!
            // This is what we should discuss and check!
            var rootModelNode = modelContainer.GetOrCreateModelNode(wrapper, typeof(MyNativeClassWrapper));
            // This provider allows to provide UI-side object to the view model generator
            var serviceProvider = new ObservableViewModelServiceProvider(Dispatcher, actionStack) { ViewModelProvider = x => SimpleObject };
            // This will generate the view model
            SimpleObject = new ObservableViewModel(serviceProvider, modelContainer, rootModelNode, new[] { Dirtiable });
        }
    }
}