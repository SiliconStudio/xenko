// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Arguments of the <see cref="IMemberNode.ValueChanging"/> and <see cref="IMemberNode.ValueChanged"/> events.
    /// </summary>
    public class MemberNodeChangeEventArgs : EventArgs, INodeChangeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberNodeChangeEventArgs"/> class.
        /// </summary>
        /// <param name="member">The member that has changed.</param>
        /// <param name="oldValue">The old value of the member that has changed.</param>
        /// <param name="newValue">The new value of the member that has changed.</param>
        public MemberNodeChangeEventArgs([NotNull] IMemberNode member, object oldValue, object newValue)
        {
            Member = member;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the member node that has changed.
        /// </summary>
        [NotNull]
        public IMemberNode Member { get; }

        /// <summary>
        /// The type of change.
        /// </summary>
        public ContentChangeType ChangeType => ContentChangeType.ValueChange;

        /// <summary>
        /// Gets the old value of the member or the item of the member that has changed.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the new value of the member or the item of the member that has changed.
        /// </summary>
        public object NewValue { get; }

        /// <inheritdoc/>
        IGraphNode INodeChangeEventArgs.Node => Member;
    }
}
