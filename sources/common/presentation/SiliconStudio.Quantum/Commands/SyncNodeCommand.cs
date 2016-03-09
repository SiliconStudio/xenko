using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class SyncNodeCommand : NodeCommandBase
    {
        public sealed override Task<IActionItem> Execute(IContent content, object index, object parameter)
        {
            var result = ExecuteSync(content, index, parameter);
            return Task.FromResult(result);
        }

        protected abstract IActionItem ExecuteSync(IContent content, object index, object parameter);
    }
}