// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Interface for compiling a package.
    /// </summary>
    public interface IPackageCompiler
    {
        /// <summary>
        /// Prepares a package with the specified compiler context.
        /// </summary>
        /// <param name="compilerContext">The compiler context.</param>
        /// <returns>Result of compilation.</returns>
        AssetCompilerResult Prepare(AssetCompilerContext compilerContext);
    }
}
