using System;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Quantum
{
    public class CombinedActionsContext : IDisposable
    {
        private readonly string nodePath;
        private readonly GraphViewModel owner;
        private readonly ITransaction transaction;

        public CombinedActionsContext(GraphViewModel owner, string actionName, string nodePath)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            var service = owner.ServiceProvider.TryGet<IUndoRedoService>();
            if (service != null)
            {
                transaction = service.CreateTransaction();
                service.SetName(transaction, actionName);
            }
            this.owner = owner;
            this.nodePath = nodePath;
        }

        public void Dispose()
        {
            owner.EndCombinedAction(nodePath);
            transaction?.Complete();
        }
    }
}
