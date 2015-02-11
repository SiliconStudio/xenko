using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// The tonemap exponential operator.
    /// </summary>
    [Display("Exponential")]
    public class ToneMapExponentialOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapExponentialOperator"/> class.
        /// </summary>
        public ToneMapExponentialOperator()
            : base("ToneMapExponentialOperatorShader")
        {
        }
    }
}