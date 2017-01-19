// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Arguments for the <see cref="GraphViewModelService.NodeInitialized"/> event.
    /// 
    /// </summary>
    public class NodeInitializedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInitializedEventArgs"/>
        /// </summary>
        /// <param name="node">The node that has been initialized.</param>
        public NodeInitializedEventArgs(SingleNodeViewModel node)
        {
            Node = node;
        }

        /// <summary>
        /// Gets the node that has been initialized.
        /// </summary>
        public SingleNodeViewModel Node { get; private set; }
    }
}