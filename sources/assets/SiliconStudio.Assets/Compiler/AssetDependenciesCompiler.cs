// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// An asset compiler that will compile an asset with all its dependencies.
    /// </summary>
    public class AssetDependenciesCompiler
    {
        public readonly BuildDependencyManager BuildDependencyManager;

        /// <summary>
        /// Raised when a single asset has been compiled.
        /// </summary>
        public EventHandler<AssetCompiledArgs> AssetCompiled;

        public AssetDependenciesCompiler(Type compilationContext)
        {
            if(!typeof(ICompilationContext).IsAssignableFrom(compilationContext))
                throw new InvalidOperationException($"{nameof(compilationContext)} should inherit from ICompilationContext");

            BuildDependencyManager = new BuildDependencyManager();
        }

        /// <summary>
        /// Prepare the list of assets to be built, building all the steps and linking them properly
        /// </summary>
        /// <param name="context">The AssetCompilerContext</param>
        /// <param name="assetItems">The assets to prepare for build</param>
        /// <returns></returns>
        public AssetCompilerResult PrepareMany(AssetCompilerContext context, List<AssetItem> assetItems)
        {
            var finalResult = new AssetCompilerResult();
            var compiledItems = new HashSet<AssetId>();
            foreach (var assetItem in assetItems)
            {
                var visitedItems = new HashSet<BuildAssetNode>();
                Prepare(finalResult, context, assetItem, context.CompilationContext, visitedItems, compiledItems);
            }
            return finalResult;
        }

        /// <summary>
        /// Prepare a single asset to be built
        /// </summary>
        /// <param name="context">The AssetCompilerContext</param>
        /// <param name="assetItem">The asset to build</param>
        /// <returns></returns>
        public AssetCompilerResult Prepare(AssetCompilerContext context, AssetItem assetItem)
        {
            var finalResult = new AssetCompilerResult();
            var visitedItems = new HashSet<BuildAssetNode>();
            var compiledItems = new HashSet<AssetId>();
            Prepare(finalResult, context, assetItem, context.CompilationContext, visitedItems, compiledItems);
            return finalResult;
        }

        private void Prepare(AssetCompilerResult finalResult, AssetCompilerContext context, AssetItem assetItem, [NotNull] Type compilationContext, HashSet<BuildAssetNode> visitedItems, HashSet<AssetId> compiledItems, BuildStep parentBuildStep = null, 
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            if (compilationContext == null) throw new ArgumentNullException(nameof(compilationContext));
            var assetNode = BuildDependencyManager.FindOrCreateNode(assetItem, compilationContext);

            // Prevent re-entrancy in the same node
            if (visitedItems.Add(assetNode))
            {
                assetNode.Analyze(context);

                // Invoke the compiler to prepare the build step for this asset if the dependency needs to compile it (Runtime or CompileContent)
                if ((dependencyType & ~BuildDependencyType.CompileAsset) != 0 && compiledItems.Add(assetNode.AssetItem.Id))
                {
                    var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType(), assetNode.CompilationContext);
                    if (mainCompiler == null)
                        return;

                    var compilerResult = mainCompiler.Prepare(context, assetItem);

                    if ((dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime && compilerResult.HasErrors) //allow Runtime dependencies to fail
                    {
                        //totally skip this asset but do not propagate errors!
                        return;
                    }

                    assetNode.BuildSteps = compilerResult.BuildSteps;

                    // Copy the log to the final result (note: this does not copy or forward the build steps)
                    compilerResult.CopyTo(finalResult);
                    if (compilerResult.HasErrors)
                    {
                        finalResult.Error($"Failed to prepare asset {assetItem.Location}");
                        return;
                    }

                    // Add the resulting build steps to the final
                    finalResult.BuildSteps.Add(assetNode.BuildSteps);

                    AssetCompiled?.Invoke(this, new AssetCompiledArgs(assetItem, compilerResult));
                }

                // Go through the dependencies of the node and prepare them as well
                foreach (var reference in assetNode.References)
                {
                    var target = reference.Target;
                    Prepare(finalResult, context, target.AssetItem, target.CompilationContext, visitedItems, compiledItems, assetNode.BuildSteps, reference.DependencyType);
                    if (finalResult.HasErrors)
                    {
                        return;
                    }
                }

                // If we didn't prepare any build step for this asset let's exit here.
                if (assetNode.BuildSteps == null)
                    return;
            }

            // Link the created build steps to their parent step.
            if (parentBuildStep != null && assetNode.BuildSteps != null && (dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent) //only if content is required Content.Load
                BuildStep.LinkBuildSteps(assetNode.BuildSteps, parentBuildStep);
        }
    }
}
