
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{


    [DataContract("ComputeCurveSamplerVector4")]
    [Display("Vector4 sampler")]
    public class ComputeCurveSamplerVector4 : ComputeCurveSampler<Vector4> { }

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
    [Display("Binary Operation")]
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
        /// <inheritdoc/>
        public override void Cubic(ref float value1, ref float value2, ref float value3, ref float value4, float t, out float result)
        {
            result = Interpolator.Cubic(value1, value2, value3, value4, t);
        }

        /// <inheritdoc/>
        public override void Linear(ref float value1, ref float value2, float t, out float result)
        {
            result = Interpolator.Linear(value1, value2, t);
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
        /// <inheritdoc/>
        public override void Cubic(ref Vector4 value1, ref Vector4 value2, ref Vector4 value3, ref Vector4 value4, float t, out Vector4 result)
        {
            Interpolator.Vector4.Cubic(ref value1, ref value2, ref value3, ref value4, t, out result);
        }

        /// <inheritdoc/>
        public override void Linear(ref Vector4 value1, ref Vector4 value2, float t, out Vector4 result)
        {
            Interpolator.Vector4.Linear(ref value1, ref value2, t, out result);
        }
    }

    /// <summary>
    /// Binary operator vector4 value for the IComputeCurve interface
    /// </summary>
    [DataContract("ComputeBinaryCurveVector4")]
    [Display("Binary Operation")]
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
