using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;

// This is a simple example of view model based on Sample1, but with a list and a command.
// This view model also inherits DirtiableEditableViewModel, which manages two additional things:
// - A dirty/saved state ("dirtiable")
// - Registering property changes to an action stack for undo/redo ("editable")

namespace SiliconStudio.Presentation.Sample2
{
    public class SimpleViewModel : DirtiableEditableViewModel
    {
        private int intValue = 5;
        private double doubleValue = 3.14;
        private string stringValue;
        private readonly ObservableList<string> stringList = new ObservableList<string>();

        public SimpleViewModel(ITransactionalActionStack actionStack, IDispatcherService dispatcher)
            : base(actionStack, dispatcher)
        {
            PopulateListCommand = new PopulateListCommand(actionStack, Dirtiables);
        }

        public int IntValue { get { return intValue; } set { SetValue(ref intValue, value); } }

        public double DoubleValue { get { return doubleValue; } set { SetValue(ref doubleValue, value); } }

        public string StringValue { get { return stringValue; } set { SetValue(ref stringValue, value); } }
        
        public ObservableList<string> StringList { get { return stringList; } }

        public CancellableCommand PopulateListCommand { get; private set; }
    }
}
