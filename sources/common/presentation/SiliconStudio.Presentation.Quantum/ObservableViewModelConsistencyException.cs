// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// An exception that occurs during consistency checks of ObservableViewModel nodes, indicating that an <see cref="ObservableNode"/> is un an unexpected state.
    /// </summary>
    public class ObservableViewModelConsistencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ObservableViewModelConsistencyException class.
        /// </summary>
        /// <param name="node">The node that is related to this error.</param>
        /// <param name="messageFormat">A composite format string that describes the error.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public ObservableViewModelConsistencyException(ObservableNode node, string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
            Node = node;

        }

        /// <summary>
        /// Gets the <see cref="ObservableNode"/> that triggered this exception.
        /// </summary>
        public ObservableNode Node { get; private set; }
    }
}