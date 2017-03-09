using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Analysis
{
    public class BuildAssetNode
    {
        private readonly BuildDependencyManager buildDependencyManager;
        private readonly ConcurrentDictionary<AssetId, BuildAssetNode> dependencyLinks = new ConcurrentDictionary<AssetId, BuildAssetNode>();

        public readonly AssetItem AssetItem;

        public long Version { get; set; } = -1;

        public Task BuildTask { get; set; }

        public ICollection<BuildAssetNode> DependencyNodes => dependencyLinks.Values;

        public BuildDependencyType DependencyType { get; }

        public BuildAssetNode(AssetItem assetItem, BuildDependencyType type, BuildDependencyManager dependencyManager)
        {
            AssetItem = assetItem;
            DependencyType = type;
            buildDependencyManager = dependencyManager;
        }

        public void Analyze()
        {
            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(AssetItem.Asset.GetType());
            if (mainCompiler == null) return; //scripts and such don't have compiler

            dependencyLinks.Clear();

            //DependencyManager check
            var dependencies = AssetItem.Package.Session.DependencyManager.ComputeDependencies(AssetItem.Id, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive, ContentLinkType.Reference);
            if (dependencies != null)
            {
                foreach (var assetDependency in dependencies.LinksOut)
                {
                    var assetType = assetDependency.Item.Asset.GetType();
                    if (mainCompiler.CompileTimeDependencyTypes.ContainsKey(assetType))
                    {
                        var dependencyType = mainCompiler.CompileTimeDependencyTypes[assetType];
                        var node = buildDependencyManager.FindOrCreateNode(assetDependency.Item, dependencyType);
                        dependencyLinks.TryAdd(assetDependency.Item.Id, node);
                    }
                }
            }

            //Input files required
            foreach (var inputFile in mainCompiler.GetInputFiles(AssetItem))
            {
                if (inputFile.Type == UrlType.Content || inputFile.Type == UrlType.ContentLink)
                {
                    var asset = AssetItem.Package.Session.FindAsset(inputFile.Path); //this will search all packages
                    if (asset == null) continue; //this might be an error tho...

                    var dependencyType = inputFile.Type == UrlType.Content ? BuildDependencyType.CompileContent : BuildDependencyType.CompileAsset;
                    var node = buildDependencyManager.FindOrCreateNode(asset, dependencyType);
                    dependencyLinks.TryAdd(asset.Id, node);
                }
            }
        }
    }
}