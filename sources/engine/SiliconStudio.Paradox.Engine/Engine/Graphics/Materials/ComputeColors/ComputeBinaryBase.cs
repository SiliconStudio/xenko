// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IComputeNode"/>
    /// </summary>
    [DataContract(Inherited = true)]
    [Display("Binary Operator")]
    public abstract class ComputeBinaryBase<T> : ComputeNode where T : class, IComputeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeBinaryBase{T}"/> class.
        /// </summary>
        protected ComputeBinaryBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeBinaryBase{T}"/> class.
        /// </summary>
        /// <param name="leftChild">The left child.</param>
        /// <param name="rightChild">The right child.</param>
        /// <param name="binaryOperand">The material binary operand.</param>
        protected ComputeBinaryBase(T leftChild, T rightChild, BinaryOperand binaryOperand)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            Operand = binaryOperand;
        }

        /// <summary>
        /// The operation to blend the nodes.
        /// </summary>
        /// <userdoc>
        /// The operation between the background (LeftChild) and the foreground (RightChild).
        /// </userdoc>
        [DataMember(10)]
        public BinaryOperand Operand { get; set; }

        /// <summary>
        /// The left (background) child node.
        /// </summary>
        /// <userdoc>
        /// The background color mapping.
        /// </userdoc>
        [DataMember(20)]
        public T LeftChild { get; set; }

        /// <summary>
        /// The right (foreground) child node.
        /// </summary>
        /// <userdoc>
        /// The foreground color mapping.
        /// </userdoc>
        [DataMember(30)]
        public T RightChild { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            if (LeftChild != null)
            	yield return LeftChild;
            if (RightChild != null)
           	    yield return RightChild;
        }

        private const string BackgroundCompositionName = "color1";
        private const string ForegroundCompositionName = "color2";

        public override ShaderSource GenerateShaderSource(MaterialGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var leftShaderSource = LeftChild.GenerateShaderSource(context, baseKeys);
            var rightShaderSource = RightChild.GenerateShaderSource(context, baseKeys);

            var shaderSource = new ShaderClassSource(GetCorrespondingShaderSourceName(Operand));
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderSource);
            if (leftShaderSource != null)
                mixin.AddComposition(BackgroundCompositionName, leftShaderSource);
            if (Operand != BinaryOperand.None && Operand != BinaryOperand.Opaque && rightShaderSource != null)
                mixin.AddComposition(ForegroundCompositionName, rightShaderSource);

            return mixin;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Binary operation";
        }

        /// <summary>
        /// Get the name of the ShaderClassSource corresponding to the operation
        /// </summary>
        /// <param name="binaryOperand">The operand.</param>
        /// <returns>The name of the ShaderClassSource.</returns>
        private static string GetCorrespondingShaderSourceName(BinaryOperand binaryOperand)
        {
            switch (binaryOperand)
            {
                case BinaryOperand.Add:
                    return "ComputeColorAdd3ds"; //TODO: change this (ComputeColorAdd?)
                case BinaryOperand.Average:
                    return "ComputeColorAverage";
                case BinaryOperand.Color:
                    return "ComputeColorColor";
                case BinaryOperand.ColorBurn:
                    return "ComputeColorColorBurn";
                case BinaryOperand.ColorDodge:
                    return "ComputeColorColorDodge";
                case BinaryOperand.Darken:
                    return "ComputeColorDarken3ds"; //"ComputeColorDarkenMaya" //TODO: change this
                case BinaryOperand.Desaturate:
                    return "ComputeColorDesaturate";
                case BinaryOperand.Difference:
                    return "ComputeColorDifference3ds"; //"ComputeColorDifferenceMaya" //TODO: change this
                case BinaryOperand.Divide:
                    return "ComputeColorDivide";
                case BinaryOperand.Exclusion:
                    return "ComputeColorExclusion";
                case BinaryOperand.HardLight:
                    return "ComputeColorHardLight";
                case BinaryOperand.HardMix:
                    return "ComputeColorHardMix";
                case BinaryOperand.Hue:
                    return "ComputeColorHue";
                case BinaryOperand.Illuminate:
                    return "ComputeColorIlluminate";
                case BinaryOperand.In:
                    return "ComputeColorIn";
                case BinaryOperand.Lighten:
                    return "ComputeColorLighten3ds"; //"ComputeColorLightenMaya" //TODO: change this
                case BinaryOperand.LinearBurn:
                    return "ComputeColorLinearBurn";
                case BinaryOperand.LinearDodge:
                    return "ComputeColorLinearDodge";
                case BinaryOperand.Mask:
                    return "ComputeColorMask";
                case BinaryOperand.Multiply:
                    return "ComputeColorMultiply"; //return "ComputeColorMultiply3ds"; //"ComputeColorMultiplyMaya" //TODO: change this
                case BinaryOperand.None:
                    return "ComputeColorNone";
                case BinaryOperand.Opaque:
                    return "ComputeColorOpaque";
                case BinaryOperand.Out:
                    return "ComputeColorOut";
                case BinaryOperand.Over:
                    return "ComputeColorOver3ds"; //TODO: change this to "ComputeColorLerpAlpha"
                case BinaryOperand.Overlay:
                    return "ComputeColorOverlay3ds"; //"ComputeColorOverlayMaya" //TODO: change this
                case BinaryOperand.PinLight:
                    return "ComputeColorPinLight";
                case BinaryOperand.Saturate:
                    return "ComputeColorSaturate";
                case BinaryOperand.Saturation:
                    return "ComputeColorSaturation";
                case BinaryOperand.Screen:
                    return "ComputeColorScreen";
                case BinaryOperand.SoftLight:
                    return "ComputeColorSoftLight";
                case BinaryOperand.Subtract:
                    return "ComputeColorSubtract"; // "ComputeColorSubtract3ds" "ComputeColorSubtractMaya" //TODO: change this
                case BinaryOperand.SubstituteAlpha:
                    return "ComputeColorSubstituteAlpha";
                default:
                    throw new ArgumentOutOfRangeException("binaryOperand");
            }
        }
    }
}
