// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public static class ImageEffectStepCollectionExtensions
    {
        public static void Add(this ImageEffectStepCollection stepCollection, ImageEffect effect)
        {
            stepCollection.Add(ImageEffects.Input, effect);
        }

        public static void Add(this ImageEffectStepCollection stepCollection, ParameterKey<Texture> input, ImageEffect effect)
        {
            stepCollection.Add(input, effect, input);
        }

        public static void Add(this ImageEffectStepCollection stepCollection, ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output)
        {
            stepCollection.Add(new ImageEffectStep(input, effect, output));
        }
    }
}