// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IGraphNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : IDisposable
    {
        private readonly IGraphNode rootNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeChangeListener"/> class.
        /// </summary>
        /// <param name="rootNode">The root node for which to track referenced node changes.</param>
        public GraphNodeChangeListener(IGraphNode rootNode)
        {
            this.rootNode = rootNode;
            foreach (var node in rootNode.GetAllChildNodes())
            {
                node.Content.Changing += ContentChanging;
                node.Content.Changed += ContentChanged;
            }
        }

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<ContentChangeEventArgs> Changing;

        /// <summary>
        /// Raised after one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<ContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var node in rootNode.GetAllChildNodes())
            {
                node.Content.Changing -= ContentChanging;
                node.Content.Changed -= ContentChanged;
            }
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content.OwnerNode as IGraphNode;
            if (node != null)
            {
                foreach (var child in node.GetAllChildNodes())
                {
                    child.Content.Changing -= ContentChanging;
                    child.Content.Changed -= ContentChanged;
                }
            }

            Changing?.Invoke(sender, e);
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content.OwnerNode as IGraphNode;
            if (node != null)
            {
                foreach (var child in node.GetAllChildNodes())
                {
                    child.Content.Changing += ContentChanging;
                    child.Content.Changed += ContentChanged;
                }
            }

            Changed?.Invoke(sender, e);
        }
    }
}