// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffectGroup : ImageEffect
    {
        private readonly HashSet<ParameterKey> requestedKeys = new HashSet<ParameterKey>();

        private readonly ImageEffectStepCollection effectSteps;

        private readonly List<bool> effectStepsEnabled;

        private readonly BrightFilter brightFilter;

        private readonly Bloom bloom;

        private readonly ColorTransformGroup colorTransformGroup;

        public ImageEffectGroup(IServiceRegistry services)
            : this(ImageEffectContext.GetShared(services))
        {
        }

        public ImageEffectGroup(ImageEffectContext context)
            : base(context)
        {
            effectSteps = new ImageEffectStepCollection();
            requestedKeys = new HashSet<ParameterKey>();

            effectStepsEnabled = new List<bool>();

            // TODO: Allow to customize the shader used by the various effects
            // Add luminance step
            var luminanceEffect = new LuminanceEffectStep(Context);
            Steps.Add(luminanceEffect);

            // Add Bright Filter step
            brightFilter = new BrightFilter(Context);
            var brightFilterStep = new BrightFilterStep(brightFilter);
            Steps.Add(brightFilterStep);

            // Add Bloom step
            bloom = new Bloom(Context);
            var bloomStep = new BloomStep(bloom);
            Steps.Add(bloomStep);
            
            // Add ColorTransformGroup last step (with ToneMap, ColorGrading, Gamma)
            colorTransformGroup = new ColorTransformGroup(Context);
            colorTransformGroup.Transforms.Add(new ToneMap());

            var colorTransformGroupStep = new ImageEffectStep(ImageEffectStepKeys.InputTexture, colorTransformGroup, isBuiltin: true);
            Steps.Add(colorTransformGroupStep);

            // Add AntiaAliasing step
        }

        public ImageEffectStepCollection Steps
        {
            get
            {
                return effectSteps;
            }
        }

        public BrightFilter BrightFilter
        {
            get
            {
                return brightFilter;
            }
        }

        public Bloom Bloom
        {
            get
            {
                return bloom;
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
            // Prepare required parameters
            foreach (var effectStep in effectSteps)
            {
                if (!effectStep.Enable)
                {
                    continue;
                }

                // Check required parameters setup on the effect step
                var requiredKeysCallback = effectStep as IImageEffectRequiredParameterKeys;
                if (requiredKeysCallback != null)
                {
                    requiredKeysCallback.FillRequired(requestedKeys);
                }

                // Check required parameters setup on the effect
                if (effectStep.Effect.Enable)
                {
                    requiredKeysCallback = effectStep.Effect as IImageEffectRequiredParameterKeys;
                    if (requiredKeysCallback != null)
                    {
                        requiredKeysCallback.FillRequired(requestedKeys);
                    }
                }
            }

            // TODO: We could reorder effects based on their Input/Output keys dependencies and required parameters

            // Check all enabled effects
            effectStepsEnabled.Clear();
            foreach (var effectStep in effectSteps)
            {
                bool effectEnabled = false;
                if (effectStep.Enable)
                {
                    effectEnabled = effectStep.Effect.Enable;
                    if (effectStep.CheckEnable != null)
                    {
                        effectEnabled = effectStep.CheckEnable(this, requestedKeys);
                    }
                }
                effectStepsEnabled.Add(effectEnabled);
            }

            for (int i = 0; i < effectSteps.Count; i++)
            {
                var effectStep = effectSteps[i];
                var effect = effectStep.Effect;

                var input = GetInput(effectStep.Input);
                var output = GetOutput(effectStep.Output);

                // TODO Handle when next steps are all disabled
                if (effectStepsEnabled[i])
                {
                    // Iterate on all effect steps
                    Context.Parameters.Set(ImageEffectStepKeys.InputTexture, input);
                    Context.Parameters.Set(ImageEffectStepKeys.OutputTexture, output);

                    effect.SetInput(input);
                    effect.SetOutput(output);
                    effect.Draw();
                }
                else if (effectStep.PassThrough && input != output && input != null && output != null)
                {
                    // If input != output and effect is disabled, copy at least the input to the output
                    Scaler.SetInput(input);
                    Scaler.SetOutput(output);
                    Scaler.Draw();
                }
            }
        }

        protected virtual Texture GetInput(ParameterKey<Texture> inputKey)
        {
            return Context.Parameters.Get(inputKey);
        }

        protected virtual Texture GetOutput(ParameterKey<Texture> outputKey)
        {
            return outputKey == ImageEffectStepKeys.NullTexture ? null : Context.Parameters.Get(outputKey);
        }
    }
}