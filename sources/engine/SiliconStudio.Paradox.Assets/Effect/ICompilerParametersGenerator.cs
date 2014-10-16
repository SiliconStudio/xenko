// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// A dynamic generator of <see cref="CompilerParameters"/> from a source parameters. See remarks.
    /// </summary>
    /// <remarks>
    /// This interface can be implemented to generate permutation of <see cref="CompilerParameters"/> derived
    /// from base parameters. This is usefull when some parameters needs to be generated based
    /// on the condition of some values in the base parameter.
    /// </remarks>
    public interface ICompilerParametersGenerator
    {
        /// <summary>
        /// The priority of the generator. Higher priority generators will override lower ones.
        /// </summary>
        int GeneratorPriority { get; }

        /// <summary>
        /// Generates derived <see cref="CompilerParameters"/> from a base parameters. 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="baseParameters">The parameters.</param>
        /// <param name="log">The log.</param>
        /// <returns>An enumerable of derived <see cref="CompilerParameters"/>.</returns>
        IEnumerable<CompilerParameters> Generate(AssetCompilerContext context, CompilerParameters baseParameters, ILogger log);
    }
}