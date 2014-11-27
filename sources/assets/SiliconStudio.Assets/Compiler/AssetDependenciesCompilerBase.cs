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

        /// <summary>
        /// The asset item session where all the <see cref="AssetItem"/> references can be found.
        /// </summary>
        protected PackageSession AssetsSession;

        public virtual AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (assetItem == null) throw new ArgumentNullException("assetItem");

            Asset = (T)assetItem.Asset;
            AssetItem = assetItem;
            // TODO: This can critically fail if the asset item has been removed from its package - find a way to prevent exceptions here and in usages of AssetsSession
            AssetsSession = AssetItem.Package.Session;

            var compilerResult = new AssetCompilerResult();

            CompileOverride((AssetCompilerContext)context, compilerResult);

            return compilerResult;
        }

        protected abstract AssetCompilerResult CompileOverride(AssetCompilerContext context, AssetCompilerResult compilationResult);

        /// <summary>
        /// Add to the current compilation result the compilation steps required to compile the <see cref="AssetItem"/> dependencies.
        /// </summary>
        /// <param name="context">A compiler context.</param>
        /// <param name="result">The current result of the compilation</param>
        protected void AddDependenciesBuildStepsToResult(AssetCompilerContext context, AssetCompilerResult result)
        {
            // create the fake package used to compile the dependences
            var dependenciesCompilePackage = AssetsSession.CreateCompilePackageFromAsset(AssetItem);

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
        }
    }
}