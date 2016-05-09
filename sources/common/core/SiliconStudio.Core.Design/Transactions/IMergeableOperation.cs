// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Core.Transactions
{
    public interface IMergeableOperation
    {
        /// <summary>
        /// Indicates whether the given operation can be merged into this operation.
        /// </summary>
        /// <param name="otherOperation">The operation to merge into this operation.</param>
        /// <returns><c>True</c> if the operation can be merged, <c>False</c> otherwise.</returns>
        /// <remarks>The operation given as argument is supposed to have occurred after this one.</remarks>
        bool CanMerge(IMergeableOperation otherOperation);

        /// <summary>
        /// Merges the given operation into this operation.
        /// </summary>
        /// <param name="otherOperation">The operation to merge into this operation.</param>
        /// <remarks>The operation given as argument is supposed to have occurred after this one.</remarks>
        void Merge(Operation otherOperation);
    }
}
