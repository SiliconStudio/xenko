// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

        public void Prepare(Dictionary<AssetId, BuildStep> addedBuildSteps, AssetCompilerResult finalResult, AssetCompilerContext context, List<AssetItem> assetItems, BuildStep parentBuildStep = null,
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            foreach (var assetItem in assetItems)
            {
                Prepare(addedBuildSteps, finalResult, context, assetItem, parentBuildStep, dependencyType);
            }
        }

        public void Prepare(Dictionary<AssetId, BuildStep> addedBuildSteps, AssetCompilerResult finalResult, AssetCompilerContext context, AssetItem assetItem, BuildStep parentBuildStep = null, 
            BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            var assetNode = buildDependencyManager.FindOrCreateNode(assetItem, dependencyType);

            assetNode.Analyze(context);

            BuildStep buildStep;
            var inCache = true;
            if (!addedBuildSteps.TryGetValue(assetItem.Id, out buildStep))
            {
                var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType());
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

            foreach (var dependencyNode in assetNode.DependencyNodes)
            {
                Prepare(addedBuildSteps, finalResult, context, dependencyNode.AssetItem, buildStep, dependencyNode.DependencyType);
                if (finalResult.HasErrors)
                {
                    return;
                }
            }

            assetNode.Version = assetItem.Version;

            if ((dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent ||
                (dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime)
            {
                if (!inCache)
                {
                    finalResult.BuildSteps.Add(buildStep);
                }

                if (parentBuildStep != null)
                    BuildStep.LinkBuildSteps(buildStep, parentBuildStep);
            }
        }
    }
}
