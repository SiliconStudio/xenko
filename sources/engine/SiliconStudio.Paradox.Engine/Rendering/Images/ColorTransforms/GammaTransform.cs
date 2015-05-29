using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A Gamma <see cref="ColorTransformBase"/>.
    /// </summary>
    [DataContract("GammaTransform")]
    public class GammaTransform : ColorTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GammaTransform"/> class.
        /// </summary>
        public GammaTransform() : this("GammaTransformShader")
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GammaTransform" /> class.
        /// </summary>
        /// <param name="colorTransformShader">Name of the shader.</param>
        public GammaTransform(string colorTransformShader) : base(colorTransformShader)
        {
        }

        /// <summary>
        /// Gets or sets the gamma value.
        /// </summary>
        /// <value>The value.</value>
        [DataMember(10)]
        [DefaultValue(2.2333333f)]
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