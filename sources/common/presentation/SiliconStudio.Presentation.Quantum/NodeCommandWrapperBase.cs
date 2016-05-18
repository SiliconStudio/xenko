// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class NodeCommandWrapperBase : CommandBase, INodeCommandWrapper
    {
        private readonly IViewModelServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCommandWrapperBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IUndoRedoService"/> to use for this view model.</param>
        protected NodeCommandWrapperBase(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// The name of the action executed by this command.
        /// </summary>
        public virtual string ActionName => $"Execute {Name}";

        /// <summary>
        /// The name of this command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the how to combine a set of <see cref="NodeCommandWrapperBase"/> in a <see cref="CombinedObservableNode"/>
        /// </summary>
        public abstract CombineMode CombineMode { get; }

        /// <summary>
        /// Gets the action stack.
        /// </summary>
        protected IUndoRedoService ActionService => serviceProvider.TryGet<IUndoRedoService>();

        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            Invoke(parameter).Forget();
        }

        /// <summary>
        /// Invokes the command and return a token that can be used to undo it.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that completes when the command has finished.</returns>
        public abstract Task Invoke(object parameter);

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            return !string.IsNullOrEmpty(name) ? $"{{{GetType().Name}: {name}}}" : base.ToString();
        }
    }
}
