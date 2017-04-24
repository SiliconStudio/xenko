// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IComputeColor"/>
    /// </summary>
    [DataContract("ComputeBinaryColor")]
    [Display("Binary Operator")]
    public class ComputeBinaryColor : ComputeBinaryBase<IComputeColor>, IComputeColor
    {
        private BinaryOperator cachedOperator;

        public ComputeBinaryColor()
        {
        }

        public ComputeBinaryColor(IComputeColor leftChild, IComputeColor rightChild, BinaryOperator binaryOperator)
            : base(leftChild, rightChild, binaryOperator)
        {
        }

        /// <inheritdoc/>
        public bool HasChanged
        {
            get
            {
                // Null children force skip changes
                if (LeftChild == null || RightChild == null || ((cachedOperator == Operator) && !LeftChild.HasChanged && !RightChild.HasChanged))
                    return false;

                cachedOperator = Operator;
                return true;
            }
        }
    }
}
