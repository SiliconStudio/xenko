// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public CompilerResults Compile(ShaderSource shaderSource, CompilerParameters compilerParameters)
        {
            ShaderMixinSourceTree mixinTree;
            var shaderMixinGeneratorSource = shaderSource as ShaderMixinGeneratorSource;

            string mainEffectName = null;

            var modifiedShaders = compilerParameters.ModifiedShaders;

            if (shaderMixinGeneratorSource != null)
            {
                string subEffect;
                mainEffectName = GetEffectName(shaderMixinGeneratorSource.Name, out subEffect);

                // getting the effect from the used parameters only makes sense when the source files are the same
                // TODO: improve this by updating earlyCompilerCache - cache can still be relevant
                if (modifiedShaders == null || modifiedShaders.Count == 0)
                {
                    // perform an early test only based on the parameters
                    var foundCompilerResults = GetShaderFromParameters(mainEffectName, subEffect, compilerParameters);
                    if (foundCompilerResults != null)
                    {
                        return foundCompilerResults;
                    }
                }
                mixinTree = ShaderMixinManager.Generate(mainEffectName, compilerParameters);
            }
            else
            {
                mainEffectName = "Effect";

                var shaderMixinSource = shaderSource as ShaderMixinSource;
                var shaderClassSource = shaderSource as ShaderClassSource;

                if (shaderClassSource != null)
                {
                    shaderMixinSource = new ShaderMixinSource();
                    shaderMixinSource.Mixins.Add(shaderClassSource);
                    mainEffectName = shaderClassSource.ClassName;
                }

                if (shaderMixinSource != null)
                {
                    mixinTree = new ShaderMixinSourceTree() { Name = mainEffectName,  Mixin = shaderMixinSource, UsedParameters =  new ShaderMixinParameters()};
                }
                else
                {
                    throw new ArgumentException("Unsupported ShaderSource type [{0}]. Supporting only ShaderMixinSource/pdxfx, ShaderClassSource", "shaderSource");
                }
            }

            // Copy global parameters to used Parameters by default, as it is used by the compiler
            mixinTree.SetGlobalUsedParameter(CompilerParameters.GraphicsPlatformKey, compilerParameters.Platform);
            mixinTree.SetGlobalUsedParameter(CompilerParameters.GraphicsProfileKey, compilerParameters.Profile);
            mixinTree.SetGlobalUsedParameter(CompilerParameters.DebugKey, compilerParameters.Debug);

            // Compile the whole mixin tree
            var compilerResults = new CompilerResults { Module = string.Format("EffectCompile [{0}]", mainEffectName) };
            var wasCompiled = Compile(string.Empty, mixinTree, compilerParameters, compilerResults);

            if (wasCompiled && shaderMixinGeneratorSource != null)
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
                    effectCompilerResults.Add(compilerResults);
                }
            }

            return compilerResults;
        }

        /// <summary>
        /// Compile the effect and its children.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <param name="compilerResults">The compiler results.</param>
        /// <returns>true if the compilation succeded, false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        private bool Compile(string name, ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, CompilerResults compilerResults)
        {
            if (name == null) throw new ArgumentNullException("name");

            if (mixinTree.Mixin == null) return false;

            var bytecode = Compile(mixinTree, compilerParameters, compilerResults);

            var wasCompiled = false;
            if (bytecode != null)
            {
                if (mixinTree.Parent == null)
                {
                    compilerResults.MainBytecode = bytecode;
                    compilerResults.MainUsedParameters = mixinTree.UsedParameters;
                }
                compilerResults.Bytecodes.Add(name, bytecode);
                compilerResults.UsedParameters.Add(name, mixinTree.UsedParameters);

                wasCompiled = true;
            }

            foreach (var childTree in mixinTree.Children)
            {
                var childName = (string.IsNullOrEmpty(name) ? string.Empty : name + ".") + childTree.Value.Name;
                wasCompiled |= Compile(childName, childTree.Value, compilerParameters, compilerResults);
            }

            return wasCompiled;
        }

        /// <summary>
        /// Compiles the ShaderMixinSource into a platform bytecode.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <param name="log">The log.</param>
        /// <returns>The platform-dependent bytecode.</returns>
        public abstract EffectBytecode Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log);

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

        public static string GetEffectName(string fullEffectName, out string subEffect)
        {
            var mainEffectNameEnd = fullEffectName.IndexOf('.');
            var mainEffectName = mainEffectNameEnd != -1 ? fullEffectName.Substring(0, mainEffectNameEnd) : fullEffectName;

            subEffect = mainEffectNameEnd != -1 ? fullEffectName.Substring(mainEffectNameEnd + 1) : string.Empty;
            return mainEffectName;
        }
    }
}