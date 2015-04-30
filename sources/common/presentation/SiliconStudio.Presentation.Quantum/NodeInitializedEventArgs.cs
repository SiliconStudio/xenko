using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// Arguments for the <see cref="ObservableViewModelService.NodeInitialized"/> event.
    /// 
    /// </summary>
    public class NodeInitializedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInitializedEventArgs"/>
        /// </summary>
        /// <param name="node">The node that has been initialized.</param>
        public NodeInitializedEventArgs(SingleObservableNode node)
        {
            Node = node;
        }

        /// <summary>
        /// Gets the node that has been initialized.
        /// </summary>
        public SingleObservableNode Node { get; private set; }
    }
}