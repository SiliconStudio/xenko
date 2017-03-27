// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        private readonly BuildDependencyManager buildDependencyManager;

        /// <summary>
        /// Raised when a single asset has been compiled.
        /// </summary>
        public EventHandler<AssetCompiledArgs> AssetCompiled;

        public AssetDependenciesCompiler(Type compilationContext)
        {
            if(!typeof(ICompilationContext).IsAssignableFrom(compilationContext))
                throw new InvalidOperationException($"{nameof(compilationContext)} should inherit from ICompilationContext");

            buildDependencyManager = new BuildDependencyManager(compilationContext);
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
                Prepare(addedBuildSteps, finalResult, context, assetItem, null);
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
            Prepare(addedBuildSteps, finalResult, context, assetItem, filters);
            return finalResult;
        }

        private void Prepare(Dictionary<AssetId, AssetCompilerResult> resultsCache, AssetCompilerResult finalResult, AssetCompilerContext context, AssetItem assetItem, HashSet<Type> filterOutTypes, BuildStep parentBuildStep = null, 
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            var assetNode = buildDependencyManager.FindOrCreateNode(assetItem, dependencyType);

            assetNode.Analyze(context, filterOutTypes);

            //We want to avoid repeating steps, so we use the local cache to check if this compile command already has the step required first
            AssetCompilerResult cachedResult;
            var inCache = true;
            if (!resultsCache.TryGetValue(assetItem.Id, out cachedResult))
            {
                var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType(), buildDependencyManager.CompilationContext);
                if (mainCompiler == null) return;

                cachedResult = mainCompiler.Prepare(context, assetItem);
                if (cachedResult.HasErrors)
                {
                    finalResult.Error($"Failed to compile preview for asset {assetItem.Location}");
                    return;
                }
                resultsCache.Add(assetItem.Id, cachedResult);
                inCache = false;
                AssetCompiled?.Invoke(this, new AssetCompiledArgs(assetItem, cachedResult));
            }

            //Go thru the dependencies of the node and prepare them as well
            foreach (var dependencyNode in assetNode.DependencyNodes)
            {
                if ((dependencyNode.DependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent || //only if content is required Content.Load
                    (dependencyNode.DependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) //or the asset is required anyway at runtime
                {
                    Prepare(resultsCache, finalResult, context, dependencyNode.AssetItem, filterOutTypes, cachedResult.BuildSteps, dependencyNode.DependencyType);
                    if (finalResult.HasErrors)
                    {
                        return;
                    }
                }
            }

            //Finally link the steps together, this uses low level build engine primitive and routines to make sure dependencies are compiled
            if ((dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent || //only if content is required Content.Load
                (dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) //or the asset is required anyway at runtime
            {
                if (!inCache)  //skip adding again the step if it was already in the final step
                {
                    finalResult.BuildSteps.Add(cachedResult.BuildSteps);
                }

                //link
                if (parentBuildStep != null)
                    BuildStep.LinkBuildSteps(cachedResult.BuildSteps, parentBuildStep);
            }
        }
    }
}
