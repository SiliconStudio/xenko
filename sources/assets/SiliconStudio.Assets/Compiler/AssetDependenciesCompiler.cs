// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

        public void Prepare(AssetCompilerResult finalResult, CompilerContext context, AssetItem assetItem, BuildStep parentBuildStep = null, BuildDependencyType dependencyType = BuildDependencyType.Runtime)
        {
            var assetNode = buildDependencyManager.FindOrCreateNode(assetItem, dependencyType);

            assetNode.Analyze();

            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType());
            if(mainCompiler == null) return;

            var mainResult = mainCompiler.Prepare(context, assetItem);
            if (mainResult.HasErrors)
            {
                finalResult.Error($"Failed to compile preview for asset {assetItem.Location}");
                return;
            }

            if(parentBuildStep != null)
                BuildStep.LinkBuildSteps(mainResult.BuildSteps, parentBuildStep);

            foreach (var dependencyNode in assetNode.DependencyNodes)
            {
                Prepare(finalResult, context, dependencyNode.AssetItem, mainResult.BuildSteps, dependencyNode.DependencyType);
                if (finalResult.HasErrors)
                {
                    return;
                }
            }

            assetNode.Version = assetItem.Version;
            assetNode.BuildTask = mainResult.BuildSteps.ExecutedAsync();

            if ((dependencyType & BuildDependencyType.CompileContent) == BuildDependencyType.CompileContent ||
                (dependencyType & BuildDependencyType.Runtime) == BuildDependencyType.Runtime)
            {
                finalResult.BuildSteps.Add(mainResult.BuildSteps);
            }
        }
    }
}
