// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffectStep
    {
        public ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output)
        {
            Input = input;
            Effect = effect;
            Output = output;
        }

        public readonly ParameterKey<Texture> Input;

        public readonly ImageEffectBase Effect;

        public readonly ParameterKey<Texture> Output;

        public override string ToString()
        {
            return string.Format("{0} => [{1}] => {2}", Input, Effect, Output);
        }
    }

    public class ImageEffects : ImageEffectBase
    {
        public static readonly ParameterKey<Texture> Input = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> Output = ParameterKeys.New<Texture>();

        private readonly List<ImageEffectStep> effectSteps;

        private readonly ColorTransformGroup colorTransformGroup;

        public ImageEffects(IServiceRegistry services)
            : this(ImageEffectContext.GetSharedContext(services))
        {
        }

        public ImageEffects(ImageEffectContext context)
            : base(context)
        {
            effectSteps = new List<ImageEffectStep>();

            colorTransformGroup = CreateDefaultColorTransformGroup();
        }

        public List<ImageEffectStep> Effects
        {
            get
            {
                return effectSteps;
            }
        }

        public ColorTransformGroup ColorTransform
        {
            get
            {
                return colorTransformGroup;
            }
        }

        protected virtual ColorTransformGroup CreateDefaultColorTransformGroup()
        {
            // TODO: Add the following color transforms:
            // TODO: Add tonemapping
            // TODO: Add Color grading
            // TODO: Add noise
            var defaultTransformGroup = new ColorTransformGroup(Context);
            return defaultTransformGroup;
        }
    }
}