using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Sample4.Model;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Sample4.ViewModel
{
    public class SimpleViewModel : SiSdkViewModel
    {
        private readonly SimpleModel model;

        public SimpleViewModel(ITransactionalActionStack actionStack, IDispatcherService dispatcher, ICommandOpManager cmdManager, SimpleModel model)
            : base(actionStack, dispatcher, cmdManager)
        {
            this.model = model;
        }

        public int IntValue { get { return model.IntValue; } set { SetValue(IntValue != value, () => InvokeSetProperty(value)); } }
    }
}
