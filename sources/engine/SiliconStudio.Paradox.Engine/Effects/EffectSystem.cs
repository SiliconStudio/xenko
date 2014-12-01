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
        private readonly static Logger Log = GlobalLogger.GetLogger("EffectSystem");

        private readonly IGraphicsDeviceService graphicsDeviceService;
        private EffectCompilerBase compiler;
        private readonly Dictionary<string, List<CompilerResults>> earlyCompilerCache = new Dictionary<string, List<CompilerResults>>();
        private Dictionary<EffectBytecode, Effect> cachedEffects = new Dictionary<EffectBytecode, Effect>();
        private DirectoryWatcher directoryWatcher;

        private readonly HashSet<string> recentlyModifiedShaders = new HashSet<string>();
        private bool clearNextFrame = false;

        public IEffectCompiler Compiler { get { return compiler; } }

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

        public override void Initialize()
        {
            base.Initialize();
            
            // Create compiler
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            var effectCompiler = new Shaders.Compiler.EffectCompiler();
            effectCompiler.SourceDirectories.Add(EffectCompilerBase.DefaultSourceShaderFolder);

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

            UpdateEffects();
        }

        public bool IsValid(Effect effect)
        {
            lock (cachedEffects)
            {
                return cachedEffects.ContainsKey(effect.Bytecode);
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
            return CreateEffect(effectName, bytecode, compilerResult.UsedParameters[subEffect]);
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
                result.Add(byteCodePair.Key, CreateEffect(effectName, bytecode, compilerResult.UsedParameters[byteCodePair.Key]));
            }
            return result;
        }

        // TODO: THIS IS JUST A WORKAROUND, REMOVE THIS

        private static void CheckResult(CompilerResults compilerResult)
        {
            // Check errors
            if (compilerResult.HasErrors)
            {
                throw new InvalidOperationException("Could not compile shader. See error messages." + compilerResult.ToText());
            }
        }

        private Effect CreateEffect(string effectName, EffectBytecode bytecode, ShaderMixinParameters usedParameters)
        {
            Effect effect;
            lock (cachedEffects)
            {
                if (!cachedEffects.TryGetValue(bytecode, out effect))
                {
                    effect = new Effect(graphicsDeviceService.GraphicsDevice, bytecode, usedParameters) { Name = effectName };
                    cachedEffects.Add(bytecode, effect);

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                    foreach (var type in bytecode.HashSources.Keys)
                    {
                        // TODO: the "/path" is hardcoded, used in ImportStreamCommand and ShaderSourceManager. Find a place to share this correctly.
                        using (var pathStream = Asset.OpenAsStream(EffectCompilerBase.GetStoragePathFromShaderType(type) + "/path"))
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
            var mainEffectName = EffectCompilerBase.GetEffectName(effectName, out subEffect);

            // Compile shader
            var isPdxfx = ShaderMixinManager.Contains(mainEffectName);
            var source = isPdxfx ? new ShaderMixinGeneratorSource(effectName) : (ShaderSource)new ShaderClassSource(mainEffectName);

            // getting the effect from the used parameters only makes sense when the source files are the same
            // TODO: improve this by updating earlyCompilerCache - cache can still be relevant

            CompilerResults compilerResult = null;

            if (isPdxfx)
            {
                // perform an early test only based on the parameters
                compilerResult = GetShaderFromParameters(mainEffectName, subEffect, compilerParameters);
            }

            if (compilerResult == null)
            {
                compilerResult = compiler.Compile(source, compilerParameters);

                if (!compilerResult.HasErrors && isPdxfx)
                {
                    lock (earlyCompilerCache)
                    {
                        List<CompilerResults> effectCompilerResults;
                        if (!earlyCompilerCache.TryGetValue(mainEffectName, out effectCompilerResults))
                        {
                            effectCompilerResults = new List<CompilerResults>();
                            earlyCompilerCache.Add(mainEffectName, effectCompilerResults);
                        }

                        // Register bytecode used parameters so that they are checked when another effect is instanced
                        effectCompilerResults.Add(compilerResult);
                    }
                }
            }

            foreach (var message in compilerResult.Messages)
            {
                Log.Log(message);
            }

            return compilerResult;
        }

        private void UpdateEffects()
        {
            lock (recentlyModifiedShaders)
            {
                if (recentlyModifiedShaders.Count == 0)
                {
                    return;
                }

                // Clear cache for recently modified shaders
                compiler.ResetCache(recentlyModifiedShaders);

                var bytecodeRemoved = new List<EffectBytecode>();

                lock (cachedEffects)
                {
                    foreach (var shaderSourceName in recentlyModifiedShaders)
                    {
                        // TODO: cache keys in a HashSet instead of ToHashSet
                        var bytecodes = new HashSet<EffectBytecode>(cachedEffects.Keys);
                        foreach (var bytecode in bytecodes)
                        {
                            if (bytecode.HashSources.ContainsKey(shaderSourceName))
                            {
                                bytecodeRemoved.Add(bytecode);

                                // Dispose previous effect
                                var effect = cachedEffects[bytecode];
                                effect.Dispose();

                                // Remove effect from cache
                                cachedEffects.Remove(bytecode);
                            }
                        }
                    }
                }

                lock (earlyCompilerCache)
                {
                    foreach (var effectCompilerResults in earlyCompilerCache.Values)
                    {
                        foreach (var bytecode in bytecodeRemoved)
                        {
                            effectCompilerResults.RemoveAll(results => results.Bytecodes.Values.Contains(bytecode));
                        }
                    }
                }


                recentlyModifiedShaders.Clear();
            }
        }

        private void FileModifiedEvent(object sender, FileEvent e)
        {
            if (e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed)
            {
                lock (recentlyModifiedShaders)
                {
                    recentlyModifiedShaders.Add(Path.GetFileNameWithoutExtension(e.Name));
                }
            }
        }

        /// <summary>
        /// Get the shader from the database based on the parameters used for its compilation.
        /// </summary>
        /// <param name="rootEffectName">Name of the effect.</param>
        /// <param name="subEffectName">Name of the sub effect.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The EffectBytecode if found.</returns>
        protected CompilerResults GetShaderFromParameters(string rootEffectName, string subEffectName, CompilerParameters parameters)
        {
            lock (earlyCompilerCache)
            {
                List<CompilerResults> compilerResultsList;
                if (!earlyCompilerCache.TryGetValue(rootEffectName, out compilerResultsList))
                    return null;

                // TODO: Optimize it so that search is not linear?
                // Probably not trivial for subset testing
                foreach (var compiledResults in compilerResultsList)
                {
                    ShaderMixinParameters usedParameters;
                    if (compiledResults.UsedParameters.TryGetValue(subEffectName, out usedParameters) && parameters.Contains(usedParameters))
                    {
                        return compiledResults;
                    }
                }
            }

            return null;
        }
    }
}