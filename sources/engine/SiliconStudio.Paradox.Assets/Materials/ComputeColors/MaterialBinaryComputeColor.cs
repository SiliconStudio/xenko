// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IMaterialComputeColor"/>
    /// </summary>
    [DataContract("MaterialBinaryComputeColor")]
    [Display("Binary Operator")]
    public class MaterialBinaryComputeColor : MaterialBinaryComputeNodeBase<IMaterialComputeColor>, IMaterialComputeColor
    {
        public MaterialBinaryComputeColor()
        {
        }

        public MaterialBinaryComputeColor(IMaterialComputeColor leftChild, IMaterialComputeColor rightChild, MaterialBinaryOperand materialBinaryOperand)
            : base(leftChild, rightChild, materialBinaryOperand)
        {
        }
    }
}