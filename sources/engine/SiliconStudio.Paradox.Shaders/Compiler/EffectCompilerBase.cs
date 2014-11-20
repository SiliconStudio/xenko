// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Base class for implementations of <see cref="IEffectCompiler"/>, providing some helper functions.
    /// </summary>
    public abstract class EffectCompilerBase : IEffectCompiler
    {
        private readonly Dictionary<string, List<CompilerResults>> earlyCompilerCache = new Dictionary<string, List<CompilerResults>>();

        protected EffectCompilerBase()
        {
        }

        public CompilerResults Compile(ShaderSource shaderSource, CompilerParameters compilerParameters, HashSet<string> modifiedShaders, HashSet<string> recentlyModifiedShaders)
        {
            ShaderMixinSourceTree mixinTree;
            var shaderMixinGeneratorSource = shaderSource as ShaderMixinGeneratorSource;
            var mainUsedParameters = new ShaderMixinParameters();
            var usedParameters = new List<ShaderMixinParameters>();

            string effectName = null;

            if (shaderMixinGeneratorSource != null)
            {
                effectName = shaderMixinGeneratorSource.Name;

                // getting the effect from the used parameters only makes sense when the source files are the same
                // TODO: improve this by updating earlyCompilerCache - cache can still be relevant
                if (modifiedShaders == null || modifiedShaders.Count == 0)
                {
                    // perform an early test only based on the parameters
                    var foundCompilerResults = GetShaderFromParameters(effectName, compilerParameters);
                    if (foundCompilerResults != null)
                    {
                        var earlyCompilerResults = new CompilerResults();
                        earlyCompilerResults.Module = string.Format("EffectCompile [{0}]", effectName);
                        earlyCompilerResults.MainBytecode = foundCompilerResults.MainBytecode;
                        earlyCompilerResults.MainUsedParameters = foundCompilerResults.MainUsedParameters;
                        foreach (var foundBytecode in foundCompilerResults.Bytecodes)
                        {
                            earlyCompilerResults.Bytecodes.Add(foundBytecode.Key, foundBytecode.Value);
                        }

                        foreach (var foundUsedParameters in foundCompilerResults.UsedParameters)
                        {
                            earlyCompilerResults.UsedParameters.Add(foundUsedParameters.Key, foundUsedParameters.Value);
                        }
                        return earlyCompilerResults;
                    }
                }
                mixinTree = ShaderMixinManager.Generate(effectName, compilerParameters, out mainUsedParameters, out usedParameters);
            }
            else
            {
                effectName = "Effect";

                var shaderMixinSource = shaderSource as ShaderMixinSource;
                var shaderClassSource = shaderSource as ShaderClassSource;

                if (shaderClassSource != null)
                {
                    shaderMixinSource = new ShaderMixinSource();
                    shaderMixinSource.Mixins.Add(shaderClassSource);
                }

                if (shaderMixinSource != null)
                {
                    mixinTree = new ShaderMixinSourceTree() { Mixin = shaderMixinSource };
                }
                else
                {
                    throw new ArgumentException("Unsupported ShaderSource type [{0}]. Supporting only ShaderMixinSource/pdxfx, ShaderClassSource", "shaderSource");
                }
            }

            // Copy global parameters to used Parameters by default, as it is used by the compiler
            mainUsedParameters.Set(CompilerParameters.GraphicsPlatformKey, compilerParameters.Platform);
            mainUsedParameters.Set(CompilerParameters.GraphicsProfileKey, compilerParameters.Profile);
            mainUsedParameters.Set(CompilerParameters.DebugKey, compilerParameters.Debug);

            foreach (var parameters in usedParameters)
            {
                parameters.Set(CompilerParameters.GraphicsPlatformKey, compilerParameters.Platform);
                parameters.Set(CompilerParameters.GraphicsProfileKey, compilerParameters.Profile);
                parameters.Set(CompilerParameters.DebugKey, compilerParameters.Debug);
            }

            // Compile the whole mixin tree
            var compilerResults = new CompilerResults();
            compilerResults.Module = string.Format("EffectCompile [{0}]", effectName);
            var wasCompiled = Compile(string.Empty, effectName, mixinTree, mainUsedParameters, usedParameters, modifiedShaders, recentlyModifiedShaders, compilerResults);

            if (wasCompiled && shaderMixinGeneratorSource != null)
            {
                lock (earlyCompilerCache)
                {
                    List<CompilerResults> effectCompilerResults;
                    if (!earlyCompilerCache.TryGetValue(effectName, out effectCompilerResults))
                    {
                        effectCompilerResults = new List<CompilerResults>();
                        earlyCompilerCache.Add(effectName, effectCompilerResults);
                    }

                    // Register bytecode used parameters so that they are checked when another effect is instanced
                    effectCompilerResults.Add(compilerResults);
                }
            }

            return compilerResults;
        }

        /// <summary>
        /// Compile the effect and its children.
        /// </summary>
        /// <param name="effectName">The name of the effect (without the base effect name).</param>
        /// <param name="fullEffectName">The full name of the effect (with the base effect name).</param>
        /// <param name="mixinTree">The ShaderMixinSourceTree.</param>
        /// <param name="mainCompilerParameters">The parameters used to create the main effect.</param>
        /// <param name="compilerParameters">The parameters used to create the child effects</param>
        /// <param name="modifiedShaders">The list of modified shaders since the beginning of the runtime.</param>
        /// <param name="recentlyModifiedShaders">The list of modified shaders that have not been replaced yet.</param>
        /// <param name="compilerResults">The result of the compilation.</param>
        /// <returns>true if the compilation succeded, false otherwise.</returns>
        protected virtual bool Compile(string effectName, string fullEffectName, ShaderMixinSourceTree mixinTree, ShaderMixinParameters mainCompilerParameters, List<ShaderMixinParameters> compilerParameters, HashSet<string> modifiedShaders, HashSet<string> recentlyModifiedShaders, CompilerResults compilerResults)
        {
            if (mixinTree.Mixin == null) return false;

            var cp = compilerParameters.FirstOrDefault(x => x.Name == fullEffectName);
            var bytecode = Compile(mixinTree.Mixin, fullEffectName, cp ?? mainCompilerParameters, modifiedShaders, recentlyModifiedShaders, compilerResults);

            var wasCompiled = false;
            if (bytecode != null)
            {
                if (effectName == string.Empty)
                {
                    compilerResults.MainBytecode = bytecode;
                    compilerResults.MainUsedParameters = mainCompilerParameters;
                }
                compilerResults.Bytecodes.Add(effectName, bytecode);
                compilerResults.UsedParameters.Add(effectName, cp);

                wasCompiled = true;
            }

            foreach (var childTree in mixinTree.Children)
            {
                var childEffectName = effectName == string.Empty ? childTree.Key : effectName + "." + childTree.Key;
                var fullChildEffectName = fullEffectName == string.Empty ? childTree.Key : fullEffectName + "." + childTree.Key;
                wasCompiled |= Compile(childEffectName, fullChildEffectName, childTree.Value, mainCompilerParameters, compilerParameters, modifiedShaders, recentlyModifiedShaders, compilerResults);
            }

            return wasCompiled;
        }

        /// <summary>
        /// Compiles the ShaderMixinSource into a platform bytecode.
        /// </summary>
        /// <param name="mixin">The ShaderMixinSource.</param>
        /// <param name="fullEffectName">The name of the effect.</param>
        /// <param name="compilerParameters">The parameters used for compilation.</param>
        /// <param name="modifiedShaders">The list of modified shaders.</param>
        /// <param name="recentlyModifiedShaders">The list of recently modified shaders.</param>
        /// <param name="log">The logger.</param>
        /// <returns>The platform-dependent bytecode.</returns>
        public abstract EffectBytecode Compile(ShaderMixinSource mixin, string fullEffectName, ShaderMixinParameters compilerParameters, HashSet<string> modifiedShaders, HashSet<string> recentlyModifiedShaders, LoggerResult log);

        /// <summary>
        /// Get the shader from the database based on the parameters used for its compilation.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The EffectBytecode if found.</returns>
        protected CompilerResults GetShaderFromParameters(string effectName, CompilerParameters parameters)
        {
            lock (earlyCompilerCache)
            {
                List<CompilerResults> effectCompilerResults;
                if (!earlyCompilerCache.TryGetValue(effectName, out effectCompilerResults))
                    return null;

                // TODO: Optimize it so that search is not linear?
                // TODO: are macros taken into account?
                // Probably not trivial for subset testing
                foreach (var compiledResults in effectCompilerResults)
                {
                    if (compiledResults.MainUsedParameters != null && parameters.Contains(compiledResults.MainUsedParameters))
                        return compiledResults;
                }
            }

            return null;
        }
    }
}