// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// An implementation of <see cref="IAssetCompiler"/> that will compile an asset with all its dependencies.
    /// </summary>
    public class AssetDependenciesCompiler
    {
        private readonly BuildDependencyManager buildDependencyManager = new BuildDependencyManager();

        public AssetDependenciesCompiler(Type compilationContext)
        {
            if(!typeof(ICompilationContext).IsAssignableFrom(compilationContext))
                throw new InvalidOperationException($"{nameof(compilationContext)} should inherit from ICompilationContext");
            buildDependencyManager.CompilationContext = compilationContext;
        }

        public void Prepare(Dictionary<AssetId, BuildStep> addedBuildSteps, AssetCompilerResult finalResult, AssetCompilerContext context, List<AssetItem> assetItems, HashSet<Type> filterOutTypes, BuildStep parentBuildStep = null,
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            foreach (var assetItem in assetItems)
            {
                Prepare(addedBuildSteps, finalResult, context, assetItem, filterOutTypes, parentBuildStep, dependencyType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedBuildSteps">A local cache, created at the first prepare.</param>
        /// <param name="finalResult">Internal steps are added to this final result</param>
        /// <param name="context"></param>
        /// <param name="assetItem"></param>
        /// <param name="filterOutTypes"></param>
        /// <param name="parentBuildStep"></param>
        /// <param name="dependencyType"></param>
        public void Prepare(Dictionary<AssetId, BuildStep> addedBuildSteps, AssetCompilerResult finalResult, AssetCompilerContext context, AssetItem assetItem, HashSet<Type> filterOutTypes, BuildStep parentBuildStep = null, 
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            var assetNode = buildDependencyManager.FindOrCreateNode(assetItem, dependencyType);

            assetNode.Analyze(context, filterOutTypes);

            //We want to avoid repeating steps, so we use the local cache to check if this compile command already has the step required first
            BuildStep buildStep;
            var inCache = true;
            if (!addedBuildSteps.TryGetValue(assetItem.Id, out buildStep))
            {
                var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType(), buildDependencyManager.CompilationContext);
                if (mainCompiler == null) return;

                var mainResult = mainCompiler.Prepare(context, assetItem);
                if (mainResult.HasErrors)
                {
                    finalResult.Error($"Failed to compile preview for asset {assetItem.Location}");
                    return;
                }
                buildStep = mainResult.BuildSteps;
                addedBuildSteps.Add(assetItem.Id, buildStep);
                inCache = false;
            }

            //Go thru the dependencies of the node and prepare them as well
            foreach (var dependencyNode in assetNode.DependencyNodes)
            {
                if ((dependencyNode.DependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent || //only if content is required Content.Load
                    (dependencyNode.DependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime) //or the asset is required anyway at runtime
                {
                    Prepare(addedBuildSteps, finalResult, context, dependencyNode.AssetItem, filterOutTypes, buildStep, dependencyNode.DependencyType);
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
                    finalResult.BuildSteps.Add(buildStep);
                }

                //link
                if (parentBuildStep != null)
                    BuildStep.LinkBuildSteps(buildStep, parentBuildStep);
            }
        }
    }
}
