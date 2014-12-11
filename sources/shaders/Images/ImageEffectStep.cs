// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffectStep
    {
        public ImageEffectStep(ImageEffect effect)
            : this(ImageEffects.Input, effect)
        {
        }

        public ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect)
            : this(input, effect, null, null, false)
        {
        }

        public ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output, ImageEffectStepEnableDelegate checkEnable = null)
            : this(input, effect, output ,checkEnable, false)
        {
        }

        internal ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output, ImageEffectStepEnableDelegate checkEnable, bool isBuiltin)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (effect == null) throw new ArgumentNullException("effect");
            Input = input;
            Effect = effect;
            Output = output ?? input;
            CheckEnable = checkEnable;
            IsBuiltin = isBuiltin;
        }

        public readonly ParameterKey<Texture> Input;

        public readonly ImageEffect Effect;

        public readonly ParameterKey<Texture> Output;

        public readonly ImageEffectStepEnableDelegate CheckEnable;

        internal readonly bool IsBuiltin;

        public override string ToString()
        {
            return string.Format("{0} => [{1}] => {2}", Input, Effect, Output);
        }
    }
}