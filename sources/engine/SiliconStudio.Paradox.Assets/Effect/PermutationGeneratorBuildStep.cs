// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Paradox.Assets.Effect.Generators;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    public class PermutationGeneratorBuildStep : EnumerableBuildStep
    {
        protected readonly AssetCompilerContext Context;

        protected readonly List<ICompilerParametersGenerator> CompilerParametersGenerators;

        public PermutationGeneratorBuildStep(AssetCompilerContext context, List<ICompilerParametersGenerator> compilerParametersGenerators)
        {
            Context = context;
            CompilerParametersGenerators = new List<ICompilerParametersGenerator> { new DefaultCompilerParametersGenerator() };
            CompilerParametersGenerators.AddRange(compilerParametersGenerators);
        }

        /// <inheritdoc/>
        public override BuildStep Clone()
        {
            return new PermutationGeneratorBuildStep(Context, CompilerParametersGenerators);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "PermutationGeneratorBuildStep";
        }
        
        protected IEnumerable<CompilerParameters> GenerateKeysPermutation(CompilerParameters parameters, IList<KeyValuePair<ParameterKey, List<object>>> parameterKeys, int keyIndex = 0)
        {
            // TODO: Merge this code with the code found in DefaultCompilerParametersGenerator
            if (keyIndex >= parameterKeys.Count)
            {
                yield return parameters.Clone();
            }
            else
            {
                var keyValues = parameterKeys[keyIndex];
                // Duplicate new parameters collection only for the first level
                if (keyIndex == 0)
                {
                    parameters = parameters.Clone();
                }
                foreach (var value in keyValues.Value)
                {
                    parameters.SetObject(keyValues.Key, value);
                    foreach (var returnParameters in GenerateKeysPermutation(parameters, parameterKeys, keyIndex + 1))
                    {
                        yield return returnParameters;
                    }
                }
            }
        }
    }
}
