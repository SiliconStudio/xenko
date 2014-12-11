// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffectStep
    {
        public ImageEffectStep(ImageEffect effect)
            : this(ImageEffectStepKeys.InputTexture, effect)
        {
        }

        public ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output = null, ImageEffectStepEnableDelegate checkEnable = null, bool isBuiltin = false)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (effect == null) throw new ArgumentNullException("effect");
            Input = input;
            Effect = effect;
            Output = output ?? input;
            CheckEnable = checkEnable;
            IsBuiltin = isBuiltin;
            PassThrough = true;
        }

        public bool Enable { get; set; }

        public readonly ParameterKey<Texture> Input;

        public readonly ImageEffect Effect;

        public readonly ParameterKey<Texture> Output;

        public readonly ImageEffectStepEnableDelegate CheckEnable;

        public bool PassThrough;

        internal readonly bool IsBuiltin;

        public override string ToString()
        {
            return String.Format("{0} => [{1}] => {2}", Input, Effect, Output);
        }
    }
}