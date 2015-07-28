// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// An implementation of <see cref="IAssetCompiler"/> that will compile an asset with all its dependencies.
    /// </summary>
    /// <remarks>This class is stateless and can reused or be shared amongst multiple asset compilation</remarks>
    public class AssetDependenciesCompiler : IAssetCompiler
    {
        /// <inheritdoc/>
        public AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (assetItem == null) throw new ArgumentNullException("assetItem");

            assetItem = assetItem.Package.Session.DependencyManager.FindDependencySet(assetItem.Id).Item;

            var compilerResult = new AssetCompilerResult();

            if (assetItem.Package == null)
            {
                compilerResult.Warning("Asset [{0}] is not attached to a package", assetItem);
                return compilerResult;
            }

            CompileWithDependencies((AssetCompilerContext)context, assetItem, compilerResult);

            return compilerResult;
        }

        /// <summary>
        /// Compiles the given asset with its dependencies.
        /// </summary>
        /// <param name="context">The asset compiler context.</param>
        /// <param name="assetItem">The asset to compile with its dependencies.</param>
        /// <param name="compilationResult">The result of the compilation.</param>
        protected virtual void CompileWithDependencies(AssetCompilerContext context, AssetItem assetItem, AssetCompilerResult compilationResult)
        {
            AddDependenciesBuildStepsToResult(assetItem.Package.Session, assetItem, context, compilationResult);
        }

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