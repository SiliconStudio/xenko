using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Analysis
{
    public class BuildAssetNode
    {
        private readonly BuildDependencyManager buildDependencyManager;
        private readonly ConcurrentDictionary<AssetId, BuildAssetNode> dependencyLinks = new ConcurrentDictionary<AssetId, BuildAssetNode>();

        public readonly AssetItem AssetItem;

        private long version;

        /// <summary>
        /// Gets the asset version incremental counter, increased everytime the asset is edited.
        /// </summary>
        public long Version
        {
            get
            {
                return Interlocked.Read(ref version);
            }
            set
            {
                Interlocked.Exchange(ref version, value);
            }
        }

        public ICollection<BuildAssetNode> DependencyNodes => dependencyLinks.Values;

        public BuildDependencyType DependencyType { get; }

        public BuildAssetNode(AssetItem assetItem, BuildDependencyType type, BuildDependencyManager dependencyManager)
        {
            AssetItem = assetItem;
            DependencyType = type;
            buildDependencyManager = dependencyManager;
        }

        public void Analyze(AssetCompilerContext context)
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
                    foreach (var inputType in mainCompiler.GetInputTypes(context, assetDependency.Item).Where(x => x.Key == assetType))
                    {
                        var dependencyType = inputType.Value;
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