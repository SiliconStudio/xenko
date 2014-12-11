namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// The tonemap Reinhard operator.
    /// </summary>
    public class ToneMapDragoOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapDragoOperator"/> class.
        /// </summary>
        public ToneMapDragoOperator()
            : base("ToneMapDragoOperatorShader")
        {
        }

        /// <summary>
        /// Gets or sets the bias.
        /// </summary>
        /// <value>The bias.</value>
        public float Bias
        {
            get
            {
                return Parameters.Get(ToneMapDragoOperatorShaderKeys.DragoBias);
            }
            set
            {
                Parameters.Set(ToneMapDragoOperatorShaderKeys.DragoBias, value);
            }
        }
    }
}