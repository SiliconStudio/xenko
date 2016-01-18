
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Constant float value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeConstCurveFloat")]
    [Display("Constant")]
    public class ComputeConstCurveFloat : ComputeConstCurve<float> { }

    /// <summary>
    /// Binary operator float value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeBinaryCurveFloat")]
    [Display("Operation")]
    public class ComputeBinaryCurveFloat : ComputeBinaryCurve<float>
    {
        /// <inheritdoc/>
        protected override float Add(float a, float b)
        {
            return a + b;
        }

        /// <inheritdoc/>
        protected override float Subtract(float a, float b)
        {
            return a - b;
        }

        /// <inheritdoc/>
        protected override float Multiply(float a, float b)
        {
            return a*b;
        }
    }

    /// <summary>
    /// Animation of a float value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeAnimationCurveFloat")]
    [Display("Animation")]
    public class ComputeAnimationCurveFloat : ComputeAnimationCurve<float>
    {
        public override float GetInterpolatedValue(float value1, float weight1, float value2, float weight2)
        {
            return (value1 * weight1) + (value2 * weight2);
        }
    }

    /// <summary>
    /// Constant vector4 value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeConstCurveVector4")]
    [Display("Constant")]
    public class ComputeConstCurveVector4 : ComputeConstCurve<Vector4> { }

    /// <summary>
    /// Constant vector4 value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeAnimationCurveVector4")]
    [Display("Animation")]
    public class ComputeAnimationCurveVector4 : ComputeAnimationCurve<Vector4>
    {
        public override Vector4 GetInterpolatedValue(Vector4 value1, float weight1, Vector4 value2, float weight2)
        {
            return (value1 * weight1) + (value2 * weight2);
        }
    }

    /// <summary>
    /// Binary operator vector4 value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeBinaryCurveVector4")]
    [Display("Operation")]
    public class ComputeBinaryCurveVector4 : ComputeBinaryCurve<Vector4>
    {
        /// <inheritdoc/>
        protected override Vector4 Add(Vector4 a, Vector4 b)
        {
            return a + b;
        }

        /// <inheritdoc/>
        protected override Vector4 Subtract(Vector4 a, Vector4 b)
        {
            return a - b;
        }

        /// <inheritdoc/>
        protected override Vector4 Multiply(Vector4 a, Vector4 b)
        {
            return a * b;
        }
    }

}
