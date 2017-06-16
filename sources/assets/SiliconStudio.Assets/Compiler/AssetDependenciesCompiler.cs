// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.BuildEngine;

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

            BuildDependencyManager = new BuildDependencyManager(compilationContext);
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
            var addedBuildSteps = new Dictionary<AssetId, AssetCompilerResult>(); // a cache of build steps in order to link and reuse
            foreach (var assetItem in assetItems)
            {
                var visitedItems = new HashSet<BuildAssetNode>();
                var typesToInclude = new HashSet<KeyValuePair<Type, BuildDependencyType>>();
                Prepare(addedBuildSteps, finalResult, context, assetItem, null, typesToInclude, visitedItems);
            }
            return finalResult;
        }

        /// <summary>
        /// Prepare a single asset to be built
        /// </summary>
        /// <param name="context">The AssetCompilerContext</param>
        /// <param name="assetItem">The asset to build</param>
        /// <param name="allowDependencyExclusion">If the process should allow asset compilers to remove unused dependency types to speed up the process</param>
        /// <returns></returns>
        public AssetCompilerResult Prepare(AssetCompilerContext context, AssetItem assetItem, bool allowDependencyExclusion = true)
        {
            var finalResult = new AssetCompilerResult();
            var addedBuildSteps = new Dictionary<AssetId, AssetCompilerResult>(); // a cache of build steps in order to link and reuse
            var filters = allowDependencyExclusion ? new HashSet<Type>() : null; //the types to filter out, this is incremental between prepares
            var typesToInclude = new HashSet<KeyValuePair<Type, BuildDependencyType>>();
            var visitedItems = new HashSet<BuildAssetNode>();
            Prepare(addedBuildSteps, finalResult, context, assetItem, filters, typesToInclude, visitedItems);
            return finalResult;
        }

        private void Prepare(Dictionary<AssetId, AssetCompilerResult> resultsCache, AssetCompilerResult finalResult, AssetCompilerContext context, AssetItem assetItem, HashSet<Type> filterOutTypes, HashSet<KeyValuePair<Type, BuildDependencyType>> includeTypes, HashSet<BuildAssetNode> visitedItems, BuildStep parentBuildStep = null, 
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            var assetNode = BuildDependencyManager.FindOrCreateNode(assetItem, dependencyType);

            // Prevent re-entrency in the same node
            if (visitedItems.Contains(assetNode))
                return;

            try
            {
                visitedItems.Add(assetNode);

                assetNode.Analyze(context, includeTypes, filterOutTypes);

                //We want to avoid repeating steps, so we use the local cache to check if this compile command already has the step required first
                AssetCompilerResult cachedResult;
                var inCache = true;
                if (!resultsCache.TryGetValue(assetItem.Id, out cachedResult))
                {
                    var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType(), BuildDependencyManager.CompilationContext);
                    if (mainCompiler == null) return;

                    cachedResult = mainCompiler.Prepare(context, assetItem);
                    if (((dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) && cachedResult.HasErrors) //allow Runtime dependencies to fail
                    {
                        //totally skip this asset but do not propagate errors!
                        return;
                    }

                    cachedResult.CopyTo(finalResult);
                    if (cachedResult.HasErrors)
                    {
                        finalResult.Error($"Failed to prepare asset {assetItem.Location}");
                        return;
                    }
                    resultsCache.Add(assetItem.Id, cachedResult);
                    inCache = false;
                    AssetCompiled?.Invoke(this, new AssetCompiledArgs(assetItem, cachedResult));
                }

                //Go thru the dependencies of the node and prepare them as well
                foreach (var dependencyNode in assetNode.References)
                {
                    if ((dependencyNode.DependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent || //only if content is required Content.Load
                        (dependencyNode.DependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) //or the asset is required anyway at runtime
                    {
                        Prepare(resultsCache, finalResult, context, dependencyNode.AssetItem, filterOutTypes, includeTypes, visitedItems, cachedResult.BuildSteps, dependencyNode.DependencyType);
                        if (finalResult.HasErrors)
                        {
                            return;
                        }
                    }
                }

                //Finally link the steps together, this uses low level build engine primitive and routines to make sure dependencies are compiled
                if (((dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent) || //only if content is required Content.Load
                    (dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) //or the asset is required anyway at runtime
                {
                    if (!inCache) //skip adding again the step if it was already in the final step
                        finalResult.BuildSteps.Add(cachedResult.BuildSteps);

                    //link
                    if (parentBuildStep != null && (dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent) //only if content is required Content.Load
                        BuildStep.LinkBuildSteps(cachedResult.BuildSteps, parentBuildStep);
                }
            }
            finally
            {
                visitedItems.Remove(assetNode);
            }
        }
    }
}
