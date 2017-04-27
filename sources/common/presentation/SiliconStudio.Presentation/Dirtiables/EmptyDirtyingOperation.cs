// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Dirtiables
{
    public sealed class EmptyDirtyingOperation : DirtyingOperation
    {
        public EmptyDirtyingOperation([NotNull] IEnumerable<IDirtiable> dirtiables)
            : base(dirtiables)
        {
        }

        /// <inheritdoc/>
        protected override void Undo()
        {
        }

        /// <inheritdoc/>
        protected override void Redo()
        {
        }
    }
}
