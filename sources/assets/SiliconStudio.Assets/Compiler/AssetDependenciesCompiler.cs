// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;

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

            var compilerResult = new AssetCompilerResult();

            if (assetItem.Package == null)
            {
                compilerResult.Warning($"Asset [{assetItem}] is not attached to a package");
                return compilerResult;
            }

            var assetCompilerContext = (AssetCompilerContext)context;

            // create the a package that contains only the asset and its the dependencies
            var dependenciesCompilePackage = assetItem.Package.Session.CreateCompilePackageFromAsset(assetItem);
            var clonedAsset = dependenciesCompilePackage.FindAsset(assetItem.Id);

            CompileWithDependencies(assetCompilerContext, clonedAsset, assetItem, compilerResult);

            // Check unloadable items
            foreach (var currentAssetItem in dependenciesCompilePackage.Assets)
            {
                var unloadableItems = UnloadableObjectRemover.Run(currentAssetItem.Asset);
                foreach (var unloadableItem in unloadableItems)
                {
                    compilerResult.Log(new AssetLogMessage(dependenciesCompilePackage, currentAssetItem.ToReference(), LogMessageType.Warning, $"Unable to load the object of type {unloadableItem.UnloadableObject.TypeName} which is located at [{unloadableItem.MemberPath}] in the asset"));
                }
            }

            // Find AssetBuildStep
            var assetBuildSteps = new Dictionary<AssetId, AssetBuildStep>();
            foreach (var step in compilerResult.BuildSteps.EnumerateRecursively())
            {
                var assetStep = step as AssetBuildStep;
                if (assetStep != null)
                {
                    assetBuildSteps[assetStep.AssetItem.Id] = assetStep;
                }
            }

            // TODO: Refactor logging of CompilerApp and BuildEngine
            // Copy log top-level to proper asset build steps
            foreach (var message in compilerResult.Messages)
            {
                var assetMessage = message as AssetLogMessage;

                // Find asset (if nothing found, default to main asset)
                var assetId = assetMessage?.AssetReference.Id ?? assetItem.Id;
                AssetBuildStep assetBuildStep;
                if (assetBuildSteps.TryGetValue(assetId, out assetBuildStep))
                {
                    // Log to AssetBuildStep
                    assetBuildStep.Logger?.Log(message);
                }
            }

            return compilerResult;
        }

        /// <summary>
        /// Compiles the given asset with its dependencies.
        /// </summary>
        /// <param name="context">The asset compiler context.</param>
        /// <param name="assetItem">The asset to compile with its dependencies.</param>
        /// <param name="originalItem"></param>
        /// <param name="compilationResult">The result of the compilation.</param>
        protected virtual void CompileWithDependencies(AssetCompilerContext context, AssetItem assetItem, AssetItem originalItem, AssetCompilerResult compilationResult)
        {
            CompilePackage(context, assetItem.Package, compilationResult);
        }

        /// <summary>
        /// Compiles the package contained in the given context and add the resulting build steps in the <see cref="AssetCompilerResult"/>
        /// </summary>
        /// <param name="context">The context which contains the package to compile.</param>
        /// <param name="result">The <see cref="AssetCompilerResult"/> where the build steps will be added.</param>
        /// <returns></returns>
        protected static BuildStep CompilePackage(AssetCompilerContext context, Package package, AssetCompilerResult result)
        {
            // compile the fake package (create the build steps)
            var assetPackageCompiler = new PackageCompiler(new PackageAssetEnumerator(package));
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
