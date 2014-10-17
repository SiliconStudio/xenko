// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Contents
{
    [Flags]
    public enum ViewModelContentFlags
    {
        /// <summary>
        /// Empty value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specify if combine (i.e. multi-value) couldn't be merged.
        /// </summary>
        CombineError = 1,
    }

    /// <summary>
    /// Flags applying to <see cref="IContent"/>.
    /// </summary>
    [Flags]
    public enum ViewModelContentSerializeFlags
    {
        /// <summary>
        /// Empty value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specify if IViewModelContent.Value should be serialied.
        /// </summary>
        SerializeValue = 1,

        /// <summary>
        /// Send only one time (Serialize flag is removed after being sent).
        /// </summary>
        Static = 2,

        /// <summary>
        /// Send asynchronously, on another channel (needs to be combined with Static for now).
        /// Not implemented yet.
        /// </summary>
        Async = 4,
    }
}