// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An interface representing an object notifying changes when an item in the value of a related node is modified, added or removed.
    /// </summary>
    public interface INotifyNodeItemChange
    {
        /// <summary>
        /// Raised just before a change to the related node occurs.
        /// </summary>
        event EventHandler<ItemChangeEventArgs> ItemChanging;

        /// <summary>
        /// Raised when a change to the related node has occurred.
        /// </summary>
        event EventHandler<ItemChangeEventArgs> ItemChanged;
    }
}
