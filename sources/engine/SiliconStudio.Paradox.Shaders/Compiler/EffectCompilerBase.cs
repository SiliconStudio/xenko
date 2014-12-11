// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Base class for implementations of <see cref="IEffectCompiler"/>, providing some helper functions.
    /// </summary>
    public abstract class EffectCompilerBase : IEffectCompiler
    {
        protected EffectCompilerBase()
        {
        }

        public virtual ObjectId GetShaderSourceHash(string type)
        {
            return ObjectId.Empty;
        }

        /// <summary>
        /// Remove cached files for modified shaders
        /// </summary>
        /// <param name="modifiedShaders"></param>
        public virtual void ResetCache(HashSet<string> modifiedShaders)
        {
        }

        public CompilerResults Compile(ShaderSource shaderSource, CompilerParameters compilerParameters)
        {
            ShaderMixinSourceTree mixinTree;
            var shaderMixinGeneratorSource = shaderSource as ShaderMixinGeneratorSource;

            string mainEffectName = null;

            if (shaderMixinGeneratorSource != null)
            {
                string subEffect;
                mainEffectName = GetEffectName(shaderMixinGeneratorSource.Name, out subEffect);
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
            Compile(string.Empty, mixinTree, compilerParameters, compilerResults);

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
        private void Compile(string name, ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, CompilerResults compilerResults)
        {
            if (name == null) throw new ArgumentNullException("name");
            var bytecode = Compile(mixinTree, compilerParameters, compilerResults);

            if (bytecode != null)
            {
                if (mixinTree.Parent == null)
                {
                    compilerResults.MainBytecode = bytecode;
                    compilerResults.MainUsedParameters = mixinTree.UsedParameters;
                }
                compilerResults.Bytecodes.Add(name, bytecode);
                compilerResults.UsedParameters.Add(name, mixinTree.UsedParameters);
            }

            foreach (var childTree in mixinTree.Children)
            {
                var childName = (string.IsNullOrEmpty(name) ? string.Empty : name + ".") + childTree.Value.Name;
                Compile(childName, childTree.Value, compilerParameters, compilerResults);
            }
        }

        /// <summary>
        /// Compiles the ShaderMixinSource into a platform bytecode.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <param name="log">The log.</param>
        /// <returns>The platform-dependent bytecode.</returns>
        public abstract EffectBytecode Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log);

        public static string GetEffectName(string fullEffectName, out string subEffect)
        {
            var mainEffectNameEnd = fullEffectName.IndexOf('.');
            var mainEffectName = mainEffectNameEnd != -1 ? fullEffectName.Substring(0, mainEffectNameEnd) : fullEffectName;

            subEffect = mainEffectNameEnd != -1 ? fullEffectName.Substring(mainEffectNameEnd + 1) : string.Empty;
            return mainEffectName;
        }

        public static readonly string DefaultSourceShaderFolder = "shaders";

        public static string GetStoragePathFromShaderType(string type)
        {
            if (type == null) throw new ArgumentNullException("type");
            // TODO: harcoded values, bad bad bad
            return DefaultSourceShaderFolder + "/" + type + ".pdxsl";
        }
    }
}