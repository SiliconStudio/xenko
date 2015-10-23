// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Graphics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used for texturing.
    /// </summary>
    public static class SSAOKeys
    {
        /// <summary>
        /// Source texture.
        /// </summary>
        public static readonly ParameterResourceKey<Texture2D> Texture = ParameterKeys.Resource<Texture2D>();

        /// <summary>
        /// Intensity of the ambient occlusion.
        /// </summary>
        public static readonly ParameterValueKey<float> Intensity = ParameterKeys.Value(4.0f);

        /// <summary>
        /// Size of the sampling radius where random samples will be taken.
        /// </summary>
        public static readonly ParameterValueKey<float> SamplingRadius = ParameterKeys.Value(5.0f);

        /// <summary>
        /// Scale distance between occluder and occludee.
        /// </summary>
        public static readonly ParameterValueKey<float> Scale = ParameterKeys.Value(0.1f);

        /// <summary>
        /// Width of the occlusion cone for occludee
        /// </summary>
        public static readonly ParameterValueKey<float> Bias = ParameterKeys.Value(0.05f);

        /// <summary>
        /// Self occlusion factors. TODO comment this field.
        /// </summary>
        public static readonly ParameterValueKey<float> SelfOcclusion = ParameterKeys.Value(0.3f);
    }
}