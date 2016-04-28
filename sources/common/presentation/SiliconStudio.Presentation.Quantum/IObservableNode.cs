// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public interface IObservableNode : INotifyPropertyChanging, INotifyPropertyChanged, IDestroyable
    {
        /// <summary>
        /// Gets the <see cref="ObservableViewModel"/> that owns this node.
        /// </summary>
        ObservableViewModel Owner { get; }

        /// <summary>
        /// Gets or sets the name of this node. Note that the name can be used to access this node from its parent using a dynamic object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the name used to display the node to the user.
        /// </summary>
        string DisplayName { get; set; }
        
        /// <summary>
        /// Gets the path of this node. The path is constructed from the name of all node from the root to this one, separated by periods.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets or the parent of this node.
        /// </summary>
        IObservableNode Parent { get; }

        /// <summary>
        /// Gets the root of this node.
        /// </summary>
        IObservableNode Root { get; }

        /// <summary>
        /// Gets the expected type of <see cref="Value"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets or sets whether this node should be displayed in the view.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets whether this node can be modified in the view.
        /// </summary>
        bool IsReadOnly { get; set; }
        
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets the index of this node, relative to its parent node when its contains a collection.
        /// Can be <see cref="SiliconStudio.Quantum.Index.Empty"/> if this node is not in a collection.
        /// </summary>
        Index Index { get; }

        /// <summary>
        /// Gets a unique identifier associated to this node.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the list of children nodes.
        /// </summary>
        IReadOnlyCollection<IObservableNode> Children { get; }

        /// <summary>
        /// Gets the list of commands available in this node.
        /// </summary>
        IEnumerable<INodeCommandWrapper> Commands { get; }

        /// <summary>
        /// Gets additional data associated to this content. This can be used when the content itself does not contain enough information to be used as a view model.
        /// </summary>
        IReadOnlyDictionary<string, object> AssociatedData { get; }

        /// <summary>
        /// Gets the level of depth of this node, starting from 0 for the root node.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Gets the order number of this node in its parent.
        /// </summary>
        int? Order { get; }

        /// <summary>
        /// Gets whether this node contains a list
        /// </summary>
        bool HasList { get; }

        /// <summary>
        /// Gets whether this node contains a dictionary
        /// </summary>
        bool HasDictionary { get; }

        /// <summary>
        /// Gets the number of <see cref="IObservableNode"/> in the <see cref="Children"/> collection that are visible according to their <see cref="IsVisible"/> property.
        /// </summary>
        int VisibleChildrenCount { get; }

        /// <summary>
        /// Raised when the <see cref="Value"/> property has changed.
        /// </summary>
        event EventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Raised when the <see cref="IsVisible"/> property has changed.
        /// </summary>
        event EventHandler<EventArgs> IsVisibleChanged;

        /// <summary>
        /// Returns the child node with the matching name.
        /// </summary>
        /// <param name="name">The name of the <see cref="ObservableNode"/> to look for.</param>
        /// <returns>The corresponding child node, or <c>null</c> if no child with the given name exists.</returns>
        ObservableNode GetChild(string name);

        /// <summary>
        /// Returns the command with the matching name.
        /// </summary>
        /// <param name="name">The name of the command to look for.</param>
        /// <returns>The corresponding command, or <c>null</c> if no command with the given name exists.</returns>
        ICommandBase GetCommand(string name);

        /// <summary>
        /// Indicates whether this node can be moved.
        /// </summary>
        /// <param name="newParent">The new parent of the node once moved.</param>
        /// <returns><c>true</c> if the node can be moved, <c>fals</c> otherwise.</returns>
        bool CanMove(IObservableNode newParent);

        /// <summary>
        /// Moves the node by setting it a new parent.
        /// </summary>
        /// <param name="newParent">The new parent of the node once moved.</param>
        /// <param name="newName">The new name to give to the node once moved. This will modify its path. If <c>null</c>, it does not modify the name.</param>
        void Move(IObservableNode newParent, string newName = null);
    }
}
