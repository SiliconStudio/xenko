using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    public abstract class CancellableCommandBase : CommandBase, ICancellableCommandBase
    {
        private readonly IViewModelServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IActionStack"/> to use for this view model.</param>
        /// <param name="dirtiables">The <see cref="IDirtiable"/> instances associated to this command.</param>
        protected CancellableCommandBase(IViewModelServiceProvider serviceProvider, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Dirtiables = dirtiables;
        }

        /// <summary>
        /// The name of this command.
        /// </summary>
        public abstract string Name { get; }

        protected IEnumerable<IDirtiable> Dirtiables { get; }

        private IActionStack ActionStack => serviceProvider.Get<IActionStack>();

        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            Invoke(parameter);
        }

        /// <summary>
        /// Executes the command and return a token that can be used to undo it.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>An <see cref="UndoToken"/> that can be used to undo the command.</returns>
        [Obsolete("This method will become private soon")]
        public IActionItem Invoke(object parameter)
        {
            // TODO: Improve this - we're discarding any change made directly by the command invoke and create a CommandActionItem after.
            // NOTE: PickupAssetCommand is currently assuming that there's such a transaction in progress, be sure to check it if changing this.
            var transactionalActionStack = ActionStack as ITransactionalActionStack;
            // This is needed only for the Do invocation. Action items created during undo/redo, are automatically discarded by design.
            transactionalActionStack?.BeginTransaction();
            var token = Do(parameter);
            transactionalActionStack?.DiscardTransaction();
            return CreateActionItem(token);
        }

        public abstract RedoToken Undo(UndoToken undoToken);

        public abstract UndoToken Redo(RedoToken redoToken);

        protected abstract UndoToken Do(object parameter);

        protected virtual IActionItem CreateActionItem(UndoToken token)
        {
            if (!token.CanUndo)
                return null;

            var actionItem = new CancellableCommandActionItem(this, token, Dirtiables);
            ActionStack.Add(actionItem);
            return actionItem;
        }
    }
}