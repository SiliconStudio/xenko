// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules
{
    /// <summary>
    /// Keys used for texturing.
    /// </summary>
    public static class SSAOKeys
    {
        /// <summary>
        /// Source texture.
        /// </summary>
        public static readonly ParameterKey<Texture> Texture = ParameterKeys.New<Texture>();

        /// <summary>
        /// Intensity of the ambient occlusion.
        /// </summary>
        public static readonly ParameterKey<float> Intensity = ParameterKeys.New(4.0f);

        /// <summary>
        /// Size of the sampling radius where random samples will be taken.
        /// </summary>
        public static readonly ParameterKey<float> SamplingRadius = ParameterKeys.New(5.0f);

        /// <summary>
        /// Scale distance between occluder and occludee.
        /// </summary>
        public static readonly ParameterKey<float> Scale = ParameterKeys.New(0.1f);

        /// <summary>
        /// Width of the occlusion cone for occludee
        /// </summary>
        public static readonly ParameterKey<float> Bias = ParameterKeys.New(0.05f);

        /// <summary>
        /// Self occlusion factors. TODO comment this field.
        /// </summary>
        public static readonly ParameterKey<float> SelfOcclusion = ParameterKeys.New(0.3f);
    }
}