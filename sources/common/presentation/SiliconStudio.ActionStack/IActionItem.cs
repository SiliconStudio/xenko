// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// Base interface for action items.
    /// </summary>
    public interface IActionItem
    {
        /// <summary>
        /// Gets or sets the name of the current action.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets an unique identifier of the action, per instance.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        /// Gets or sets whether this action as already been saved when evaluating the dirtiness of an object.
        /// </summary>
        bool IsSaved { get; set; }

        /// <summary>
        /// Gets whether this action is currently done. Modified when invoking <see cref="Undo"/> or <see cref="Redo"/>.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Gets whether this action has been frozen with the <see cref="Freeze"/> method.
        /// </summary>
        bool IsFrozen { get; }

        /// <summary>
        /// Freezes this ActionItem. A frozen action item can't be undone anymore and should have freed the resources stored for undo actions
        /// </summary>
        void Freeze();

        /// <summary>
        /// Undo the action.
        /// </summary>
        void Undo();

        /// <summary>
        /// Redo the action.
        /// </summary>
        void Redo();
    }
}
