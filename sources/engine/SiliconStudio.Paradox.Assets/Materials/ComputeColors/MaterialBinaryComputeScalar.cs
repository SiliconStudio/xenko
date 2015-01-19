// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IMaterialComputeScalar"/>
    /// </summary>
    [DataContract("MaterialBinaryComputeScalar")]
    [Display("Binary Operator")]
    public class MaterialBinaryComputeScalar : MaterialBinaryComputeNodeBase<IMaterialComputeScalar>, IMaterialComputeScalar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryComputeScalar"/> class.
        /// </summary>
        public MaterialBinaryComputeScalar()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryComputeScalar"/> class.
        /// </summary>
        /// <param name="leftChild">The left child.</param>
        /// <param name="rightChild">The right child.</param>
        /// <param name="materialBinaryOperand">The material binary operand.</param>
        public MaterialBinaryComputeScalar(IMaterialComputeScalar leftChild, IMaterialComputeScalar rightChild, MaterialBinaryOperand materialBinaryOperand)
            : base(leftChild, rightChild, materialBinaryOperand)
        {
        }
    }
}