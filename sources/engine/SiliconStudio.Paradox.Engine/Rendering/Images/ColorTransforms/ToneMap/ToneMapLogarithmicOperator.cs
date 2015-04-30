using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// The tonemap logarithmic operator.
    /// </summary>
    [DataContract("ToneMapLogarithmicOperator")]
    [Display("Logarithmic")]
    public class ToneMapLogarithmicOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapLogarithmicOperator"/> class.
        /// </summary>
        public ToneMapLogarithmicOperator()
            : base("ToneMapLogarithmicOperatorShader")
        {
        }
    }
}