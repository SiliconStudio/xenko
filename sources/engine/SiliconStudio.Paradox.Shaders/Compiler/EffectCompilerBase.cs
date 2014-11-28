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

            string effectName = null;

            var modifiedShaders = compilerParameters.ModifiedShaders;

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
                mixinTree = ShaderMixinManager.Generate(effectName, compilerParameters);
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
                    mixinTree = new ShaderMixinSourceTree() { Mixin = shaderMixinSource, UsedParameters =  new ShaderMixinParameters()};
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
            var compilerResults = new CompilerResults { Module = string.Format("EffectCompile [{0}]", effectName) };
            var internalCompilerParameters = new InternalCompilerParameters(mixinTree, compilerParameters, compilerResults);

            var wasCompiled = Compile(string.Empty, internalCompilerParameters, compilerResults);

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
        /// <param name="name">The name.</param>
        /// <param name="internalCompilerParameters">The internal compiler parameters.</param>
        /// <param name="compilerResults">The compiler results.</param>
        /// <returns>true if the compilation succeded, false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        private bool Compile(string name, InternalCompilerParameters internalCompilerParameters, CompilerResults compilerResults)
        {
            if (name == null) throw new ArgumentNullException("name");

            var mixinTree = internalCompilerParameters.MixinTree;
            if (mixinTree.Mixin == null) return false;
            
            var bytecode = Compile(internalCompilerParameters);

            var wasCompiled = false;
            if (bytecode != null)
            {
                if (internalCompilerParameters.MixinTree.Parent == null)
                {
                    compilerResults.MainBytecode = bytecode;
                    compilerResults.MainUsedParameters = internalCompilerParameters.MixinTree.UsedParameters;
                }
                compilerResults.Bytecodes.Add(name, bytecode);
                compilerResults.UsedParameters.Add(name, internalCompilerParameters.MixinTree.UsedParameters);

                wasCompiled = true;
            }

            foreach (var childTree in mixinTree.Children)
            {
                var childName = (string.IsNullOrEmpty(name) ? string.Empty : name + ".") + childTree.Value.Name;

                var subParameters = internalCompilerParameters;
                subParameters.MixinTree = childTree.Value;

                wasCompiled |= Compile(childName, subParameters, compilerResults);
            }

            return wasCompiled;
        }

        /// <summary>
        /// Compiles the ShaderMixinSource into a platform bytecode.
        /// </summary>
        /// <param name="internalCompilerParameters">The internal compiler parameters.</param>
        /// <returns>The platform-dependent bytecode.</returns>
        public abstract EffectBytecode Compile(InternalCompilerParameters internalCompilerParameters);

        /// <summary>
        /// Internal parameters used to compile a shader.
        /// </summary>
        public struct InternalCompilerParameters
        {
            private ShaderMixinSourceTree mixinTree;

            private readonly CompilerParameters compilerParameters;

            private LoggerResult log;

            public InternalCompilerParameters(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log)
            {
                this.mixinTree = mixinTree;
                this.compilerParameters = compilerParameters ;
                this.log = log;
            }

            public ShaderMixinSourceTree MixinTree
            {
                get
                {
                    return mixinTree;
                }
                set
                {
                    mixinTree = value;
                }
            }

            public CompilerParameters CompilerParameters
            {
                get
                {
                    return compilerParameters;
                }
            }

            public LoggerResult Log
            {
                get
                {
                    return log;
                }
                set
                {
                    log = value;
                }
            }
        }

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