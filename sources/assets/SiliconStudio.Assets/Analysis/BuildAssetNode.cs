// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
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
        private long version = -1;

        public AssetItem AssetItem { get; }

        public BuildDependencyType DependencyType { get; }

        public ICollection<BuildAssetNode> DependencyNodes => dependencyLinks.Values;

        public BuildAssetNode(AssetItem assetItem, BuildDependencyType type, BuildDependencyManager dependencyManager)
        {
            AssetItem = assetItem;
            DependencyType = type;
            buildDependencyManager = dependencyManager;
        }

        /// <summary>
        /// Performs analysis on the asset to figure out all the needed dependencies
        /// </summary>
        /// <param name="context">The compiler context</param>
        /// <param name="typesToFilterOut">The types to not mark as dependency</param>
        public void Analyze(AssetCompilerContext context, HashSet<Type> typesToFilterOut = null)
        {
            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(AssetItem.Asset.GetType(), buildDependencyManager.CompilationContext);
            if (mainCompiler == null) return; //scripts and such don't have compiler

            if (typesToFilterOut != null)
            {
                foreach (var type in mainCompiler.GetInputTypesToExclude(context, AssetItem))
                {
                    typesToFilterOut.Add(type);
                }
            }

            var assetVersion = AssetItem.Version;
            if (Interlocked.Exchange(ref version, assetVersion) == assetVersion) return; //same version, skip analysis, do not clear links

            //rebuild the dependency links, we clean first
            dependencyLinks.Clear();

            //DependencyManager check
            //for now we use the dependency manager itself to resolve runtime dependencies, in the future we might want to unify the builddependency manager with the dependency manager
            var dependencies = AssetItem.Package.Session.DependencyManager.ComputeDependencies(AssetItem.Id, AssetDependencySearchOptions.Out, ContentLinkType.Reference);
            if (dependencies != null)
            {
                foreach (var assetDependency in dependencies.LinksOut)
                {
                    var assetType = assetDependency.Item.Asset.GetType();
                    if (typesToFilterOut == null || !typesToFilterOut.Contains(assetType)) //filter out what we do not need
                    {
                        foreach (var inputType in mainCompiler.GetInputTypes(context, assetDependency.Item).Where(x => x.Key == assetType)) //resolve by type since dependency manager will provide us the assets needed
                        {
                            var dependencyType = inputType.Value;
                            var node = buildDependencyManager.FindOrCreateNode(assetDependency.Item, dependencyType);
                            dependencyLinks.TryAdd(assetDependency.Item.Id, node);
                        }
                    }
                }
            }

            //Input files required
            foreach (var inputFile in mainCompiler.GetInputFiles(context, AssetItem)) //directly resolve by input files, in the future we might just want this pass
            {
                if (inputFile.Type == UrlType.Content || inputFile.Type == UrlType.ContentLink)
                {
                    var asset = AssetItem.Package.Session.FindAsset(inputFile.Path); //this will search all packages
                    if (asset == null) continue; //this might be an error tho... but in the end compilation might fail so we let the build engine do the error reporting if it really was a issue
                    if (typesToFilterOut == null || !typesToFilterOut.Contains(asset.GetType()))
                    {
                        var dependencyType = inputFile.Type == UrlType.Content ? BuildDependencyType.CompileContent : BuildDependencyType.CompileAsset; //Content means we need to load the content, the rest is just asset dependency
                        var node = buildDependencyManager.FindOrCreateNode(asset, dependencyType);
                        dependencyLinks.TryAdd(asset.Id, node);
                    }
                }
            }
        }
    }
}
