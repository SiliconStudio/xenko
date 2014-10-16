using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;

// This file contain a simple view model class that inherits from our DispatcherViewModel.
// Property writing is automatically dispatched to the UI thread.

namespace SiliconStudio.Presentation.Sample
{
    public class SimpleViewModel : DispatcherViewModel
    {
        private int intValue = 5;
        private double doubleValue = 3.14;
        private string stringValue;

        public SimpleViewModel(IDispatcherService dispatcher) : base(dispatcher)
        {
        }

        public int IntValue { get { return intValue; } set { SetValue(ref intValue, value); } }

        public double DoubleValue { get { return doubleValue; } set { SetValue(ref doubleValue, value); } }

        public string StringValue { get { return stringValue; } set { SetValue(ref stringValue, value); } }
    }
}