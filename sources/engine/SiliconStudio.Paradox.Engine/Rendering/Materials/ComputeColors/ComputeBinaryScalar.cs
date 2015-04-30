// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IComputeScalar"/>
    /// </summary>
    [DataContract("ComputeBinaryScalar")]
    [Display("Binary Operator")]
    public class ComputeBinaryScalar : ComputeBinaryBase<IComputeScalar>, IComputeScalar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeBinaryScalar"/> class.
        /// </summary>
        public ComputeBinaryScalar()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeBinaryScalar"/> class.
        /// </summary>
        /// <param name="leftChild">The left child.</param>
        /// <param name="rightChild">The right child.</param>
        /// <param name="binaryOperator">The material binary operand.</param>
        public ComputeBinaryScalar(IComputeScalar leftChild, IComputeScalar rightChild, BinaryOperator binaryOperator)
            : base(leftChild, rightChild, binaryOperator)
        {
        }
    }
}