// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    internal class LuminanceEffectStep : ImageEffectStep
    {
        public LuminanceEffectStep(ImageEffectContext context)
            : this(new LuminanceEffect(context))
        {
        }
        public LuminanceEffectStep(LuminanceEffect luminanceEffect)
            : base(ImageEffectStepKeys.InputTexture, luminanceEffect, ImageEffectStepKeys.NullTexture, CheckEnabled, true)
        {
        }

        private static bool CheckEnabled(ImageEffectGroup imageEffectGroup, HashSet<ParameterKey> requiredKeys)
        {
            return requiredKeys.Contains(LuminanceEffect.LuminanceResult);
        }
    }
}