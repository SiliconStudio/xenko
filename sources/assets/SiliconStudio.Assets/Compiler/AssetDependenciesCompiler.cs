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

        public AssetCompilerResult Prepare(CompilerContext context, AssetItem assetItem)
        {
            var finalResult = new AssetCompilerResult();

            var assetNode = buildDependencyManager.FindOrCreateNode(assetItem, BuildDependencyType.Runtime);
            assetNode.Analyze();

            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(assetItem.Asset.GetType());
            if(mainCompiler == null) return finalResult;

            foreach (var dependencyNode in assetNode.DependencyNodes)
            {
                var result = Compile(context, dependencyNode.AssetItem);
                if (result.HasErrors)
                {
                    finalResult.Error($"Failed to compile preview for asset {assetItem.Location}");
                    return finalResult;
                }
                finalResult.BuildSteps.Add(result.BuildSteps);
                finalResult.BuildSteps.Add(new WaitBuildStep()); //todo use LINK
            }

            var mainResult = mainCompiler.Prepare(context, assetItem);
            if (mainResult.HasErrors)
            {
                finalResult.Error($"Failed to compile preview for asset {assetItem.Location}");
                return finalResult;
            }

            finalResult.BuildSteps.Add(mainResult.BuildSteps);
            return finalResult;
        }
    }
}
