// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
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
            yield return parameters.Clone();
        }
   }
}