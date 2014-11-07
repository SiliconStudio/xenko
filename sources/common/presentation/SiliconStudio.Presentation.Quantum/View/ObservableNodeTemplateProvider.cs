// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Presentation.View;

namespace SiliconStudio.Presentation.Quantum.View
{
    /// <summary>
    /// A base class for implementations of <see cref="ITemplateProvider"/> that can provide templates for <see cref="IObservableNode"/> instances.
    /// </summary>
    public abstract class ObservableNodeTemplateProvider : TemplateProviderBase
    {
        /// <inheritdoc/>
        public override bool Match(object obj)
        {
            return obj is IObservableNode && MatchNode((IObservableNode)obj);
        }

        /// <summary>
        /// Indicates whether this instance of <see cref="ITemplateProvider"/> can provide a template for the given <see cref="IObservableNode"/>.
        /// </summary>
        /// <param name="node">The node to test.</param>
        /// <returns><c>true</c> if this template provider can provide a template for the given node, <c>false</c> otherwise.</returns>    
        /// <remarks>This method is invoked by <see cref="Match"/>.</remarks> 
        public abstract bool MatchNode(IObservableNode node);

        /// <summary>
        /// Indicates whether the given node matches the given type, either with the <see cref="IObservableNode.Type"/> property
        /// or the type of the <see cref="IObservableNode.Value"/> property.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="type">The type to match.</param>
        /// <returns><c>true</c> if the node matches the given type, <c>false</c> otherwise.</returns>
        protected static bool MatchType(IObservableNode node, Type type)
        {
            return type.IsAssignableFrom(node.Type) || type.IsInstanceOfType(node.Value);
        }
    }
}
