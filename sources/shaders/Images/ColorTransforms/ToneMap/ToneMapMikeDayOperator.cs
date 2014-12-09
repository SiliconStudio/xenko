using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// The tonemap operator by Mike Day, Insomniac Games.
    /// </summary>
    /// <remarks>
    /// https://d3cw3dd2w32x2b.cloudfront.net/wp-content/uploads/2012/09/an-efficient-and-user-friendly-tone-mapping-operator.pdf
    /// </remarks>
    public class ToneMapMikeDayOperator : ToneMapOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapMikeDayOperator"/> class.
        /// </summary>
        public ToneMapMikeDayOperator()
            : base("ToneMapMikeDayOperatorShader")
        {
            BlackPoint = 0.005f;
            CrossOver = 0.8f;
            WhitePoint = 4.0f;
            Toe = 0.2f;
            Shoulder = 0.8f;
        }

        /// <summary>
        /// Gets or sets the black point.
        /// </summary>
        /// <value>The black point.</value>
        public float BlackPoint { get; set; }

        /// <summary>
        /// Gets or sets the cross over.
        /// </summary>
        /// <value>The cross over.</value>
        public float CrossOver { get; set; }

        /// <summary>
        /// Gets or sets the white point.
        /// </summary>
        /// <value>The white point.</value>
        public float WhitePoint { get; set; }

        /// <summary>
        /// Gets or sets the toe.
        /// </summary>
        /// <value>The toe.</value>
        public float Toe { get; set; }

        /// <summary>
        /// Gets or sets the shoulder.
        /// </summary>
        /// <value>The shoulder.</value>
        public float Shoulder { get; set; }

        public override void UpdateParameters(ColorTransformContext context)
        {
            // TODO This could be put as part 

            double b = BlackPoint;
            double c = CrossOver;
            double w = WhitePoint;
            double t = Toe;
            double s = Shoulder;

            double k = ((1 - t) * (c - b)) / ((1 - s) * (w - c) + (1 - t) * (c - b));

            var toe = new Vector4(
                (float)((k * (1 - t))),
                (float)(-t),
                (float)(k * (1 - t) * (-b)),
                (float)(c - (1 - t) * b));

            var shoulder = new Vector4(
                (float)(((1 - k) + k * s)),
                (float)(s),
                (float)((1 - k) * (-c) + k * ((1 - s) * w - c)),
                (float)((1 - s) * w - c));

            // Don't call base, as we are rewriting all parameters for the shader
            Parameters.Set(ToneMapMikeDayOperatorShaderKeys.ToeCoeffs, toe);
            Parameters.Set(ToneMapMikeDayOperatorShaderKeys.ShoulderCoeffs, shoulder);
            Parameters.Set(ToneMapMikeDayOperatorShaderKeys.MiddleCrossOver, CrossOver);
        }
    }
}