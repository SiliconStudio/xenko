// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// An abstact class that inherits from <see cref="ActionItem"/> and can synchronize the dirty status of am <see cref="IDirtiable"/> object.
    /// </summary>
    public abstract class DirtiableActionItem : ActionItem
    {
        private readonly List<IDirtiable> dirtiables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableActionItem"/> class with the specified name and dirtiable object.
        /// </summary>
        /// <param name="name">The name of the action item.</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        protected DirtiableActionItem(string name, IEnumerable<IDirtiable> dirtiables)
            : base(name)
        {
            if (dirtiables == null) throw new ArgumentNullException(nameof(dirtiables));
            this.dirtiables = dirtiables.ToList();
        }

        /// <summary>
        /// Gets the dirtiable view model associated to this object, or <c>null</c> if no dirtiable is associated.
        /// </summary>
        public IReadOnlyCollection<IDirtiable> Dirtiables => dirtiables;
    }
}
