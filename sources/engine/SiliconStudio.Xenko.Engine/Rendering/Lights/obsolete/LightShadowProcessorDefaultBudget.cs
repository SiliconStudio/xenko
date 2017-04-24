// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Effects.Lights;
using SiliconStudio.Xenko.Effects.Shadows;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Effects.Processors
{
    /// <summary>
    /// This class inherits from <see cref="LightShadowProcessorWithBudget"/> and has a budget of two 2048 x 2048 shadow map textures: one for the variance shadow maps, one for the other types.
    /// </summary>
    // TODO: rewrite this class
    public class LightShadowProcessorDefaultBudget : LightShadowProcessorWithBudget
    {
        public LightShadowProcessorDefaultBudget(GraphicsDevice device, bool manageShadows)
            : base(device, manageShadows)
        {
            // Fixed budget of textures
            AddShadowMapTexture(new ShadowMapTexture(GraphicsDevice, LightShadowMapFilterType.Nearest, 2048), LightShadowMapFilterType.Nearest);
            AddShadowMapTexture(new ShadowMapTexture(GraphicsDevice, LightShadowMapFilterType.Variance, 2048), LightShadowMapFilterType.Variance);
        }
    }
}
