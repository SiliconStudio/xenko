// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A base class for the compilers that need to recursively compile an asset's dependencies.
    /// </summary>
    /// <typeparam name="T">The type of asset that the builder build</typeparam>
    public abstract class AssetDependenciesCompilerBase<T> : IAssetCompiler where T : Asset
    {
        /// <summary>
        /// The item asset to compile
        /// </summary>
        protected AssetItem AssetItem;

        /// <summary>
        /// The typed asset associated to <see cref="AssetItem"/>
        /// </summary>
        protected T Asset;

        public virtual AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (assetItem == null) throw new ArgumentNullException("assetItem");

            Asset = (T)assetItem.Asset;
            AssetItem = assetItem;

            var compilerResult = new AssetCompilerResult();

            if (AssetItem.Package == null)
            {
                compilerResult.Warning("Asset [{0}] is not attached to a package", AssetItem);
                return compilerResult;
            }

            CompileOverride((AssetCompilerContext)context, compilerResult);

            return compilerResult;
        }

        protected abstract AssetCompilerResult CompileOverride(AssetCompilerContext context, AssetCompilerResult compilationResult);

        /// <summary>
        /// Clones the given asset and its dependencies, generates the build steps corresponding to all these assets and adds them to the given <see cref="AssetCompilerResult"/>.
        /// </summary>
        /// <param name="session">The session where the asset and its dependencies can be found.</param>
        /// <param name="assetItem">The asset to clone with its dependencies.</param>
        /// <param name="context">The compiler context.</param>
        /// <param name="result">The result of the compilation in which the generated build steps will be added.</param>
        /// <returns>The generated build steps.</returns>
        protected static BuildStep AddDependenciesBuildStepsToResult(PackageSession session, AssetItem assetItem, AssetCompilerContext context, AssetCompilerResult result)
        {
            // create the fake package used to compile the dependences
            var dependenciesCompilePackage = session.CreateCompilePackageFromAsset(assetItem);

            // compile the fake package (create the build steps)
            var assetPackageCompiler = new PackageCompiler();
            context.Package = dependenciesCompilePackage.LocalPackages.FirstOrDefault();
            var dependenciesCompileResult = assetPackageCompiler.Compile(context);

            // Create the result build steps if not existing yet
            if (result.BuildSteps == null)
                result.BuildSteps = new ListBuildStep();

            // Add the dependencies build steps to the current result
            result.BuildSteps.Add(dependenciesCompileResult.BuildSteps);

            // Copy log the dependencies result to the current result
            dependenciesCompileResult.CopyTo(result);

            return dependenciesCompileResult.BuildSteps;
        }
    }
}