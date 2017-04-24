// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    /// <summary>
    /// Arguments of the <see cref="INodePresenter.ValueChanged"/> event.
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldValue">The old value of the node.</param>
        public ValueChangedEventArgs(object oldValue)
        {
            OldValue = oldValue;
        }

        /// <summary>
        /// The old value of the node.
        /// </summary>
        public object OldValue { get; }
    }
}
