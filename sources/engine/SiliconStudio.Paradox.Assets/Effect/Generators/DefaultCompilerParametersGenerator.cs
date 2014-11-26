// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect.Generators
{
    /// <summary>
    /// The default implementation for <see cref="ICompilerParametersGenerator"/> simply copy a clone version of the input baseParameters. See remarks.
    /// </summary>
    /// <remarks>
    /// This generator is always registered and call first in the 
    /// </remarks>
    public class DefaultCompilerParametersGenerator : ICompilerParametersGenerator
    {
        public int GeneratorPriority
        {
            get
            {
                return 0;
            }
        }

        public IEnumerable<CompilerParameters> Generate(AssetCompilerContext context, CompilerParameters parameters, ILogger log)
        {
            var effectName = parameters.Get(EffectKeys.Name);
            if (effectName != null)
            {
                List<ParameterKey> keys = null;
                FindAllKeys(effectName, ref keys);

                if (keys != null)
                {
                    var cloneParameters = parameters.Clone();
                    foreach (var newParams in GeneratePermutations(keys, 0, cloneParameters))
                    {
                        yield return newParams;
                    }

                    yield break;
                }
            }

            yield return parameters.Clone();
        }

        private IEnumerable<CompilerParameters> GeneratePermutations(List<ParameterKey> keys, int fromIndex, CompilerParameters parameters)
        {
            var key = keys[fromIndex];
            var values = key.Metadatas.OfType<ParameterKeyPermutationsMetadata>().First().Values;
            foreach (var value in values)
            {
                parameters.SetObject(key, value);
                var nextKeyIndex = fromIndex + 1;
                if (nextKeyIndex < keys.Count)
                {
                    foreach (var subParams in GeneratePermutations(keys, nextKeyIndex, parameters))
                    {
                        yield return subParams;
                    }
                }
                else
                {
                    yield return parameters.Clone();
                }
            }
        }

        private void FindAllKeys(string effectName, ref List<ParameterKey> keys)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            IShaderMixinBuilder builder;
            if (ShaderMixinManager.TryGet(effectName, out builder))
            {
                FindAllKeys(builder, ref keys);
            }
        }

        private void FindAllKeys(IShaderMixinBuilder builder, ref List<ParameterKey> keys)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            var extendedBuilder = builder as IShaderMixinBuilderExtended;
            if (extendedBuilder == null)
            {
                return;
            }

            foreach (var mixin in extendedBuilder.Mixins)
            {
                FindAllKeys(mixin, ref keys);
            }

            foreach (var key in extendedBuilder.Keys.Where(key => key.Metadatas.OfType<ParameterKeyPermutationsMetadata>().Any()))
            {
                if (keys == null)
                {
                    keys = new List<ParameterKey>();
                }

                if (!keys.Contains(key))
                {
                    keys.Add(key);
                }
            }
        }
    }
}