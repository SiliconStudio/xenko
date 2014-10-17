// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Entry point to compile an <see cref="EffectLibraryAsset"/>
    /// </summary>
    public class EffectLibraryAssetCompiler : AssetCompilerBase<EffectLibraryAsset>
    {
        private static readonly List<ICompilerParametersGenerator> registeredCompilerParametersGenerators = new List<ICompilerParametersGenerator>();

        /// <summary>
        /// Gets the registered <see cref="CompilerParameters"/> generators.
        /// </summary>
        /// <value>The registered <see cref="CompilerParameters"/> generators.</value>
        public static IEnumerable<ICompilerParametersGenerator> RegisteredCompilerParametersGenerators
        {
            get
            {
                lock (registeredCompilerParametersGenerators)
                {
                    return registeredCompilerParametersGenerators.ToList();
                }
            }
        }

        /// <summary>
        /// Registers a <see cref="CompilerParameters"/> generator.
        /// </summary>
        /// <param name="generator">The generator.</param>
        public static void RegisterCompilerParametersGenerator(ICompilerParametersGenerator generator)
        {
            lock (registeredCompilerParametersGenerators)
            {
                if (!registeredCompilerParametersGenerators.Contains(generator))
                {
                    var insertIndex = registeredCompilerParametersGenerators.FindIndex(x => x.GeneratorPriority > generator.GeneratorPriority);
                    if (insertIndex == -1)
                        registeredCompilerParametersGenerators.Add(generator);
                    else
                        registeredCompilerParametersGenerators.Insert(insertIndex, generator);
                }
            }
        }

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EffectLibraryAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true; // force to wait on all assets before starting EffectCompilerGeneratorBuildStep
            result.BuildSteps = new ListBuildStep { new EffectCompileGeneratorBuildStep(context, urlInStorage, RegisteredCompilerParametersGenerators.ToList(), asset) };
        }
    }
}