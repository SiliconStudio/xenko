namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A Gamma <see cref="ColorTransform"/>.
    /// </summary>
    public class GammaTransform : ColorTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorTransform" /> class.
        /// </summary>
        /// <param name="colorTransformShader">Name of the shader.</param>
        public GammaTransform(string colorTransformShader = "GammaTransformShader") : base(colorTransformShader)
        {
        }

        /// <summary>
        /// Gets or sets the gamma value.
        /// </summary>
        /// <value>The value.</value>
        public float Value
        {
            get
            {
                return Parameters.Get(GammaTransformShaderKeys.Gamma);
            }
            set
            {
                Parameters.Set(GammaTransformShaderKeys.Gamma, value);
            }
        }
    }
}