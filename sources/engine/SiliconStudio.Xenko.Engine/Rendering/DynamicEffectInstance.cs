// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    public class DynamicEffectInstance : EffectInstance
    {
        // Parameter keys used for effect permutation
        private KeyValuePair<ParameterKey, object>[] effectParameterKeys;

        private string effectName;
        private EffectSystem effectSystem;

        public DynamicEffectInstance(string effectName, NextGenParameterCollection parameters = null) : base(null, parameters)
        {
            this.effectName = effectName;
        }

        public string EffectName
        {
            get { return effectName; }
            set { effectName = value; }
        }

        public void Initialize(IServiceRegistry services)
        {
            this.effectSystem = services.GetSafeServiceAs<EffectSystem>();
        }

        /// <summary>
        /// Sets a value that will impact effect permutation (used in .xkfx file).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterKey"></param>
        /// <param name="value"></param>
        public void SetPermutationValue<T>(ParameterKey<T> parameterKey, T value)
        {
            // Look for existing entries
            if (effectParameterKeys != null)
            {
                for (int i = 0; i < effectParameterKeys.Length; ++i)
                {
                    if (effectParameterKeys[i].Key == parameterKey)
                    {
                        if (effectParameterKeys[i].Value != (object)value)
                        {
                            effectParameterKeys[i] = new KeyValuePair<ParameterKey, object>(parameterKey, value);
                            effectDirty = true;
                        }
                        return;
                    }
                }
            }

            // It's a new key, let's add it
            Array.Resize(ref effectParameterKeys, (effectParameterKeys?.Length ?? 0) + 1);
            effectParameterKeys[effectParameterKeys.Length - 1] = new KeyValuePair<ParameterKey, object>(parameterKey, value);
            effectDirty = true;
        }

        protected override void ChooseEffect(GraphicsDevice graphicsDevice)
        {
            // TODO: Free previous descriptor sets and layouts?

            // Looks like the effect changed, it needs a recompilation
            var compilerParameters = new CompilerParameters();
            if (effectParameterKeys != null)
            {
                foreach (var effectParameterKey in effectParameterKeys)
                {
                    compilerParameters.SetObject(effectParameterKey.Key, effectParameterKey.Value);
                }
            }

            effect = effectSystem.LoadEffect(effectName, compilerParameters).WaitForResult();
        }
    }
}