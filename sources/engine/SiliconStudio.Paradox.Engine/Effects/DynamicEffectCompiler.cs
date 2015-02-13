// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Provides a dynamic compiler for an effect based on parameters changed.
    /// </summary>
    public class DynamicEffectCompiler
    {
        private readonly FastList<ParameterCollection> parameterCollections;

        private readonly string effectName;
        private bool asyncEffectCompiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEffectCompiler" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="asyncDynamicEffectCompiler">if set to <c>true</c> it can compile effect asynchronously.</param>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// effectName</exception>
        public DynamicEffectCompiler(IServiceRegistry services, string effectName)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (effectName == null) throw new ArgumentNullException("effectName");

            Services = services;
            this.effectName = effectName;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            parameterCollections = new FastList<ParameterCollection>();

            // Default behavior for fallback effect: load effect with same name but empty compiler parameters
            ComputeFallbackEffect = (dynamicEffectCompiler, name, parameters) =>
            {
                return dynamicEffectCompiler.EffectSystem.LoadEffect(effectName, new CompilerParameters()).WaitForResult();
            };
        }

        public bool AsyncEffectCompiler
        {
            get { return asyncEffectCompiler; }
            set { asyncEffectCompiler = value; }
        }

        public delegate Effect ComputeFallbackEffectDelegate(DynamicEffectCompiler dynamicEffectCompiler, string effectName, CompilerParameters compilerParameters);

        public ComputeFallbackEffectDelegate ComputeFallbackEffect { get; set; }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the name of the effect.
        /// </summary>
        /// <value>The name of the effect.</value>
        public string EffectName
        {
            get
            {
                return effectName;
            }
        }

        /// <summary>
        /// Gets or sets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        private GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Update a dynamic effect instance based on its parameters.
        /// </summary>
        /// <param name="effectInstance">A dynmaic effect instance</param>
        /// <param name="passParameters">The pass parameters.</param>
        /// <returns><c>true</c> if the effect was recomiled on the effect instance, <c>false</c> otherwise.</returns>
        public bool Update(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            bool effectChanged = false;

            var currentlyCompilingEffect = effectInstance.CurrentlyCompilingEffect;
            if (currentlyCompilingEffect != null)
            {
                if (currentlyCompilingEffect.IsCompleted)
                {
                    UpdateEffect(effectInstance, currentlyCompilingEffect.Result, effectInstance.CurrentlyCompilingUsedParameters);
                    effectChanged = true;

                    // Effect has been updated
                    effectInstance.CurrentlyCompilingEffect = null;
                    effectInstance.CurrentlyCompilingUsedParameters = null;
                }
            }
            else if (effectInstance.Effect == null || !EffectSystem.IsValid(effectInstance.Effect) || HasCollectionChanged(effectInstance, passParameters))
            {
                CreateEffect(effectInstance, passParameters);
                effectChanged = true;
            }

            return effectChanged;
        }

        private bool HasCollectionChanged(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            PrepareUpdater(effectInstance, passParameters);
            return effectInstance.Updater.HasChanged(effectInstance.UpdaterDefinition);
        }

        private void CreateEffect(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            var compilerParameters = new CompilerParameters();
            parameterCollections.Clear(true);
            if (passParameters != null)
            {
                parameterCollections.Add(passParameters);
            }
            effectInstance.FillParameterCollections(parameterCollections);

            foreach (var parameterCollection in parameterCollections)
            {
                if (parameterCollection != null)
                {
                    foreach (var parameter in parameterCollection.InternalValues)
                    {
                        compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
                    }
                }
            }

            foreach (var parameter in GraphicsDevice.Parameters.InternalValues)
            {
                compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
            }

            // Compile shader
            // possible exception in LoadEffect
            ShaderMixinParameters usedParameters;
            var effect = EffectSystem.LoadEffect(EffectName, compilerParameters, out usedParameters);

            // Do we have an async compilation?
            if (asyncEffectCompiler && effect.Task != null)
            {
                effectInstance.CurrentlyCompilingEffect = effect.Task;
                effectInstance.CurrentlyCompilingUsedParameters = usedParameters;
                // Fallback to default effect
                UpdateEffect(effectInstance, ComputeFallbackEffect(this, EffectName, compilerParameters), null);
                return;
            }

            var compiledEffect = effect.WaitForResult();

            UpdateEffect(effectInstance, compiledEffect, usedParameters);

            // Effect has been updated
            effectInstance.CurrentlyCompilingEffect = null;
            effectInstance.CurrentlyCompilingUsedParameters = null;
        }

        private void UpdateEffect(DynamicEffectInstance effectInstance, Effect compiledEffect, ParameterCollection usedParameters)
        {
            if (usedParameters == null)
                usedParameters = effectInstance.UpdaterDefinition.Parameters;

            if (!ReferenceEquals(compiledEffect, effectInstance.Effect))
            {
                effectInstance.Effect = compiledEffect;
                effectInstance.UpdaterDefinition = new EffectParameterUpdaterDefinition(compiledEffect, usedParameters);
            }
            else
            {
                // Same effect than previous one

                effectInstance.UpdaterDefinition.UpdateCounter(usedParameters);
            }

            UpdateLevels(effectInstance, null);
            effectInstance.Updater.UpdateCounters(effectInstance.UpdaterDefinition);
        }

        private void UpdateLevels(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            PrepareUpdater(effectInstance, passParameters);
            effectInstance.Updater.ComputeLevels(effectInstance.UpdaterDefinition);
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect instance.
        /// </summary>
        /// <param name="effectInstance">The effect instance.</param>
        /// <param name="passParameters">The pass parameters.</param>
        private void PrepareUpdater(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            parameterCollections.Clear(true);
            parameterCollections.Add(effectInstance.UpdaterDefinition.Parameters);
            if (passParameters != null)
            {
                parameterCollections.Add(passParameters);
            }
            effectInstance.FillParameterCollections(parameterCollections);
            parameterCollections.Add(GraphicsDevice.Parameters);

            effectInstance.Updater.Update(effectInstance.UpdaterDefinition, parameterCollections.Items, parameterCollections.Count);
        }
    }
}