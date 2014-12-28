// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="Materials.MaterialComputeColor"/>
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<MaterialBinaryComputeColor>))]
    [DataContract("MaterialBinaryNode")]
    [Display("Binary Operator")]
    public class MaterialBinaryComputeColor : MaterialComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryComputeColor"/> class.
        /// </summary>
        public MaterialBinaryComputeColor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryComputeColor"/> class.
        /// </summary>
        /// <param name="leftChild">The left child.</param>
        /// <param name="rightChild">The right child.</param>
        /// <param name="materialBinaryOperand">The material binary operand.</param>
        public MaterialBinaryComputeColor(Materials.MaterialComputeColor leftChild, Materials.MaterialComputeColor rightChild, MaterialBinaryOperand materialBinaryOperand)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            Operand = materialBinaryOperand;
        }

        /// <summary>
        /// The operation to blend the nodes.
        /// </summary>
        /// <userdoc>
        /// The operation between the background (LeftChild) and the foreground (RightChild).
        /// </userdoc>
        [DataMember(10)]
        public MaterialBinaryOperand Operand { get; set; }

        /// <summary>
        /// The left (background) child node.
        /// </summary>
        /// <userdoc>
        /// The background color mapping.
        /// </userdoc>
        [DataMember(20)]
        public Materials.MaterialComputeColor LeftChild { get; set; }

        /// <summary>
        /// The right (foreground) child node.
        /// </summary>
        /// <userdoc>
        /// The foreground color mapping.
        /// </userdoc>
        [DataMember(30)]
        public Materials.MaterialComputeColor RightChild { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<Materials.MaterialComputeColor> GetChildren(object context = null)
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
            if (Operand != MaterialBinaryOperand.None && Operand != MaterialBinaryOperand.Opaque && rightShaderSource != null)
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
        /// <param name="materialBinaryOperand">The operand.</param>
        /// <returns>The name of the ShaderClassSource.</returns>
        private static string GetCorrespondingShaderSourceName(MaterialBinaryOperand materialBinaryOperand)
        {
            switch (materialBinaryOperand)
            {
                case MaterialBinaryOperand.Add:
                    return "ComputeColorAdd3ds"; //TODO: change this (ComputeColorAdd?)
                case MaterialBinaryOperand.Average:
                    return "ComputeColorAverage";
                case MaterialBinaryOperand.Color:
                    return "ComputeColorColor";
                case MaterialBinaryOperand.ColorBurn:
                    return "ComputeColorColorBurn";
                case MaterialBinaryOperand.ColorDodge:
                    return "ComputeColorColorDodge";
                case MaterialBinaryOperand.Darken:
                    return "ComputeColorDarken3ds"; //"ComputeColorDarkenMaya" //TODO: change this
                case MaterialBinaryOperand.Desaturate:
                    return "ComputeColorDesaturate";
                case MaterialBinaryOperand.Difference:
                    return "ComputeColorDifference3ds"; //"ComputeColorDifferenceMaya" //TODO: change this
                case MaterialBinaryOperand.Divide:
                    return "ComputeColorDivide";
                case MaterialBinaryOperand.Exclusion:
                    return "ComputeColorExclusion";
                case MaterialBinaryOperand.HardLight:
                    return "ComputeColorHardLight";
                case MaterialBinaryOperand.HardMix:
                    return "ComputeColorHardMix";
                case MaterialBinaryOperand.Hue:
                    return "ComputeColorHue";
                case MaterialBinaryOperand.Illuminate:
                    return "ComputeColorIlluminate";
                case MaterialBinaryOperand.In:
                    return "ComputeColorIn";
                case MaterialBinaryOperand.Lighten:
                    return "ComputeColorLighten3ds"; //"ComputeColorLightenMaya" //TODO: change this
                case MaterialBinaryOperand.LinearBurn:
                    return "ComputeColorLinearBurn";
                case MaterialBinaryOperand.LinearDodge:
                    return "ComputeColorLinearDodge";
                case MaterialBinaryOperand.Mask:
                    return "ComputeColorMask";
                case MaterialBinaryOperand.Multiply:
                    return "ComputeColorMultiply"; //return "ComputeColorMultiply3ds"; //"ComputeColorMultiplyMaya" //TODO: change this
                case MaterialBinaryOperand.None:
                    return "ComputeColorNone";
                case MaterialBinaryOperand.Opaque:
                    return "ComputeColorOpaque";
                case MaterialBinaryOperand.Out:
                    return "ComputeColorOut";
                case MaterialBinaryOperand.Over:
                    return "ComputeColorOver3ds"; //TODO: change this to "ComputeColorLerpAlpha"
                case MaterialBinaryOperand.Overlay:
                    return "ComputeColorOverlay3ds"; //"ComputeColorOverlayMaya" //TODO: change this
                case MaterialBinaryOperand.PinLight:
                    return "ComputeColorPinLight";
                case MaterialBinaryOperand.Saturate:
                    return "ComputeColorSaturate";
                case MaterialBinaryOperand.Saturation:
                    return "ComputeColorSaturation";
                case MaterialBinaryOperand.Screen:
                    return "ComputeColorScreen";
                case MaterialBinaryOperand.SoftLight:
                    return "ComputeColorSoftLight";
                case MaterialBinaryOperand.Subtract:
                    return "ComputeColorSubtract3ds"; //"ComputeColorOverlayMaya" //TODO: change this
                case MaterialBinaryOperand.SubstituteAlpha:
                    return "ComputeColorSubstituteAlpha";
                default:
                    throw new ArgumentOutOfRangeException("materialBinaryOperand");
            }
        }
    }
}
