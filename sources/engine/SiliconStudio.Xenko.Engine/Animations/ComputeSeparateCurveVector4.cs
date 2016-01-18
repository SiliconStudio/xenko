using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Animations
{
    [DataContract("ComputeSeparateCurveVector4")]
    [Display("4 Channels")]
    public class ComputeSeparateCurveVector4 : IComputeCurve<Vector4>
    {
        [DataMember(10)]
        [NotNull]
        [Display("X")]
        public IComputeCurve<float> X { get; set; } = new ComputeConstCurveFloat();

        [DataMember(20)]
        [NotNull]
        [Display("Y")]
        public IComputeCurve<float> Y { get; set; } = new ComputeConstCurveFloat();

        [DataMember(30)]
        [NotNull]
        [Display("Z")]
        public IComputeCurve<float> Z { get; set; } = new ComputeConstCurveFloat();

        [DataMember(40)]
        [NotNull]
        [Display("W")]
        public IComputeCurve<float> W { get; set; } = new ComputeConstCurveFloat();

        public Vector4 SampleAt(float t)
        {
            return new Vector4(X.SampleAt(t), Y.SampleAt(t), Z.SampleAt(t), W.SampleAt(t));            
        }
    }
}
