// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;

using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The base class to compile a series of <see cref="AssetItem"/>s using associated <see cref="IAssetCompiler"/>s.
    /// An item list compiler only creates the build steps required to creates some output items.
    /// The result of a compilation has then to be executed by the build engine to effectively create the outputs items.
    /// </summary>
    public abstract class ItemListCompiler
    {
        private readonly CompilerRegistry<IAssetCompiler> compilerRegistry;
        private int latestPriority;

        /// <summary>
        /// Raised when a single asset has been compiled.
        /// </summary>
        public EventHandler<AssetCompiledArgs> AssetCompiled;

        /// <summary>
        /// Create an instance of <see cref="ItemListCompiler"/> using the provided compiler registry.
        /// </summary>
        /// <param name="compilerRegistry">The registry that contains the compiler to use for each asset type</param>
        protected ItemListCompiler(CompilerRegistry<IAssetCompiler> compilerRegistry)
        {
            this.compilerRegistry = compilerRegistry;
        }

        /// <summary>
        /// Compile the required build steps necessary to produce the desired outputs items.
        /// </summary>
        /// <param name="context">The context source.</param>
        /// <param name="assetItems">The list of items to compile</param>
        /// <param name="compilationResult">The current compilation result, containing the build steps and the logging</param>
        protected void Compile(CompilerContext context, IEnumerable<AssetItem> assetItems,
            AssetCompilerResult compilationResult)
        {
            foreach (var assetItem in assetItems)
            {
                var itemBuildStep = CompileItem(context, compilationResult, assetItem);
                if (itemBuildStep != null)
                    compilationResult.BuildSteps.Add(itemBuildStep);
            }
        }

        /// <summary>
        /// Compile the required build step necessary to produce the desired output item.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="compilationResult">The compilation result.</param>
        /// <param name="assetItem">The asset item.</param>
        protected BuildStep CompileItem(CompilerContext context, AssetCompilerResult compilationResult, AssetItem assetItem)
        {
            // First try to find an asset compiler for this particular asset.
            IAssetCompiler compiler;
            try
            {
                compiler = compilerRegistry.GetCompiler(assetItem.Asset.GetType());
            }
            catch (Exception ex)
            {
                compilationResult.Error("Cannot find a compiler for asset [{0}] from path [{1}]", ex, assetItem.Id,
                    assetItem.Location);
                return null;
            }

            if (compiler == null)
            {
                return null;
            }

            // Second we are compiling the asset (generating a build step)
            try
            {
                var resultPerAssetType = compiler.Compile(context, assetItem);

                // Raise the AssetCompiled event.
                var handler = AssetCompiled;
                if (handler != null)
                    handler(this, new AssetCompiledArgs(assetItem, resultPerAssetType));

                resultPerAssetType.CopyTo(compilationResult);

                if (resultPerAssetType.BuildSteps == null)
                    return null;

                // Build the module string
                var assetAbsolutePath = assetItem.FullPath;
                assetAbsolutePath = Path.GetFullPath(assetAbsolutePath);
                var module = string.Format("{0}(1,1)", assetAbsolutePath);

                // Assign module string to all command build steps
                SetModule(resultPerAssetType.BuildSteps, module);

                // Add a wait command to the build steps if required by the item build
                if (resultPerAssetType.ShouldWaitForPreviousBuilds)
                    compilationResult.BuildSteps.Add(new WaitBuildStep());

                foreach (var buildStep in resultPerAssetType.BuildSteps)
                {
                    buildStep.Priority = latestPriority++;
                }

                // Add the item result build steps the item list result build steps 
                return resultPerAssetType.BuildSteps;
            }
            catch (Exception ex)
            {
                compilationResult.Error("Unexpected exception while compiling asset [{0}] from path [{1}]", ex, assetItem.Id,
                    assetItem.Location);
                return null;
            }
        }

        /// <summary>
        /// Sets recursively the <see cref="BuildStep.Module"/>.
        /// </summary>
        /// <param name="buildStep">The build step.</param>
        /// <param name="module">The module.</param>
        private void SetModule(BuildStep buildStep, string module)
        {
            if (buildStep.Module == null)
                buildStep.Module = module;

            var enumerableBuildStep = buildStep as EnumerableBuildStep;
            if (enumerableBuildStep != null && enumerableBuildStep.Steps != null)
            {
                foreach (var child in enumerableBuildStep.Steps)
                {
                    SetModule(child, module);
                }
            }
        }
    }
}
