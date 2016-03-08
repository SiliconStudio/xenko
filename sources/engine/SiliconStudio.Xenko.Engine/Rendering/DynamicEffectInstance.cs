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
        //private KeyValuePair<ParameterKey, object>[] effectParameterKeys;

        private string effectName;
        private EffectSystem effectSystem;

        public DynamicEffectInstance(string effectName, ParameterCollection parameters = null) : base(null, parameters)
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

        protected override void ChooseEffect(GraphicsDevice graphicsDevice)
        {
            // TODO: Free previous descriptor sets and layouts?

            // Looks like the effect changed, it needs a recompilation
            var compilerParameters = new CompilerParameters();
            foreach (var effectParameterKey in Parameters.ParameterKeyInfos)
            {
                if (effectParameterKey.Key.Type == ParameterKeyType.Permutation)
                {
                    // TODO GRAPHICS REFACTOR avoid direct access, esp. since permutation values might be separated from Objects at some point
                    compilerParameters.SetObject(effectParameterKey.Key, Parameters.ObjectValues[effectParameterKey.BindingSlot]);
                }
            }

            effect = effectSystem.LoadEffect(effectName, compilerParameters).WaitForResult();
        }
    }
}