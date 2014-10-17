// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Assets.Effect.ValueGenerators;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// This build steps is responsible to generate all compiler commmands for all effect permutations.
    /// </summary>
    internal class EffectCompileGeneratorBuildStep : PermutationGeneratorBuildStep
    {
        private readonly string url;

        protected readonly EffectLibraryAsset Asset;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectCompileGeneratorBuildStep" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="url">The URL.</param>
        /// <param name="compilerParametersGenerators">The compiler parameters generators.</param>
        /// <param name="asset">The asset.</param>
        public EffectCompileGeneratorBuildStep(AssetCompilerContext context, string url, List<ICompilerParametersGenerator> compilerParametersGenerators, EffectLibraryAsset asset)
            : base(context, compilerParametersGenerators)
        {
            this.url = url;
            this.Asset = asset;
        }

        public override string Title
        {
            get
            {
                return "EffectCompileGenerator";
            }
        }

        public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            var steps = new List<BuildStep>();
            Steps = steps;

            // Pre-generate CompilerParameters for all meshes
            var log = new LoggerResult("EffectLibrary [{0}]".ToFormat(url));

            var urlRoot = new UFile(url).GetParent();
            var rootParameters = new CompilerParameters
                {
                    Platform = Context.GetGraphicsPlatform(),
                    Profile = Context.GetGraphicsProfile()
                };

            var keys = Asset.Permutations == null ? null : Asset.Permutations.Keys;
            var children = Asset.Permutations == null ? null : Asset.Permutations.Children;

            foreach (var parametersPerPermutation in GeneratePermutation(rootParameters, keys, children, log))
            {
                var parameters = parametersPerPermutation.Clone();

                if (!parameters.ContainsKey(EffectKeys.Name))
                {
                    log.Warning("Permutation not compiled. It doesn't contain [Effect.Name] key to select the correct pdxfx/pdxsl to compile with parameters [{0}]", parametersPerPermutation.ToStringDetailed());
                }
                else
                {
                    var effectName = parameters.Get(EffectKeys.Name);
                    steps.Add(new CommandBuildStep(new EffectCompileCommand(Context, urlRoot, effectName, parameters)));
                }
            }

            // Copy all logs
            log.CopyTo(executeContext.Logger);

            return base.Execute(executeContext, builderContext);
        }

        private IEnumerable<CompilerParameters> GeneratePermutation(CompilerParameters parameters, EffectParameterKeyStandardGenerator keys, List<EffectPermutation> permutations, ILogger log)
        {
            if (keys == null || keys.Count == 0)
            {
                if (permutations == null || permutations.Count == 0)
                {
                    foreach (var newParameters in GenerateCompilerParametersPermutation(parameters, 0, log))
                    {
                        yield return newParameters;
                    }
                }
                else
                {
                    foreach (var permutation in permutations)
                    {
                        foreach (var parametersPerPermutations in GeneratePermutation(parameters, permutation.Keys, permutation.Children, log))
                        {
                            yield return parametersPerPermutations;
                        }
                    }
                }
            }
            else
            {
                foreach (var parametersPerKeyValuePermutations in GenerateKeysPermutation(parameters, keys.GenerateKeyValues()))
                {
                    foreach (var subParameters in GeneratePermutation(parametersPerKeyValuePermutations, null, permutations, log))
                    {
                        yield return subParameters;
                    }
                }
            }
        }

        private IEnumerable<CompilerParameters> GenerateCompilerParametersPermutation(CompilerParameters parameters, int compilerGeneratorIndex, ILogger log)
        {
            if (compilerGeneratorIndex >= CompilerParametersGenerators.Count)
            {
                // Clone for each version of CompilerParameters
                yield return parameters;
            }
            else
            {
                var generator = CompilerParametersGenerators[compilerGeneratorIndex];
                foreach (var nextParameters in generator.Generate(Context, parameters, log))
                {
                    foreach (var nextSubParameters in GenerateCompilerParametersPermutation(nextParameters, compilerGeneratorIndex + 1, log))
                    {
                        yield return nextSubParameters;
                    }
                }
            }
        }
    }
}