// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public class AnonymousCommandWrapper : NodeCommandWrapperBase
    {
        private readonly Func<object, UndoToken> doRedo;
        private readonly Action<UndoToken> undo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommandWrapper"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        /// <param name="name">The name of this command.</param>
        /// <param name="combineMode">The combine mode to apply to this command.</param>
        /// <param name="doRedo">The do/redo function.</param>
        /// <param name="undo">The undo action, if the command can be undone.</param>
        /// <param name="dirtiables">The <see cref="IDirtiable"/> instances associated to this command.</param>
        public AnonymousCommandWrapper(IViewModelServiceProvider serviceProvider, string name, CombineMode combineMode, Func<object, UndoToken> doRedo, Action<UndoToken> undo, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider, dirtiables)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (doRedo == null) throw new ArgumentNullException(nameof(doRedo));
            Name = name;
            CombineMode = combineMode;
            this.doRedo = doRedo;
            this.undo = undo;
        }

        /// <inheritdoc/>
        public override string Name { get; }

        protected override Task<UndoToken> InvokeInternal(object parameter)
        {
            var token = doRedo(parameter);
            return Task.FromResult(new UndoToken(undo != null, new NodeCommandBase.TokenData(parameter, token)));
        }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get; }
    }
}