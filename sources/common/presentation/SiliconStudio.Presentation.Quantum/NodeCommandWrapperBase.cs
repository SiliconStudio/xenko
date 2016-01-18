// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A base class to wrap one or multiple <see cref="INodeCommand"/> instances into a <see cref="CancellableCommandBase"/>.
    /// </summary>
    public abstract class NodeCommandWrapperBase : CommandBase, INodeCommandWrapper
    {
        private readonly IViewModelServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCommandWrapperBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IActionStack"/> to use for this view model.</param>
        /// <param name="dirtiables">The <see cref="IDirtiable"/> instances associated to this command.</param>
        protected NodeCommandWrapperBase(IViewModelServiceProvider serviceProvider, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Dirtiables = dirtiables;
        }

        /// <summary>
        /// The name of this command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the <see cref="IDirtiable"/> object affected by this command wrapper.
        /// </summary>
        public IEnumerable<IDirtiable> Dirtiables { get; }

        /// <summary>
        /// Gets the how to combine a set of <see cref="NodeCommandWrapperBase"/> in a <see cref="CombinedObservableNode"/>
        /// </summary>
        public abstract CombineMode CombineMode { get; }

        /// <summary>
        /// Gets the action stack.
        /// </summary>
        private IActionStack ActionStack => serviceProvider.Get<IActionStack>();

        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            Invoke(parameter).Forget();
        }

        /// <summary>
        /// Invokes the command and return a token that can be used to undo it.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>An <see cref="UndoToken"/> that can be used to undo the command.</returns>
        public async Task<UndoToken> Invoke(object parameter)
        {
            var transactionalActionStack = ActionStack as ITransactionalActionStack;
            transactionalActionStack?.BeginTransaction();
            var token = await InvokeInternal(parameter);
            transactionalActionStack?.EndTransaction($"Executed {Name}");
            return token;
        }

        /// <summary>
        /// Invokes the command and return a token that can be used to undo it.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>An <see cref="UndoToken"/> that can be used to undo the command.</returns>
        /// <remarks>This method is internally called by <see cref="Invoke"/>.</remarks>
        protected abstract Task<UndoToken> InvokeInternal(object parameter);
    }
}
