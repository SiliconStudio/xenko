using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.Rendering.Images
{
    [DataContract("FilmGrain")]
    public sealed class FilmGrain : ColorTransform
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FilmGrain"/> class.
        /// </summary>
        public FilmGrain() : this("FilmGrainShader")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilmGrain"/> class.
        /// </summary>
        public FilmGrain(string effect) : base(effect)
        {
            Amount = 0.25f;
            GrainSize = 1.6f;
            Animate = false;
            LuminanceFactor = 1f;
        }

        /// <summary>
        /// Amount of grain.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(0.25f)]
        [DataMemberRange(0f, 1f, 0.01f, 0.1f)]
        public float Amount { get; set; }


        /// <summary>
        /// Grain size.
        /// </summary>
        [DataMember(20)]
        [DefaultValue(1.6f)]
        [DataMemberRange(0f, 10f, 0.01f, 0.1f)]
        public float GrainSize { get; set; }

        /// <summary>
        /// Animates the film grain.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool Animate { get; set; }

        /// <summary>
        /// How the luminance influences the amount of grain.
        /// </summary>
        [DataMember(40)]
        [DefaultValue(1.0)]
        [DataMemberRange(0f, 1f, 0.01f, 0.1f)]
        public float LuminanceFactor { get; set; }

        private float time = 1f;

        public override void UpdateParameters(ColorTransformContext context)
        {
            base.UpdateParameters(context);

            Parameters.Set(FilmGrainShaderKeys.Amount, Amount * 4 * 0.02f);
            Parameters.Set(FilmGrainShaderKeys.GrainSize, GrainSize);
            Parameters.Set(FilmGrainShaderKeys.LuminanceFactor, LuminanceFactor);
            if (Animate)
            {
                time += 0.1f;
                if (time > 100f) time = 1f;
            }
            Parameters.Set(FilmGrainShaderKeys.Time, time);
        }

    }
}
