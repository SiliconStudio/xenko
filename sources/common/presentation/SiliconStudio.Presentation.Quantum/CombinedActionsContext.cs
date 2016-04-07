using System;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Quantum
{
    public class CombinedActionsContext : IDisposable
    {
        private readonly string observableNodePath;
        private readonly ObservableViewModel owner;
        private readonly ITransaction transaction;

        public CombinedActionsContext(ObservableViewModel owner, string actionName, string observableNodePath)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            var service = owner.ServiceProvider.TryGet<IUndoRedoService>();
            if (service != null)
            {
                transaction = service.CreateTransaction();
                service.SetName(transaction, actionName);
            }
            this.owner = owner;
            this.observableNodePath = observableNodePath;
        }

        public void Dispose()
        {
            owner.EndCombinedAction(observableNodePath);
            transaction?.Complete();
        }
    }
}
