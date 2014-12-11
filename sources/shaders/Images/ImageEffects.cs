// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffects : ImageEffect
    {
        public static readonly ParameterKey<Texture> Input = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> Output = ParameterKeys.New<Texture>();

        private readonly HashSet<ParameterKey> requestedKeys = new HashSet<ParameterKey>();

        private readonly ImageEffectStepCollection effectSteps;

        private readonly List<bool> effectStepsEnabled;

        private readonly ImageEffectStep colorTransformGroupStep;

        public ImageEffects(IServiceRegistry services)
            : this(ImageEffectContext.GetShared(services))
        {
        }

        public ImageEffects(ImageEffectContext context)
            : base(context)
        {
            effectSteps = new ImageEffectStepCollection();
            requestedKeys = new HashSet<ParameterKey>();

            effectStepsEnabled = new List<bool>();

            // TODO: Allow to customize the shader used by the various effects
            colorTransformGroupStep = new ImageEffectStep(Input, new ColorTransformGroup(Context), null, null, true);
            Steps.Add(colorTransformGroupStep);
        }

        public ImageEffectStepCollection Steps
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
                return (ColorTransformGroup)colorTransformGroupStep.Effect;
            }
        }

        protected override void DrawCore()
        {
            // Prepare required parameters
            foreach (var effectStep in effectSteps)
            {
                var parameterKeyDependencies = effectStep.Effect as IImageEffectParameterKeyDependencies;
                if (parameterKeyDependencies != null)
                {
                    parameterKeyDependencies.FillParameterKeyDependencies(requestedKeys);
                }
            }

            // Check all enabled effects
            effectStepsEnabled.Clear();
            foreach (var effectStep in effectSteps)
            {
                if (effectStep.CheckEnable != null)
                {
                    effectStep.Effect.Enable = effectStep.CheckEnable(this, requestedKeys);
                }
                effectStepsEnabled.Add(effectStep.Effect.Enable);
            }

            // Iterate on all effect steps
            Context.Parameters.Set(Input, GetInput(0));
            Context.Parameters.Set(Output, GetOutput(0));

            for (int i = 0; i < effectSteps.Count; i++)
            {
                var effectStep = effectSteps[i];
                var effect = effectStep.Effect;
                if (effectStep.CheckEnable != null)
                {
                    effect.Enable = effectStep.CheckEnable(this, requestedKeys);
                }

                var input = Context.Parameters.Get(effectStep.Input);
                var output = Context.Parameters.Get(effectStep.Output);

                // TODO Handle when next steps are all disabled

                if (effect.Enable)
                {
                    effect.SetInput(input);
                    effect.SetOutput(output);
                    effect.Draw();
                }
                else if (input != output)
                {
                    // If input != output and effect is disabled, copy at least the input to the output
                    Scaler.SetInput(input);
                    Scaler.SetOutput(output);
                    Scaler.Draw();
                }
            }
        }
    }
}