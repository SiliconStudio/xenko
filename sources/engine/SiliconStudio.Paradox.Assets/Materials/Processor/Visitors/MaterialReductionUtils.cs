// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.Nodes;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    internal class MaterialReductionUtils
    {
        /// <summary>
        /// Perform the operation between two floats on CPU
        /// </summary>
        /// <param name="leftValue">The value of the left child.</param>
        /// <param name="rightValue">The value of the right child.</param>
        /// <param name="operand">The operation between the two values.</param>
        /// <returns>The new float</returns>
        public static float MixFloat(float leftValue, float rightValue, MaterialBinaryOperand operand)
        {
            switch (operand)
            {
                case MaterialBinaryOperand.Add:
                    return leftValue + rightValue;
                case MaterialBinaryOperand.Average:
                    return 0.5f * (leftValue + rightValue);
                case MaterialBinaryOperand.Divide:
                    return leftValue / rightValue;
                case MaterialBinaryOperand.Multiply:
                    return leftValue * rightValue;
                case MaterialBinaryOperand.None:
                case MaterialBinaryOperand.Opaque:
                    return leftValue;
                case MaterialBinaryOperand.Subtract:
                    return leftValue - rightValue;
                default:
                    throw new ArgumentOutOfRangeException("Operand not supported between two floats");
            }
        }

        /// <summary>
        /// Perform the operation between two float4 on CPU
        /// </summary>
        /// <param name="leftValue">The value of the left child.</param>
        /// <param name="rightValue">The value of the right child.</param>
        /// <param name="operand">The operation between the two values.</param>
        /// <returns>The new float4</returns>
        public static Vector4 MixFloat4(Vector4 leftValue, Vector4 rightValue, MaterialBinaryOperand operand)
        {
            switch (operand)
            {
                case MaterialBinaryOperand.Add:
                    return Add(leftValue, rightValue);
                case MaterialBinaryOperand.Average:
                    return Average(leftValue, rightValue);
                case MaterialBinaryOperand.Color:
                    return Color(leftValue, rightValue);
                case MaterialBinaryOperand.ColorBurn:
                    return ColorBurn(leftValue, rightValue);
                case MaterialBinaryOperand.ColorDodge:
                    return ColorDodge(leftValue, rightValue);
                case MaterialBinaryOperand.Darken:
                    return Darken(leftValue, rightValue);
                case MaterialBinaryOperand.Desaturate:
                    return Desaturate(leftValue, rightValue);
                case MaterialBinaryOperand.Difference:
                    return Difference(leftValue, rightValue);
                case MaterialBinaryOperand.Divide:
                    return Divide(leftValue, rightValue);
                case MaterialBinaryOperand.Exclusion:
                    return Exclusion(leftValue, rightValue);
                case MaterialBinaryOperand.HardLight:
                    return HardLight(leftValue, rightValue);
                case MaterialBinaryOperand.HardMix:
                    return HardMix(leftValue, rightValue);
                case MaterialBinaryOperand.Hue:
                    return Hue(leftValue, rightValue);
                case MaterialBinaryOperand.Illuminate:
                    return Illuminate(leftValue, rightValue);
                case MaterialBinaryOperand.In:
                    return In(leftValue, rightValue);
                case MaterialBinaryOperand.Lighten:
                    return Lighten(leftValue, rightValue);
                case MaterialBinaryOperand.LinearBurn:
                    return LinearBurn(leftValue, rightValue);
                case MaterialBinaryOperand.LinearDodge:
                    return LinearDodge(leftValue, rightValue);
                case MaterialBinaryOperand.Mask:
                    return Mask(leftValue, rightValue);
                case MaterialBinaryOperand.Multiply:
                    return Multiply(leftValue, rightValue);
                case MaterialBinaryOperand.None:
                    return None(leftValue, rightValue);
                case MaterialBinaryOperand.Opaque:
                    return Opaque(leftValue, rightValue);
                case MaterialBinaryOperand.Out:
                    return Out(leftValue, rightValue);
                case MaterialBinaryOperand.Over:
                    return Over(leftValue, rightValue);
                case MaterialBinaryOperand.Overlay:
                    return Overlay(leftValue, rightValue);
                case MaterialBinaryOperand.PinLight:
                    return PinLight(leftValue, rightValue);
                case MaterialBinaryOperand.Saturate:
                    return Saturate(leftValue, rightValue);
                case MaterialBinaryOperand.Saturation:
                    return Saturation(leftValue, rightValue);
                case MaterialBinaryOperand.Screen:
                    return Screen(leftValue, rightValue);
                case MaterialBinaryOperand.SoftLight:
                    return SoftLight(leftValue, rightValue);
                case MaterialBinaryOperand.Subtract:
                    return Subtract(leftValue, rightValue);
                case MaterialBinaryOperand.SubstituteAlpha:
                    return SubstituteAlpha(leftValue, rightValue);
                default:
                    throw new ArgumentOutOfRangeException("operand");
            }
        }

        /// <summary>
        /// Perform the operation between two colors on CPU
        /// </summary>
        /// <param name="leftValue">The value of the left child.</param>
        /// <param name="rightValue">The value of the right child.</param>
        /// <param name="operand">The operation between the two values.</param>
        /// <returns>The new Color</returns>
        public static Color4 MixColor(Color4 leftValue, Color4 rightValue, MaterialBinaryOperand operand)
        {
            Vector4 res = MixFloat4(leftValue.ToVector4(), rightValue.ToVector4(), operand);
            return new Color4(res);
        }

        ///////////////////////////////////////////////////////
        //
        //          Blend functions utils
        //
        ///////////////////////////////////////////////////////

        private static Vector4 BasicColorBlend(Vector4 backColor, Vector4 frontColor, Vector4 interColor)
        {
            return frontColor.W * backColor.W * interColor + frontColor.W * (1.0f - backColor.W) * frontColor + (1.0f - frontColor.W) * backColor.W * backColor;
        }

        private static float BasicAlphaBlend(float ba, float fa)
        {
            return fa * (1.0f - ba) + ba;
        }

        private static Vector4 BasicBlend(Vector4 backColor, Vector4 frontColor, Vector4 interColor)
        {
            var temp = BasicColorBlend(backColor, frontColor, interColor);
            temp.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return temp;
        }

        private static float GetSaturation(Vector4 tex)
        {
            float max = Math.Max(Math.Max(tex.X, tex.Y), tex.Z);

            if (max < 1.0e-10)
                return 0.0f;

            return 1.0f - Math.Min(Math.Min(tex.X, tex.Y), tex.Z) / max;
        }

        private static float GetValue(Vector4 tex)
        {
            return Math.Max(Math.Max(tex.X, tex.Y), tex.Z);
        }

        private static float GetHue(Vector4 tex)
        {
            float max = Math.Max(Math.Max(tex.X, tex.Y), tex.Z);
            float delta = max - Math.Min(Math.Min(tex.X, tex.Y), tex.Z);

            if (delta < 1.0e-10)
                return 0.0f;
            if (max == tex.X)
                return (tex.Y - tex.Z) / (6.0f * delta);
            if (max == tex.Y)
                return 1.0f / 3.0f + (tex.Z - tex.X) / (6.0f * delta);

            return 2.0f / 3.0f + (tex.X - tex.Y) / (6.0f * delta);
        }

        private static Vector4 HSVToRGB(float hue, float saturation, float value)
        {
            var ti = (int)Math.Floor(1.0f - hue < 1.0e-10 ? 0.0f : hue * 6.0f);
            var f = hue * 6.0f - ti;
            var l = value * (1.0f - saturation);
            var m = value * (1.0f - f * saturation);
            var n = value * (1.0f - (1.0f - f) * saturation);

            switch (ti)
            {
                case 0:
                    return new Vector4(value, n, l, 0.0f);
                case 1:
                    return new Vector4(m, value, l, 0.0f);
                case 2:
                    return new Vector4(l, value, n, 0.0f);
                case 3:
                    return new Vector4(l, m, value, 0.0f);
                case 4:
                    return new Vector4(n, l, value, 0.0f);
                case 5:
                    return new Vector4(value, l, m, 0.0f);
                default:
                    throw new Exception("Unable to convert HSV to RGB");
            }
        }

        private static float ColorDivide(float t1, float t2)
        {
            if (t2 < 1.0e-10)
            {
                if (t1 < 1.0e-10)
                    return 0.0f;
                return 1.0f;
            }

            return t1 / t2;
        }

        private static Vector4 TermMultiply(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }

        private static Vector4 TermDivide(Vector4 v1, Vector4 v2)
        {
            return new Vector4(
                ColorDivide(v1.X, v2.X),
                ColorDivide(v1.Y, v2.Y),
                ColorDivide(v1.Z, v2.Z),
                ColorDivide(v1.W, v2.W)
                );
        }

        private static Vector4 Lerp(Vector4 v1, Vector4 v2, float coeff)
        {
            return v1 + coeff * (v2 - v1);
        }

        private static float Saturate(float a)
        {
            return Math.Max(0.0f, Math.Min(a, 1.0f));
        }

        ///////////////////////////////////////////////////////
        //
        //          Blend functions
        //
        ///////////////////////////////////////////////////////
        

        private static Vector4 Add(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var interColor = backColor + frontColor;
            return BasicBlend(backColor, frontColor, interColor);

            //// Maya version
            //interColor = backColor + frontColor * frontColor.W;
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 Average(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = 0.5f * (backColor + frontColor);
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 Color(Vector4 backColor, Vector4 frontColor)
        {
            Vector4 interColor;
            var frontSaturation = GetSaturation(frontColor);
            if (frontSaturation < 1.0e-10)
            {
                interColor = GetValue(backColor) * Vector4.One;
            }
            else
            {
                interColor = HSVToRGB(GetHue(frontColor), GetSaturation(frontColor), GetValue(backColor));
            }
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }

        private static Vector4 ColorBurn(Vector4 backColor, Vector4 frontColor)
        {
            return new Vector4(
                1.0f - ColorDivide(1.0f - backColor.X, frontColor.X),
                1.0f - ColorDivide(1.0f - backColor.Y, frontColor.Y),
                1.0f - ColorDivide(1.0f - backColor.Z, frontColor.Z),
                1.0f - ColorDivide(1.0f - backColor.W, frontColor.W)
                );
        }

        private static Vector4 ColorDodge(Vector4 backColor, Vector4 frontColor)
        {
            var frontColor3 = new Vector3(frontColor.X, frontColor.Y, frontColor.Z);
            var interColor = new Vector4();
            if (frontColor3 == Vector3.One)
            {
                interColor.X = backColor.X == 1.0f ? 1.0f : 0.0f;
                interColor.Y = backColor.Y == 1.0f ? 1.0f : 0.0f;
                interColor.Z = backColor.Z == 1.0f ? 1.0f : 0.0f;
            }
            else
            {
                interColor = TermDivide(backColor, Vector4.One - frontColor);
                interColor.X = Saturate(interColor.X);
                interColor.Y = Saturate(interColor.Y);
                interColor.Z = Saturate(interColor.Z);
            }
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 Darken(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var alphaBack = backColor.W * backColor;
            var alphaFront = frontColor.W * frontColor;
            var interColor0 = Lerp(alphaBack, frontColor, frontColor.W);
            var interColor1 = Lerp(alphaFront, backColor, backColor.W);
            return new Vector4(
                Math.Min(interColor0.X, interColor1.X),
                Math.Min(interColor0.Y, interColor1.Y),
                Math.Min(interColor0.Z, interColor1.Z),
                BasicAlphaBlend(backColor.W, frontColor.W)
                );

            //// Maya version
            //var min = new Vector4(
            //    Math.Min(frontColor.X, backColor.X),
            //    Math.Min(frontColor.Y, backColor.Y),
            //    Math.Min(frontColor.Z, backColor.Z),
            //    0.0f
            //    );
            //var interColor = Lerp(backColor, min, frontColor.W);
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 Desaturate(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = TermMultiply(backColor, Vector4.One - frontColor.W * frontColor);
            interColor.W = backColor.W;
            return interColor;
        }

        private static Vector4 Difference(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var interColor = frontColor - backColor;
            interColor.X = Math.Abs(interColor.X);
            interColor.Y = Math.Abs(interColor.Y);
            interColor.Z = Math.Abs(interColor.Z);

            return BasicBlend(backColor, frontColor, interColor);

            //// Maya version
            //var diff = new Vector4(
            //    Math.Abs(frontColor.X - backColor.X),
            //    Math.Abs(frontColor.Y - backColor.Y),
            //    Math.Abs(frontColor.Z - backColor.Z),
            //    0.0f
            //    );
            //interColor = Lerp(backColor, diff, frontColor.W);
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 Divide(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = new Vector4(
                ColorDivide(backColor.X, frontColor.X),
                ColorDivide(backColor.Y, frontColor.Y),
                ColorDivide(backColor.Z, frontColor.Z),
                0.0f
                );
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }

        private static Vector4 Exclusion(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = backColor + frontColor - 2.0f * TermMultiply(backColor, frontColor);
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 HardLight(Vector4 backColor, Vector4 frontColor)
        {
            var step = new Vector4(
                frontColor.X < 0.5f ? 0.0f : 1.0f,
                frontColor.Y < 0.5f ? 0.0f : 1.0f,
                frontColor.Z < 0.5f ? 0.0f : 1.0f,
                0.0f
                );
            var v1 = 2.0f * TermMultiply(frontColor, backColor);
            var v2 = Vector4.One - 2.0f * TermMultiply(Vector4.One - frontColor, Vector4.One - backColor);
            var interColor = v1 - TermMultiply(step, v2 - v1);
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 HardMix(Vector4 backColor, Vector4 frontColor)
        {
            var step = new Vector4(
                frontColor.X + backColor.X < 1.0f ? 0.0f : 1.0f,
                frontColor.Y + backColor.Y < 1.0f ? 0.0f : 1.0f,
                frontColor.Z + backColor.Z < 1.0f ? 0.0f : 1.0f,
                1.0f
                );
            return BasicBlend(backColor, frontColor, Vector4.One - step);
        }

        private static Vector4 Hue(Vector4 backColor, Vector4 frontColor)
        {
            Vector4 interColor;
            if (GetSaturation(frontColor) < 1.0e-10)
            {
                interColor = GetValue(backColor) * Vector4.One;
            }
            else
            {
                interColor = HSVToRGB(GetHue(frontColor), GetSaturation(backColor), GetValue(backColor));
            }
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }

        private static Vector4 Illuminate(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = TermMultiply(backColor, 2.0f * frontColor.W * frontColor + (1.0f - frontColor.W) * Vector4.One);
            interColor.W = backColor.W;
            return interColor;
        }

        private static Vector4 In(Vector4 backColor, Vector4 frontColor)
        {
            return backColor * frontColor.W;
        }

        private static Vector4 Lighten(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var v1 = Lerp(backColor * backColor.W, frontColor, frontColor.W);
            var v2 = Lerp(frontColor * frontColor.W, backColor, backColor.W);
            return new Vector4(
                Math.Max(v1.X, v2.X),
                Math.Max(v1.Y, v2.Y),
                Math.Max(v1.Z, v2.Z),
                BasicAlphaBlend(backColor.W, frontColor.W)
                );

            //// Maya version
            //var maxColor = new Vector4(
            //    Math.Max(backColor.X, frontColor.X),
            //    Math.Max(backColor.Y, frontColor.Y),
            //    Math.Max(backColor.Z, frontColor.Z),
            //    1.0f
            //    );
            //var interColor = Lerp(backColor, maxColor, frontColor.W);
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 LinearBurn(Vector4 backColor, Vector4 frontColor)
        {
            var sum = frontColor + backColor;
            var interColor = new Vector4(
                sum.X > 1.0f ? sum.X - 1.0f : 0.0f,
                sum.Y > 1.0f ? sum.Y - 1.0f : 0.0f,
                sum.Z > 1.0f ? sum.Z - 1.0f : 0.0f,
                0.0f
                );
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 LinearDodge(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = new Vector4(
                Saturate(backColor.X + frontColor.X),
                Saturate(backColor.Y + frontColor.Y),
                Saturate(backColor.Z + frontColor.Z),
                1.0f
                );
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 Mask(Vector4 backColor, Vector4 frontColor)
        {
            return new Vector4(backColor.X, backColor.Y, backColor.Z, backColor.W * (frontColor.X + frontColor.Y + frontColor.Z) / 3.0f);
        }

        private static Vector4 Multiply(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var interColor = TermMultiply(backColor, frontColor);
            return BasicBlend(backColor, frontColor, interColor);

            //// Maya version
            //interColor = TermMultiply(backColor, Lerp(Vector4.One, frontColor, frontColor.W));
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 None(Vector4 backColor, Vector4 frontColor)
        {
            return backColor;
        }

        private static Vector4 Opaque(Vector4 backColor, Vector4 frontColor)
        {
            return new Vector4(backColor.X, backColor.Y, backColor.Z, 1.0f);
        }

        private static Vector4 Out(Vector4 backColor, Vector4 frontColor)
        {
            return backColor * (1.0f - frontColor.W);
        }

        private static Vector4 Over(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS max version
            return BasicBlend(backColor, frontColor, frontColor);

            //// Maya version
            //var interColor = Lerp(backColor, frontColor, frontColor.W);
            //interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            //return interColor;
        }

        private static Vector4 Overlay(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Version
            var lerp0 = 2.0f * TermMultiply(backColor, frontColor);
            var lerp1 = Vector4.One - 2.0f * TermMultiply(Vector4.One - backColor, Vector4.One - frontColor);

            var stepX = frontColor.X < 0.5f ? 0.0f : 1.0f;
            var stepY = frontColor.Y < 0.5f ? 0.0f : 1.0f;
            var stepZ = frontColor.Z < 0.5f ? 0.0f : 1.0f;
            var stepW = frontColor.W < 0.5f ? 0.0f : 1.0f;

            return new Vector4(
                lerp0.X + stepX * (lerp1.X - lerp0.X),
                lerp0.Y + stepY * (lerp1.Y - lerp0.Y),
                lerp0.Z + stepZ * (lerp1.Z - lerp0.Z),
                lerp0.W + stepW * (lerp1.W - lerp0.W)
                );
        }

        private static Vector4 PinLight(Vector4 backColor, Vector4 frontColor)
        {
            var v1 = 2.0f * frontColor;
            var max = new Vector4(
                Math.Max(backColor.X, v1.X - 1.0f),
                Math.Max(backColor.Y, v1.Y - 1.0f),
                Math.Max(backColor.Z, v1.Z - 1.0f),
                0.0f
                );
            var min = new Vector4(
                Math.Min(backColor.X, v1.X),
                Math.Min(backColor.Y, v1.Y),
                Math.Min(backColor.Z, v1.Z),
                0.0f
                );
            var step = new Vector4(
                frontColor.X < 0.5f ? 1.0f : 0.0f,
                frontColor.Y < 0.5f ? 1.0f : 0.0f,
                frontColor.Z < 0.5f ? 1.0f : 0.0f,
                0.0f
                );
            var interColor = max + TermMultiply(step, min - max);
            return BasicBlend(backColor, frontColor, interColor);
        }

        private static Vector4 Saturate(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = TermMultiply(backColor, Vector4.One + (frontColor * frontColor.W));
            interColor.W = backColor.W;
            return interColor;
        }

        private static Vector4 Saturation(Vector4 backColor, Vector4 frontColor)
        {
            Vector4 interColor;
            if (GetSaturation(backColor) < 1.0e-10)
            {
                interColor = GetValue(backColor) * Vector4.One;
            }
            else
            {
                interColor = HSVToRGB(GetHue(backColor), GetSaturation(frontColor), GetValue(backColor));
            }
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }

        private static Vector4 Screen(Vector4 backColor, Vector4 frontColor)
        {
            var v1 = backColor.W * backColor;
            var interColor = TermMultiply(frontColor * frontColor.W, Vector4.One - v1) + v1;
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }

        private static float SoftLightTest(float bc, float fc)
        {
            if (fc < 0.5f)
                return bc * (1.0f + (1.0f - bc) * (2.0f * fc - 1.0f));
            if (bc < 9.0f / 64.0f)
                return bc * (bc * (9.0f - 18.0f * fc) + 5.76f * fc - 1.88f);
            return bc + ((float)Math.Sqrt(bc) - bc) * (2.0f * fc - 1.0f);
        }

        private static Vector4 SoftLight(Vector4 backColor, Vector4 frontColor)
        {
            return new Vector4(
                SoftLightTest(backColor.X, frontColor.X),
                SoftLightTest(backColor.Y, frontColor.Y),
                SoftLightTest(backColor.Z, frontColor.Z),
                BasicAlphaBlend(backColor.W, frontColor.W));
        }

        private static Vector4 Subtract(Vector4 backColor, Vector4 frontColor)
        {
            // 3DS Max version
            var interColor = backColor - frontColor;
            return BasicBlend(backColor, frontColor, interColor);

            //// Maya version
            //interColor = backColor - frontColor.W * frontColor;
            //interColor.W = backColor.W;
            //return interColor;
        }

        private static Vector4 SubstituteAlpha(Vector4 backColor, Vector4 frontColor)
        {
            var interColor = backColor;
            interColor.W = frontColor.W;
            return interColor;
        }

        private static Vector4 Value(Vector4 backColor, Vector4 frontColor)
        {
            Vector4 interColor;
            if (GetSaturation(backColor) < 1.0e-10)
                interColor = GetValue(backColor) * Vector4.One;
            else
            {
                interColor = HSVToRGB(GetHue(backColor), GetValue(backColor), GetValue(frontColor));
            }
            interColor.W = BasicAlphaBlend(backColor.W, frontColor.W);
            return interColor;
        }
    }
}
