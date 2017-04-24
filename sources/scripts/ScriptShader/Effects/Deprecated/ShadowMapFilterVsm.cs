// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class ShadowMapFilterVsm : ShadowMapFilter
    {
        /// <summary>
        /// Scale minimum pmax factor
        /// </summary>
        internal static readonly ParameterKey<float> VsmBleedingFactor = ParameterKeys.Value(0.0f);

        /// <summary>
        /// Scale minimum pmax factor
        /// </summary>
        internal static readonly ParameterKey<float> VsmMinVariance = ParameterKeys.Value(0.0f);

        public ShadowMapFilterVsm(ShadowMap shadowMap)
            : base(shadowMap)
        {
            MinVariance = 0.0000001f;
            BleedingFactor = 0.38f;
        }
        
        /// <summary>
        /// Gets or sets the bleeding factor.
        /// </summary>
        /// <value>
        /// The bleeding factor.
        /// </value>
        /// <remarks>
        /// Value between [0.0, 1.0[
        /// </remarks>
        public float BleedingFactor
        {
            get
            {
                return ShadowMap.Parameters.Get(VsmBleedingFactor);
            }
            set
            {
                ShadowMap.Parameters.Set(VsmBleedingFactor, value);
            }
        }

        /// <summary>
        /// Gets or sets the min variance.
        /// </summary>
        /// <value>
        /// The min variance.
        /// </value>
        public float MinVariance
        {
            get
            {
                return ShadowMap.Parameters.Get(VsmMinVariance);
            }
            set
            {
                ShadowMap.Parameters.Set(VsmMinVariance, value);
            }
        }

        public override ShaderClassSource GenerateShaderSource(int shadowMapCount)
        {
            return new ShaderClassSource("ShadowMapFilterVsm", shadowMapCount);
        }
    }
}
