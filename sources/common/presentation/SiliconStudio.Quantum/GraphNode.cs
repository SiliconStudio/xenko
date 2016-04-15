// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is the default implementation of the <see cref="IGraphNode"/>.
    /// </summary>
    public class GraphNode : IGraphNode
    {
        private readonly List<IGraphNode> children = new List<IGraphNode>();
        private readonly List<INodeCommand> commands = new List<INodeCommand>();
        private bool isSealed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNode"/> class.
        /// </summary>
        /// <param name="name">The name of this node.</param>
        /// <param name="content">The content of this node.</param>
        /// <param name="guid">An unique identifier for this node.</param>
        public GraphNode(string name, IContent content, Guid guid)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (guid == Guid.Empty) throw new ArgumentException(@"The guid must be different from Guid.Empty.", nameof(content));
            Content = content;
            Name = name;
            Guid = guid;

            var updatableContent = content as ContentBase;
            updatableContent?.RegisterOwner(this);
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public Guid Guid { get; }

        /// <inheritdoc/>
        public virtual IContent Content { get; }

        /// <inheritdoc/>
        public virtual IGraphNode Parent { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IGraphNode> Children => children;

        /// <inheritdoc/>
        public IReadOnlyCollection<INodeCommand> Commands => commands;

        /// <summary>
        /// Add a child to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IContent.Reference"/> is not null.</param>
        public void AddChild(GraphNode child, bool allowIfReference = false)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

            if (child.Parent != null)
                throw new ArgumentException(@"This node has already been registered to a different parent", nameof(child));

            if (Content.Reference != null && !allowIfReference)
                throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

            child.Parent = this;
            children.Add(child);
        }

        /// <summary>
        /// Add a command to this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to add.</param>
        public void AddCommand(INodeCommand command)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a command to a GraphNode that has been sealed");

            commands.Add(command);
        }

        /// <summary>
        /// Remove a command from this node. The node must not have been sealed yet.
        /// </summary>
        /// <param name="command">The node command to remove.</param>
        public void RemoveCommand(INodeCommand command)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to remove a command from a GraphNode that has been sealed");

            commands.Remove(command);
        }

        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children or commands will be added.
        /// </summary>
        public void Seal()
        {
            isSealed = true;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}: [{Content.Value}]";
        }
    }
}
