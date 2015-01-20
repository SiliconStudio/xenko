// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IComputeColor"/>
    /// </summary>
    [DataContract("ComputeBinaryColor")]
    [Display("Binary Operator")]
    public class ComputeBinaryColor : ComputeBinaryBase<IComputeColor>, IComputeColor
    {
        public ComputeBinaryColor()
        {
        }

        public ComputeBinaryColor(IComputeColor leftChild, IComputeColor rightChild, BinaryOperand binaryOperand)
            : base(leftChild, rightChild, binaryOperand)
        {
        }
    }
}