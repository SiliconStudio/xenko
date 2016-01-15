using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    [Obsolete("Cancellable command system will be removed soon and each command will have responsibility to create action items.")]
    public abstract class CancellableCommandBase : CommandBase, ICancellableCommandBase
    {
        private readonly IViewModelServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableCommandBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IActionStack"/> to use for this view model.</param>
        /// <param name="dirtiables">The <see cref="IDirtiable"/> instances associated to this command.</param>
        protected CancellableCommandBase(IViewModelServiceProvider serviceProvider, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider)
        {
            if (dirtiables == null) throw new ArgumentNullException(nameof(dirtiables));
            this.serviceProvider = serviceProvider;
            Dirtiables = dirtiables;
        }

        /// <summary>
        /// The name of this command.
        /// </summary>
        public abstract string Name { get; }

        public IEnumerable<IDirtiable> Dirtiables { get; }

        private IActionStack ActionStack => serviceProvider.Get<IActionStack>();

        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            Invoke(parameter).Forget();
        }

        /// <summary>
        /// Executes the command and return a token that can be used to undo it.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>An <see cref="UndoToken"/> that can be used to undo the command.</returns>
        public async Task<UndoToken> Invoke(object parameter)
        {
            var transactionalActionStack = ActionStack as ITransactionalActionStack;
            transactionalActionStack?.BeginTransaction();
            var token = await Do(parameter);
            transactionalActionStack?.EndTransaction($"Executed {Name}");
            return token;
        }

        public abstract RedoToken Undo(UndoToken undoToken);

        public abstract UndoToken Redo(RedoToken redoToken);

        protected abstract Task<UndoToken> Do(object parameter);
    }
}