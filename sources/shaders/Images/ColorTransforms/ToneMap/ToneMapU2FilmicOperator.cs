// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// The U2Filmic operator.
    /// </summary>
    /// <remarks>
    /// http://filmicgames.com/archives/75
    /// </remarks>
    public class ToneMapU2FilmicOperator : ToneMapOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapU2FilmicOperator"/> class.
        /// </summary>
        public ToneMapU2FilmicOperator()
            : base("ToneMapU2FilmicOperatorShader")
        {
        }

        /// <summary>
        /// Gets or sets the shoulder strength.
        /// </summary>
        /// <value>The shoulder strength.</value>
        public float ShoulderStrength
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.ShoulderStrength);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.ShoulderStrength, value);
            }
        }

        /// <summary>
        /// Gets or sets the linear strength.
        /// </summary>
        /// <value>The linear strength.</value>
        public float LinearStrength
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.LinearStrength);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.LinearStrength, value);
            }
        }

        /// <summary>
        /// Gets or sets the linear angle.
        /// </summary>
        /// <value>The linear angle.</value>
        public float LinearAngle
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.LinearAngle);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.LinearAngle, value);
            }
        }

        /// <summary>
        /// Gets or sets the toe strength.
        /// </summary>
        /// <value>The toe strength.</value>
        public float ToeStrength
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.ToeStrength);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.ToeStrength, value);
            }
        }

        /// <summary>
        /// Gets or sets the toe numerator.
        /// </summary>
        /// <value>The toe numerator.</value>
        public float ToeNumerator
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.ToeNumerator);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.ToeNumerator, value);
            }
        }

        /// <summary>
        /// Gets or sets the toe denominator.
        /// </summary>
        /// <value>The toe denominator.</value>
        public float ToeDenominator
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.ToeDenominator);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.ToeDenominator, value);
            }
        }

        /// <summary>
        /// Gets or sets the linear white.
        /// </summary>
        /// <value>The linear white.</value>
        public float LinearWhite
        {
            get
            {
                return Parameters.Get(ToneMapU2FilmicOperatorShaderKeys.LinearWhite);
            }
            set
            {
                Parameters.Set(ToneMapU2FilmicOperatorShaderKeys.LinearWhite, value);
            }
        }
    }
}