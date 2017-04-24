// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An enum that describes what to do with a node or a command when combining view models.
    /// </summary>
    public enum CombineMode
    {
        /// <summary>
        /// The command or the node should never be combined.
        /// </summary>
        DoNotCombine,
        /// <summary>
        /// The command should always be combined, even if some of the combined nodes do not have it.
        /// The nodes should always be combined, even if some single view models does not have it.
        /// </summary>
        AlwaysCombine,
        /// <summary>
        /// The command should be combined only if all combined nodes have it.
        /// The nodes should be combined only if all single view models have it.
        /// </summary>
        CombineOnlyForAll,
    }
}
