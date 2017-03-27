using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    /// <summary>
    /// Arguments of the <see cref="INodePresenter.ValueChanging"/> event.
    /// </summary>
    public class ValueChangingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueChangingEventArgs"/> class.
        /// </summary>
        /// <param name="newValue">The new value of the node.</param>
        public ValueChangingEventArgs(object newValue)
        {
            NewValue = newValue;
        }

        /// <summary>
        /// The new value of the node.
        /// </summary>
        public object NewValue { get; private set; }
    }
}