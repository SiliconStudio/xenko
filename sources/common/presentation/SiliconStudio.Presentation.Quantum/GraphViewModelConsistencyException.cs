// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// An exception that occurs during consistency checks of <see cref="GraphViewModel"/> nodes, indicating that an <see cref="NodeViewModel"/> is un an unexpected state.
    /// </summary>
    public class GraphViewModelConsistencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the GraphViewModelConsistencyException class.
        /// </summary>
        /// <param name="node">The node that is related to this error.</param>
        /// <param name="messageFormat">A composite format string that describes the error.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public GraphViewModelConsistencyException(NodeViewModel node, string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
            Node = node;

        }

        /// <summary>
        /// Gets the <see cref="NodeViewModel"/> that triggered this exception.
        /// </summary>
        public NodeViewModel Node { get; private set; }
    }
}