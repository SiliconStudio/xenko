// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Provides a dynamic compiler for an effect based on parameters changed.
    /// </summary>
    public class DynamicEffectCompiler
    {
        private readonly EffectParameterUpdater updater;
        private readonly FastList<ParameterCollection> parameterCollections;

        private readonly string effectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEffectCompiler"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <exception cref="System.ArgumentNullException">
        /// services
        /// or
        /// effectName
        /// </exception>
        public DynamicEffectCompiler(IServiceRegistry services, string effectName)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (effectName == null) throw new ArgumentNullException("effectName");
            Services = services;
            this.effectName = effectName;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            updater = new EffectParameterUpdater();
            parameterCollections = new FastList<ParameterCollection>();
        }

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
        private EffectSystem EffectSystem { get; set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        private GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Update a dynamic effect instance based on its parameters.
        /// </summary>
        /// <param name="effectInstance">A dynmaic effect instance</param>
        /// <returns><c>true</c> if the effect was recomiled on the effect instance, <c>false</c> otherwise.</returns>
        public bool Update(DynamicEffectInstance effectInstance)
        {
            bool effectChanged = false;

            if (effectInstance.Effect == null || !EffectSystem.IsValid(effectInstance.Effect) || HasCollectionChanged(effectInstance))
            {
                CreateEffect(effectInstance);
                effectChanged = true;
            }

            return effectChanged;
        }

        private bool HasCollectionChanged(DynamicEffectInstance effectInstance)
        {
            PrepareUpdater(effectInstance);
            return updater.HasChanged(effectInstance.UpdaterDefinition);
        }

        private void CreateEffect(DynamicEffectInstance effectInstance)
        {
            var compilerParameters = new CompilerParameters();
            parameterCollections.Clear(true);
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
            var effect = EffectSystem.LoadEffect(EffectName, compilerParameters);

            if (!ReferenceEquals(effect, effectInstance.Effect))
            {
                effectInstance.Effect = effect;
                effectInstance.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect);
            }
            else
            {
                // Same effect than previous one

                effectInstance.UpdaterDefinition.UpdateCounter(effect.CompilationParameters);
            }

            UpdateLevels(effectInstance);
            updater.UpdateCounters(effectInstance.UpdaterDefinition);
        }

        private void UpdateLevels(DynamicEffectInstance effectInstance)
        {
            PrepareUpdater(effectInstance);
            updater.ComputeLevels(effectInstance.UpdaterDefinition);
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect instance.
        /// </summary>
        /// <param name="effectInstance">The effect instance.</param>
        private void PrepareUpdater(DynamicEffectInstance effectInstance)
        {
            parameterCollections.Clear(true);
            parameterCollections.Add(effectInstance.Effect.CompilationParameters);
            effectInstance.FillParameterCollections(parameterCollections);
            parameterCollections.Add(GraphicsDevice.Parameters);

            updater.Update(effectInstance.UpdaterDefinition, parameterCollections.Items, parameterCollections.Count);
        }
    }
}