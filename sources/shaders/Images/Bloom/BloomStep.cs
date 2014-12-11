// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    internal class BloomStep : ImageEffectStep, IImageEffectRequiredParameterKeys
    {
        public BloomStep(ImageEffectContext context)
            : this(new Bloom(context))
        {
        }
        public BloomStep(Bloom bloom)
            : base(BrightFilter.TextureResult, bloom, ImageEffectStepKeys.NullTexture, null, true)
        {
        }

        public void FillRequired(HashSet<ParameterKey> requiredKeys)
        {
            requiredKeys.Add(Input);
        }
    }
}