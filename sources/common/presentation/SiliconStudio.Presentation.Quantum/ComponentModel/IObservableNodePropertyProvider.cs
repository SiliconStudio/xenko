// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

namespace SiliconStudio.Presentation.Quantum.ComponentModel
{
    /// <summary>
    /// An interface that can provide a <see cref="PropertyDescriptorCollection"/> for a given <see cref="ObservableNode"/>.
    /// </summary>
    public interface IObservableNodePropertyProvider
    {
        /// <summary>
        /// Returns a <see cref="PropertyDescriptorCollection"/> instance describing the property of the node associated to this instance
        /// of <see cref="IObservableNodePropertyProvider"/>.
        /// </summary>
        PropertyDescriptorCollection GetProperties();
    }
}