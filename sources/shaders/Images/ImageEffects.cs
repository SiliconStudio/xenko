// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public delegate bool ImageEffectStepEnableDelegate(ImageEffects imageEffects, HashSet<ParameterKey> requiredKeys);

    public class ImageEffectStep
    {
        public ImageEffectStep(ParameterKey<Texture> input, ImageEffect effect, ParameterKey<Texture> output, ImageEffectStepEnableDelegate checkEnable)
        {
            Input = input;
            Effect = effect;
            Output = output;
            CheckEnable = checkEnable;
        }

        public readonly ParameterKey<Texture> Input;

        public readonly ImageEffect Effect;

        public readonly ParameterKey<Texture> Output;

        public readonly ImageEffectStepEnableDelegate CheckEnable;

        public override string ToString()
        {
            return string.Format("{0} => [{1}] => {2}", Input, Effect, Output);
        }
    }

    public class ImageEffects : ImageEffect
    {
        public static readonly ParameterKey<Texture> Input = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> Output = ParameterKeys.New<Texture>();

        private readonly HashSet<ParameterKey> requestedKeys = new HashSet<ParameterKey>();

        private readonly List<ImageEffectStep> effectSteps;

        private readonly ColorTransformGroup colorTransformGroup;

        public ImageEffects(IServiceRegistry services)
            : this(ImageEffectContext.GetShared(services))
        {
        }

        public ImageEffects(ImageEffectContext context)
            : base(context)
        {
            effectSteps = new List<ImageEffectStep>();
            requestedKeys = new HashSet<ParameterKey>();

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

        protected override void DrawCore()
        {
            // Prepare requested parameters
            foreach (var effectStep in effectSteps)
            {
                var parameterKeyDependencies = effectStep.Effect as IImageEffectParameterKeyDependencies;
                if (parameterKeyDependencies != null)
                {
                    parameterKeyDependencies.FillParameterKeyDependencies(requestedKeys);
                }
            }

            Context.Parameters.Set(ImageEffects.Input, GetInput(0));
            Context.Parameters.Set(ImageEffects.Output, GetOutput(0));

            foreach (var effectStep in effectSteps)
            {
                var effect = effectStep.Effect;
                if (effectStep.CheckEnable != null)
                {
                    effect.Enable = effectStep.CheckEnable(this, requestedKeys);
                }
                if (effect.Enable)
                {
                    var input = Context.Parameters.Get(effectStep.Input);
                    var output = Context.Parameters.Get(effectStep.Output ?? effectStep.Input);

                    effect.SetInput(input);
                    effect.SetOutput(output.ToRenderTarget());



                }



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