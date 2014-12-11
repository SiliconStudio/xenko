// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    internal class BrightFilterStep : ImageEffectStep
    {
        public BrightFilterStep(ImageEffectContext context)
            : this(new BrightFilter(context))
        {
        }
        public BrightFilterStep(BrightFilter brightFilter)
            : base(ImageEffectStepKeys.InputTexture, brightFilter, ImageEffectStepKeys.NullTexture, CheckEnabled, true)
        {
        }

        private static bool CheckEnabled(ImageEffectGroup imageEffectGroup, HashSet<ParameterKey> requiredKeys)
        {
            return requiredKeys.Contains(BrightFilter.TextureResult);
        }
    }
}