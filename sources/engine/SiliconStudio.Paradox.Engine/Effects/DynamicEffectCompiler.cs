// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects
{
    public class DynamicEffectCompiler
    {
        private readonly EffectParameterUpdater updater;
        private readonly FastList<ParameterCollection> parameterCollections;

        public DynamicEffectCompiler(IServiceRegistry services, string effectName)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (effectName == null) throw new ArgumentNullException("effectName");
            Services = services;
            EffectName = effectName;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            updater = new EffectParameterUpdater();
            parameterCollections = new FastList<ParameterCollection>();
        }

        public IServiceRegistry Services { get; private set; }

        public string EffectName { get; private set; }

        private EffectSystem EffectSystem { get; set; }

        private GraphicsDevice GraphicsDevice { get; set; }

        public bool Update(DynamicEffectHolder effectHolder)
        {
            bool effectChanged = false;

            if (effectHolder.Effect != null && effectHolder.Effect.Changed)
            {
                effectHolder.UpdaterDefinition.Initialize(effectHolder.Effect);
                UpdateLevels(effectHolder);
                effectChanged = true;
            }

            if (effectHolder.Effect == null || HasCollectionChanged(effectHolder))
            {
                CreateEffect(effectHolder);
                effectChanged = true;
            }

            return effectChanged;
        }

        private bool HasCollectionChanged(DynamicEffectHolder effectHolder)
        {
            PrepareUpdater(effectHolder);
            return updater.HasChanged(effectHolder.UpdaterDefinition);
        }

        private void CreateEffect(DynamicEffectHolder effectHolder)
        {
            var compilerParameters = new CompilerParameters();
            parameterCollections.Clear(true);
            effectHolder.FillParameterCollections(parameterCollections);

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

            if (!ReferenceEquals(effect, effectHolder.Effect))
            {
                // Copy back parameters set on previous effect to new effect
                if (effectHolder.Effect != null)
                {
                    foreach (var parameter in effectHolder.Effect.Parameters.InternalValues)
                    {
                        effect.Parameters.SetObject(parameter.Key, parameter.Value.Object);
                    }
                }

                effectHolder.Effect = effect;
                effectHolder.UpdaterDefinition = new EffectParameterUpdaterDefinition(effect);
            }
            else
            {
                // Same effect than previous one

                effectHolder.UpdaterDefinition.UpdateCounter(effect.CompilationParameters);
            }

            UpdateLevels(effectHolder);
            updater.UpdateCounters(effectHolder.UpdaterDefinition);
        }

        private void UpdateLevels(DynamicEffectHolder effectHolder)
        {
            PrepareUpdater(effectHolder);
            updater.ComputeLevels(effectHolder.UpdaterDefinition);
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect instance.
        /// </summary>
        /// <param name="effectHolder">The effect instance.</param>
        private void PrepareUpdater(DynamicEffectHolder effectHolder)
        {
            parameterCollections.Clear(true);
            parameterCollections.Add(effectHolder.Effect.DefaultCompilationParameters);
            effectHolder.FillParameterCollections(parameterCollections);
            parameterCollections.Add(GraphicsDevice.Parameters);

            updater.Update(effectHolder.UpdaterDefinition, parameterCollections.Items, parameterCollections.Count);
        }
    }
}