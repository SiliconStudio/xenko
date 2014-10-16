// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Assets.Effect.ValueGenerators;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    internal class LightingCompileGenerator : PermutationGeneratorBuildStep
    {
        protected readonly LightingAsset Asset;

        public LightingCompileGenerator(LightingAsset asset, AssetCompilerContext context)
            : base(context, EffectLibraryAssetCompiler.RegisteredCompilerParametersGenerators.ToList())
        {
            Asset = asset;
        }

        public override string Title
        {
            get
            {
                return "LightingCompileGenerator";
            }
        }

        public List<CompilerParameters> Execute()
        {
            var steps = new List<BuildStep>();
            Steps = steps;

            // Pre-generate CompilerParameters for all meshes
            var log = new LoggerResult("Lighting permutation of asset [{0}]".ToFormat(Asset.Id));

            var rootParameters = new CompilerParameters
                {
                    Platform = Context.GetGraphicsPlatform(),
                    Profile = Context.GetGraphicsProfile()
                };

            var keys = Asset.Permutations == null ? null : Asset.Permutations.Keys;
            var children = Asset.Permutations == null ? null : Asset.Permutations.Children;

            var result = new List<CompilerParameters>();

            foreach (var parametersPerPermutation in GeneratePermutation(rootParameters, keys, children, log))
            {
                var parameters = parametersPerPermutation.Clone();
                result.Add(parameters);
            }
            return result;
        }

        private IEnumerable<CompilerParameters> GeneratePermutation(CompilerParameters parameters, EffectParameterKeyStandardGenerator keys,  List<EffectPermutation> permutations, ILogger log)
        {
            if (keys == null || keys.Count == 0)
            {
                if (permutations == null || permutations.Count == 0)
                {
                    yield return parameters;
                }
                else
                {
                    for (int permutationIndex = 0; permutationIndex < permutations.Count; permutationIndex++)
                    {
                        var permutation = permutations[permutationIndex];
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
    }
}
