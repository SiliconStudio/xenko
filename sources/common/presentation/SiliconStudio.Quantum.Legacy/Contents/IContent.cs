// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// Content of a IViewModelNode.
    /// </summary>
    public interface IContent
    {
        /// <summary>
        /// Gets or sets the owner view model node.
        /// </summary>
        /// <value>
        /// The owner view model node.
        /// </value>
        IViewModelNode OwnerNode { get; set; }

        /// <summary>
        /// Gets whether the value can be written.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the expected type of <see cref="Value"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Gets the new value of the content if it has changed since the last access, or <see cref="ContentBase.ValueNotUpdated"/> if it has not.
        /// </summary>
        // TODO: make this better
        object UpdatedValue { get; }

        /// <summary>
        /// Gets or sets the loading state.
        /// </summary>
        ViewModelContentState LoadState { get; set; }

        /// <summary>
        /// Gets or sets the content flags.
        /// </summary>
        ViewModelContentFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the serialization flags.
        /// </summary>
        ViewModelContentSerializeFlags SerializeFlags { get; set; }
    }
}