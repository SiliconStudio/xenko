// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ColorTransformGroup : ImageEffectBase
    {
        private readonly ParameterCollection transformsParameters;

        private readonly ImageEffect transformGroupEffect;

        private readonly Dictionary<ParameterCompositeKey, ParameterKey> compositeKeys;

        private readonly List<ColorTransform> transforms;

        private readonly List<ColorTransform> collectTransforms;

        private readonly List<ColorTransform> enabledTransforms;

        private readonly GammaTransform gammaTransform;

        private readonly ColorTransformContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorTransformGroup"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="colorTransformGroupEffect">The color transform group effect.</param>
        public ColorTransformGroup(ImageEffectContext context, string colorTransformGroupEffect = "ColorTransformGroupEffect")
            : base(context, colorTransformGroupEffect)
        {
            compositeKeys = new Dictionary<ParameterCompositeKey, ParameterKey>();
            transforms = new List<ColorTransform>();
            enabledTransforms = new List<ColorTransform>();
            collectTransforms = new List<ColorTransform>();

            transformsParameters = new ParameterCollection();

            gammaTransform = new GammaTransform();

            transformGroupEffect = new ImageEffect(context, colorTransformGroupEffect, Parameters);
            Parameters.Set(ColorTransformGroupKeys.Transforms, enabledTransforms);

            // we are adding parameter collections after as transform parameters should override previous parameters
            transformGroupEffect.ParameterCollections.Add(transformsParameters);

            this.context = new ColorTransformContext(this);
        }

        /// <summary>
        /// Gets the color transforms.
        /// </summary>
        /// <value>The transforms.</value>
        public List<ColorTransform> Transforms
        {
            get
            {
                return transforms;
            }
        }

        /// <summary>
        /// Gets the gamma transform that is applied after all <see cref="Transforms"/>
        /// </summary>
        /// <value>The gamma transform.</value>
        public GammaTransform GammaTransform
        {
            get
            {
                return gammaTransform;
            }
        }

        protected override void DrawCore()
        {
            var output = GetOutput(0);
            if (output == null)
            {
                return;
            }

            // Collect all transform parameters
            CollectTransformsParameters();

            for (int i = 0; i < context.Inputs.Count; i++)
            {
                transformGroupEffect.SetInput(i, context.Inputs[i]);
            }
            transformGroupEffect.SetOutput(output);
            transformGroupEffect.Draw(Name);
        }

        protected virtual void AddPreTransforms(List<ColorTransform> tempTransforms)
        {
        }


        protected virtual void AddPostTransforms(List<ColorTransform> tempTransforms)
        {
            tempTransforms.Add(gammaTransform);
        }

        private void CollectTransformsParameters()
        {
            context.Inputs.Clear();
            for (int i = 0; i < InputCount; i++)
            {
                context.Inputs.Add(GetInput(i));
            }

            // Grab all color transforms
            collectTransforms.Clear();
            AddPreTransforms(collectTransforms);
            collectTransforms.AddRange(transforms);
            AddPostTransforms(collectTransforms);
            enabledTransforms.Clear();

            // Copy all parameters from ColorTransform to effect parameters
            foreach (var transform in collectTransforms)
            {
                // Skip unused transform
                if (transform == null || !transform.Enabled || transform.Shader == null)
                {
                    continue;
                }

                enabledTransforms.Add(transform);
            }

            transformsParameters.Clear();
            for (int i = 0; i < enabledTransforms.Count; i++)
            {
                var transform = enabledTransforms[i];
                context.TransformParameters.Clear();
                transform.UpdateParameters(context);

                // Copy transform parameters back to the composition with the current index
                foreach (var parameterValue in context.TransformParameters)
                {
                    var key = GetComposedKey(parameterValue.Key, i);
                    var value = parameterValue.Value;

                    // TODO: values are boxed/unboxed here generating GC
                    transformsParameters.SetObject(key, value);
                }
            }
        }

        private ParameterKey GetComposedKey(ParameterKey key, int transformIndex)
        {
            var compositeKey = new ParameterCompositeKey(key, transformIndex);

            ParameterKey rawCompositeKey;
            if (!compositeKeys.TryGetValue(compositeKey, out rawCompositeKey))
            {
                rawCompositeKey = ParameterKeys.FindByName(string.Format("{0}.Transforms[{1}]", key.Name, transformIndex));
                compositeKeys.Add(compositeKey, rawCompositeKey);
            }
            return rawCompositeKey;
        }

        /// <summary>
        /// An internal key to cache {Key,TransformIndex} => CompositeKey
        /// </summary>
        private struct ParameterCompositeKey : IEquatable<ParameterCompositeKey>
        {
            private readonly ParameterKey key;

            private readonly int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterCompositeKey"/> struct.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="transformIndex">Index of the transform.</param>
            public ParameterCompositeKey(ParameterKey key, int transformIndex)
            {
                if (key == null) throw new ArgumentNullException("key");
                this.key = key;
                index = transformIndex;
            }

            public bool Equals(ParameterCompositeKey other)
            {
                return key.Equals(other.key) && index == other.index;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ParameterCompositeKey && Equals((ParameterCompositeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (key.GetHashCode() * 397) ^ index;
                }
            }
        }
    }
}