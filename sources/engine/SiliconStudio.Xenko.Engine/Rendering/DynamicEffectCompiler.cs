// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Provides a dynamic compiler for an effect based on parameters changed.
    /// </summary>
    public class DynamicEffectCompiler
    {
        // How long to wait before trying to recompile an effect that failed compilation (only on Windows Desktop)
        private static readonly TimeSpan ErrorCheckTimeSpan = new TimeSpan(TimeSpan.TicksPerSecond);

        private FastListStruct<ParameterCollection> parameterCollections;

        private readonly string effectName;
        private bool asyncEffectCompiler;
        private int taskPriority;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEffectCompiler" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="taskPriority">The task priority.</param>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// effectName</exception>
        public DynamicEffectCompiler(IServiceRegistry services, string effectName, int taskPriority = 0)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (effectName == null) throw new ArgumentNullException("effectName");

            Services = services;
            this.effectName = effectName;
            this.taskPriority = taskPriority;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            parameterCollections = new FastListStruct<ParameterCollection>(8);

            // Default behavior for fallback effect: load effect with same name but empty compiler parameters
            ComputeFallbackEffect = (dynamicEffectCompiler, type, name, parameters) =>
            {
                ParameterCollection usedParameters;
                var compilerParameters = new CompilerParameters { TaskPriority = -1 };

                // We want high priority

                var effect = dynamicEffectCompiler.EffectSystem.LoadEffect(effectName, compilerParameters, out usedParameters).WaitForResult();
                return new ComputeFallbackEffectResult(effect, usedParameters);
            };
        }

        public bool AsyncEffectCompiler
        {
            get { return asyncEffectCompiler; }
            set { asyncEffectCompiler = value; }
        }

        public delegate ComputeFallbackEffectResult ComputeFallbackEffectDelegate(DynamicEffectCompiler dynamicEffectCompiler, FallbackEffectType fallbackEffectType, string effectName, CompilerParameters compilerParameters);

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
                    if (currentlyCompilingEffect.IsFaulted)
                    {
                        var compilerParameters = new CompilerParameters();
                        effectInstance.CurrentlyCompilingUsedParameters.CopyTo(compilerParameters);

                        SwitchFallbackEffect(FallbackEffectType.Error, effectInstance, passParameters, compilerParameters);
                    }
                    else
                    {
                        effectInstance.HasErrors = false;
                        // Do not update effect right away: passParameters might have changed since last compilation; just try to go through a CreateEffect that will properly update the effect synchronously
                        // TODO: This class (and maybe whole ParameterCollection system) need a complete rethink and rewrite with newest assumptions...
                        //UpdateEffect(effectInstance, currentlyCompilingEffect.Result, effectInstance.CurrentlyCompilingUsedParameters, passParameters);
                    }

                    effectChanged = true;

                    // Effect has been updated
                    effectInstance.CurrentlyCompilingEffect = null;
                    effectInstance.CurrentlyCompilingUsedParameters = null;
                }
            }

            if (effectChanged || // Check again, in case effect was just finished async compilation
                (effectInstance.Effect == null || !EffectSystem.IsValid(effectInstance.Effect) || HasCollectionChanged(effectInstance, passParameters) || effectInstance.HasErrors))
            {
                if (effectInstance.HasErrors)
                {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                    var currentTime = DateTime.Now;
                    if (currentTime < effectInstance.LastErrorCheck + ErrorCheckTimeSpan)
                    {
                        // Wait a regular interval before retrying to compile effect (i.e. every second)
                        return false;
                    }

                    // Update last check time
                    effectInstance.LastErrorCheck = currentTime;
#else
                    // Other platforms: never try to recompile failed effects for now
                    return false;
#endif
                }

                CreateEffect(effectInstance, passParameters);
                effectChanged = true;
            }

            return effectChanged;
        }

        public void SwitchFallbackEffect(FallbackEffectType fallbackEffectType, DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            var compilerParameters = BuildCompilerParameters(effectInstance, passParameters);
            SwitchFallbackEffect(fallbackEffectType, effectInstance, passParameters, compilerParameters);
        }

        private void SwitchFallbackEffect(FallbackEffectType fallbackEffectType, DynamicEffectInstance effectInstance, ParameterCollection passParameters, CompilerParameters compilerParameters)
        {
            // Fallback for errors
            effectInstance.HasErrors = true;
            var fallbackEffect = ComputeFallbackEffect(this, fallbackEffectType, EffectName, compilerParameters);
            UpdateEffect(effectInstance, fallbackEffect.Effect, fallbackEffect.UsedParameters, passParameters);
        }

        private bool HasCollectionChanged(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            PrepareUpdater(effectInstance, passParameters);
            return effectInstance.ParameterCollectionGroup.HasChanged(effectInstance.UpdaterDefinition);
        }

        private void CreateEffect(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            var compilerParameters = BuildCompilerParameters(effectInstance, passParameters);

            // Compile shader
            // possible exception in LoadEffect
            TaskOrResult<Effect> effect;
            ParameterCollection usedParameters;
            try
            {
                effect = EffectSystem.LoadEffect(EffectName, compilerParameters, out usedParameters);
            }
            catch (Exception)
            {
                SwitchFallbackEffect(FallbackEffectType.Error, effectInstance, passParameters, compilerParameters);
                return;
            }

            // Do we have an async compilation?
            if (asyncEffectCompiler && effect.Task != null)
            {
                effectInstance.CurrentlyCompilingEffect = effect.Task;
                effectInstance.CurrentlyCompilingUsedParameters = usedParameters;

                if (!effectInstance.HasErrors) // If there was an error, stay in that state (we don't want to switch between reloading and error states)
                {
                    // Fallback to default effect
                    var fallbackEffect = ComputeFallbackEffect(this, FallbackEffectType.Compiling, EffectName, compilerParameters);
                    UpdateEffect(effectInstance, fallbackEffect.Effect, fallbackEffect.UsedParameters, passParameters);
                }
                return;
            }

            // TODO It throws an exception here when the compilation fails!
            var compiledEffect = effect.WaitForResult();

            UpdateEffect(effectInstance, compiledEffect, usedParameters, passParameters);

            // Effect has been updated
            effectInstance.CurrentlyCompilingEffect = null;
            effectInstance.CurrentlyCompilingUsedParameters = null;
        }

        private CompilerParameters BuildCompilerParameters(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            var compilerParameters = new CompilerParameters();
            parameterCollections.Clear();
            if (passParameters != null)
            {
                parameterCollections.Add(passParameters);
            }
            effectInstance.FillParameterCollections(ref parameterCollections);

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

            compilerParameters.TaskPriority = taskPriority;

            foreach (var parameter in GraphicsDevice.Parameters.InternalValues)
            {
                compilerParameters.SetObject(parameter.Key, parameter.Value.Object);
            }
            return compilerParameters;
        }

        private void UpdateEffect(DynamicEffectInstance effectInstance, Effect compiledEffect, ParameterCollection usedParameters, ParameterCollection passParameters)
        {
            if (!ReferenceEquals(compiledEffect, effectInstance.Effect))
            {
                effectInstance.Effect = compiledEffect;
                effectInstance.UpdaterDefinition = new DynamicEffectParameterUpdaterDefinition(compiledEffect, usedParameters);
                effectInstance.ParameterCollectionGroup = null; // When Effect changes, first collection changes too
            }
            else
            {
                // Same effect than previous one

                effectInstance.UpdaterDefinition.UpdatedUsedParameters(compiledEffect, usedParameters);
            }

            UpdateLevels(effectInstance, passParameters);
            effectInstance.ParameterCollectionGroup.UpdateCounters(effectInstance.UpdaterDefinition);
        }

        private void UpdateLevels(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            PrepareUpdater(effectInstance, passParameters);
            effectInstance.ParameterCollectionGroup.ComputeLevels(effectInstance.UpdaterDefinition);
        }

        /// <summary>
        /// Prepare the EffectParameterUpdater for the effect instance.
        /// </summary>
        /// <param name="effectInstance">The effect instance.</param>
        /// <param name="passParameters">The pass parameters.</param>
        private void PrepareUpdater(DynamicEffectInstance effectInstance, ParameterCollection passParameters)
        {
            parameterCollections.Clear();
            parameterCollections.Add(effectInstance.UpdaterDefinition.Parameters);
            if (passParameters != null)
            {
                parameterCollections.Add(passParameters);
            }
            effectInstance.FillParameterCollections(ref parameterCollections);
            parameterCollections.Add(GraphicsDevice.Parameters);

            // Collections are mostly stable, but sometimes not (i.e. material change)
            // TODO: We can improve performance by redesigning FillParameterCollections to avoid ArrayExtensions.ArraysReferenceEqual (or directly check the appropriate parameter collections)
            // This also happens in another place: RenderMesh (we probably want to factorize it when doing additional optimizations)
            if (effectInstance.ParameterCollectionGroup == null || !ArrayExtensions.ArraysReferenceEqual(effectInstance.ParameterCollectionGroup.ParameterCollections, parameterCollections))
            {
                effectInstance.ParameterCollectionGroup = new DynamicEffectParameterCollectionGroup(parameterCollections.ToArray());

                // Reset counters, to force comparison of values again (in DynamicEffectParameterCollectionGroup.HasChanged).
                // (ideally, we should only reset counters of collections that changed to avoid unecessary comparisons)
                var sortedCounters = effectInstance.UpdaterDefinition.SortedCounters;
                for (int i = 0; i < sortedCounters.Length; ++i)
                {
                    sortedCounters[i] = 0;
                }
            }

            effectInstance.ParameterCollectionGroup.Update(effectInstance.UpdaterDefinition);
        }

        public struct ComputeFallbackEffectResult
        {
            public readonly Effect Effect;
            public readonly ParameterCollection UsedParameters;

            public ComputeFallbackEffectResult(Effect effect, ParameterCollection usedParameters)
            {
                Effect = effect;
                UsedParameters = usedParameters;
            }
        }
    }
}