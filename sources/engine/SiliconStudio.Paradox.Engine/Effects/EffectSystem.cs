// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// The effect system.
    /// </summary>
    public class EffectSystem : GameSystemBase
    {
        #region Private static members

        public static readonly string DefaultSourceShaderFolder = "shaders";

        #endregion

        #region Private members

        private static Logger Log = GlobalLogger.GetLogger("EffectSystem");

        private readonly IGraphicsDeviceService graphicsDeviceService;
        private Shaders.Compiler.IEffectCompiler compiler;
        private Dictionary<EffectBytecode, Effect> cachedEffects = new Dictionary<EffectBytecode, Effect>();
        private DirectoryWatcher directoryWatcher;

        private readonly List<EffectUpdateInfos> effectsToRecompile = new List<EffectUpdateInfos>();
        private readonly List<EffectUpdateInfos> effectsToUpdate = new List<EffectUpdateInfos>();
        private readonly List<Effect> updatedEffects = new List<Effect>();
        private readonly HashSet<string> modifiedShaders = new HashSet<string>();
        private readonly HashSet<string> recentlyModifiedShaders = new HashSet<string>();
        private bool clearNextFrame = false;

        #endregion

        #region Public members

        public IEffectCompiler Compiler { get { return compiler; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectSystem"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public EffectSystem(IServiceRegistry services)
            : base(services)
        {
            Services.AddService(typeof(EffectSystem), this);

            // Get graphics device service
            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();
        }

        #endregion

        #region Public methods

        public override void Initialize()
        {
            base.Initialize();
            
            // Create compiler
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            var effectCompiler = new Shaders.Compiler.EffectCompiler();
            effectCompiler.SourceDirectories.Add(DefaultSourceShaderFolder);

            Enabled = true;
            directoryWatcher = new DirectoryWatcher("*.pdxsl");
            directoryWatcher.Modified += FileModifiedEvent;

            // TODO: pdxfx too
#else
            var effectCompiler = new NullEffectCompiler();
#endif
            compiler = new EffectCompilerCache(effectCompiler);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // clear at the beginning of the frame
            if (clearNextFrame)
            {
                foreach (var effect in updatedEffects)
                {
                    effect.Changed = false;
                }
                updatedEffects.Clear();

                clearNextFrame = false;
            }

            lock (effectsToUpdate)
            {
                if (effectsToUpdate.Count > 0)
                {
                    foreach (var effectToUpdate in effectsToUpdate)
                        UpdateEffect(effectToUpdate);
                    clearNextFrame = true;
                }
                effectsToUpdate.Clear();
            }
        }

        /// <summary>
        /// Loads the effect.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <returns>A new instance of an effect.</returns>
        /// <exception cref="System.InvalidOperationException">Could not compile shader. Need fallback.</exception>
        public Effect LoadEffect(string effectName, CompilerParameters compilerParameters)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            if (compilerParameters == null) throw new ArgumentNullException("compilerParameters");

            string subEffect;
            // Get the compiled result
            var compilerResult = GetCompilerResults(effectName, compilerParameters, out subEffect);
            CheckResult(compilerResult);

            if (!compilerResult.Bytecodes.ContainsKey(subEffect))
            {
                throw new InvalidOperationException(string.Format("Unable to find sub effect [{0}] from effect [{1}]", subEffect, effectName));
            }

            // Only take the sub-effect
            var bytecode = compilerResult.Bytecodes[subEffect];

            // return it as a fullname instead 
            // TODO: move this to the underlying result, we should not have to do this here
            bytecode.Name = effectName;

            return CreateEffect(bytecode, compilerResult.UsedParameters[subEffect]);
        }

        /// <summary>
        /// Loads the effect and its children.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <returns>A new instance of an effect.</returns>
        /// <exception cref="System.InvalidOperationException">Could not compile shader. Need fallback.</exception>
        public Dictionary<string, Effect> LoadEffects(string effectName, CompilerParameters compilerParameters)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            if (compilerParameters == null) throw new ArgumentNullException("compilerParameters");

            string subEffect;
            var compilerResult = GetCompilerResults(effectName, compilerParameters, out subEffect);
            CheckResult(compilerResult);

            var result = new Dictionary<string, Effect>();

            foreach (var byteCodePair in compilerResult.Bytecodes)
            {
                var bytecode = byteCodePair.Value;
                bytecode.Name = effectName;

                result.Add(byteCodePair.Key, CreateEffect(bytecode, compilerResult.UsedParameters[byteCodePair.Key]));
            }
            return result;
        }

        #endregion

        #region Private methods

        private static void CheckResult(CompilerResults compilerResult)
        {
            // Check errors
            if (compilerResult.HasErrors)
            {
                throw new InvalidOperationException("Could not compile shader. See error messages." + compilerResult.ToText());
            }
        }

        private Effect CreateEffect(EffectBytecode bytecode, ShaderMixinParameters usedParameters)
        {
            Effect effect;
            lock (cachedEffects)
            {
                if (!cachedEffects.TryGetValue(bytecode, out effect))
                {
                    effect = new Effect(graphicsDeviceService.GraphicsDevice, bytecode, usedParameters);
                    effect.Name = bytecode.Name;
                    cachedEffects.Add(bytecode, effect);

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                    foreach (var sourcePath in bytecode.HashSources.Keys)
                    {
                        using (var pathStream = Asset.OpenAsStream(sourcePath + "/path"))
                        using (var reader = new StreamReader(pathStream))
                        {
                            var path = reader.ReadToEnd();
                            directoryWatcher.Track(path);
                        }
                    }
#endif
                }
            }
            return effect;
        }

        private CompilerResults GetCompilerResults(string effectName, CompilerParameters compilerParameters, out string subEffect)
        {
            compilerParameters.Profile = GraphicsDevice.ShaderProfile.HasValue ? GraphicsDevice.ShaderProfile.Value : graphicsDeviceService.GraphicsDevice.Features.Profile;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLCORE
            compilerParameters.Platform = GraphicsPlatform.OpenGL;
#endif
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES 
            compilerParameters.Platform = GraphicsPlatform.OpenGLES;
#endif

            // Get main effect name (before the first dot)
            var mainEffectNameEnd = effectName.IndexOf('.');
            var mainEffectName = mainEffectNameEnd != -1 ? effectName.Substring(0, mainEffectNameEnd) : effectName;

            subEffect = mainEffectNameEnd != -1 ? effectName.Substring(mainEffectNameEnd + 1) : string.Empty;

            // Compile shader
            var isPdxfx = ShaderMixinManager.Contains(mainEffectName);
            var source = isPdxfx ? new ShaderMixinGeneratorSource(mainEffectName) : (ShaderSource)new ShaderClassSource(mainEffectName);

            var compilerResult = compiler.Compile(source, compilerParameters, modifiedShaders, recentlyModifiedShaders);

            foreach (var message in compilerResult.Messages)
            {
                Log.Log(message);
            }

            return compilerResult;
        }

        private void FileModifiedEvent(object sender, FileEvent e)
        {
            if (e.ChangeType == FileEventChangeType.Changed)
            {
                var shaderSourceName = DefaultSourceShaderFolder + "/" + e.Name;
                modifiedShaders.Add(shaderSourceName);
                recentlyModifiedShaders.Add(shaderSourceName);

                lock (cachedEffects)
                {
                    foreach (var bytecode in cachedEffects.Keys)
                    {
                        if (bytecode.HashSources.ContainsKey(shaderSourceName))
                        {
                            var effect = cachedEffects[bytecode];
                            lock (effectsToRecompile)
                            {
                                EffectUpdateInfos updateInfos;
                                updateInfos.Effect = effect;
                                updateInfos.CompilerResults = null;
                                updateInfos.EffectName = null;
                                updateInfos.SubEffectName = null;
                                updateInfos.OldBytecode = bytecode;
                                effectsToRecompile.Add(updateInfos);
                            }
                        }
                    }
                }

                lock (effectsToRecompile)
                {
                    foreach (var effectToRecompile in effectsToRecompile)
                    {
                        RecompileEffect(effectToRecompile);
                    }
                    effectsToRecompile.Clear();
                }
            }
        }

        private void RecompileEffect(EffectUpdateInfos updateInfos)
        {
            var compilerParameters = new CompilerParameters();
            updateInfos.Effect.CompilationParameters.CopyTo(compilerParameters);
            var effectName = updateInfos.Effect.Name;

            string subEffect;
            var compilerResult = GetCompilerResults(effectName, compilerParameters, out subEffect);

            // If there are any errors when recompiling return immediately
            if (compilerResult.HasErrors)
            {
                Log.Error("Effect {0} failed to reompile: {0}", compilerResult.ToText());
                return;
            }

            // update information
            updateInfos.CompilerResults = compilerResult;
            updateInfos.EffectName = effectName;
            updateInfos.SubEffectName = subEffect;
            lock (effectsToUpdate)
            {
                effectsToUpdate.Add(updateInfos);
            }
        }

        private void UpdateEffect(EffectUpdateInfos updateInfos)
        {
            EffectBytecode bytecode;
            try
            {
                bytecode = updateInfos.CompilerResults.Bytecodes[updateInfos.SubEffectName];
            }
            catch (KeyNotFoundException)
            {
                Log.Error("The sub-effect {0} wasn't found in the compiler results.", updateInfos.SubEffectName);
                return;
            }

            ShaderMixinParameters parameters;
            try
            {
                parameters = updateInfos.CompilerResults.UsedParameters[updateInfos.SubEffectName];
            }
            catch (KeyNotFoundException)
            {
                Log.Error("The sub-effect {0} parameters weren't found in the compiler results.", updateInfos.SubEffectName);
                return;
            }

            bytecode.Name = updateInfos.EffectName;
            updateInfos.Effect.Initialize(GraphicsDevice, bytecode, parameters);
            updatedEffects.Add(updateInfos.Effect);

            lock (cachedEffects)
            {
                cachedEffects.Remove(updateInfos.OldBytecode);

                Effect newEffect;
                if (!cachedEffects.TryGetValue(bytecode, out newEffect))
                {
                    cachedEffects.Add(bytecode, updateInfos.Effect);
                }
            }
        }

        #endregion

        #region Helpers

        private struct EffectUpdateInfos
        {
            public Effect Effect;
            public CompilerResults CompilerResults;
            public string EffectName;
            public string SubEffectName;
            public EffectBytecode OldBytecode;
        }

        #endregion
    }
}