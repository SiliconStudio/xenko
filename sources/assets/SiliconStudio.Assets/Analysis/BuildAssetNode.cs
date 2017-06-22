// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Analysis
{
    public class BuildAssetNode
    {
        public static PropertyKey<bool> VisitRuntimeTypes = new PropertyKey<bool>("VisitRuntimeTypes", typeof(BuildAssetNode));

        private readonly BuildDependencyManager buildDependencyManager;
        private readonly ConcurrentDictionary<AssetId, BuildAssetNode> references = new ConcurrentDictionary<AssetId, BuildAssetNode>();
        private readonly ConcurrentDictionary<BuildAssetNode, AssetId> referencedBy = new ConcurrentDictionary<BuildAssetNode, AssetId>();
        private long version = -1;

        public AssetItem AssetItem { get; }

        public BuildDependencyType DependencyType { get; }

        public ICollection<BuildAssetNode> References => references.Values;

        public ICollection<BuildAssetNode> ReferencedBy => referencedBy.Keys;

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
        public void Analyze(AssetCompilerContext context)
        {
            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(AssetItem.Asset.GetType(), buildDependencyManager.CompilationContext);
            if (mainCompiler == null)
                return; //scripts and such don't have compiler

            var assetVersion = AssetItem.Version;
            if (Interlocked.Exchange(ref version, assetVersion) == assetVersion)
                return; //same version, skip analysis, do not clear links

            var typesToInclude = new HashSet<KeyValuePair<Type, BuildDependencyType>>(mainCompiler.GetInputTypes(AssetItem));
            var typesToExclude = new HashSet<Type>(mainCompiler.GetInputTypesToExclude(AssetItem));

            //rebuild the dependency links, we clean first
            //remove self from current childs
            foreach (var buildAssetNode in references)
            {
                referencedBy.TryRemove(buildAssetNode.Value, out AssetId _);
            }
            //clean up our references
            references.Clear();

            //DependencyManager check
            //for now we use the dependency manager itself to resolve runtime dependencies, in the future we might want to unify the builddependency manager with the dependency manager
            var dependencies = AssetItem.Package.Session.DependencyManager.ComputeDependencies(AssetItem.Id, AssetDependencySearchOptions.Out);
            foreach (var assetDependency in dependencies.LinksOut)
            {
                var assetType = assetDependency.Item.Asset.GetType();
                if (!typesToExclude.Contains(assetType)) //filter out what we do not need
                {
                    foreach (var input in typesToInclude.Where(x => x.Key == assetType))
                    {
                        var dependencyType = input.Value;
                        var node = buildDependencyManager.FindOrCreateNode(assetDependency.Item, dependencyType);
                        references.TryAdd(assetDependency.Item.Id, node);
                        node.referencedBy.TryAdd(this, AssetItem.Id); //add this as referenced by child
                    }
                }
            }

            //Input files required
            foreach (var inputFile in mainCompiler.GetInputFiles(AssetItem)) //directly resolve by input files, in the future we might just want this pass
            {
                if (inputFile.Type == UrlType.Content || inputFile.Type == UrlType.ContentLink)
                {
                    var asset = AssetItem.Package.Session.FindAsset(inputFile.Path); //this will search all packages
                    if (asset == null)
                        continue; //this might be an error tho... but in the end compilation might fail so we let the build engine do the error reporting if it really was a issue

                    if (!typesToExclude.Contains(asset.GetType()))
                    {
                        var dependencyType = inputFile.Type == UrlType.Content ? BuildDependencyType.CompileContent : BuildDependencyType.CompileAsset; //Content means we need to load the content, the rest is just asset dependency
                        var node = buildDependencyManager.FindOrCreateNode(asset, dependencyType);
                        references.TryAdd(asset.Id, node);
                        node.referencedBy.TryAdd(this, AssetItem.Id); //add this as referenced by child
                    }
                }
            }

            bool shouldVisitTypes;
            context.Properties.TryGet(VisitRuntimeTypes, out shouldVisitTypes);
            if (shouldVisitTypes || mainCompiler.AlwaysCheckRuntimeTypes)
            {
                var collector = new RuntimeDependenciesCollector(mainCompiler.GetRuntimeTypes(AssetItem));
                var deps = collector.GetDependencies(AssetItem);
                foreach (var reference in deps)
                {
                    var asset = AssetItem.Package.FindAsset(reference.Id);
                    if (asset != null)
                    {
                        var dependencyType = BuildDependencyType.Runtime;
                        var node = buildDependencyManager.FindOrCreateNode(asset, dependencyType);
                        references.TryAdd(asset.Id, node);
                        node.referencedBy.TryAdd(this, AssetItem.Id); //add this as referenced by child
                    }
                }
            }
        }

        private class RuntimeDependenciesCollector : AssetVisitorBase
        {
            private object visitedRuntimeObject;
            private readonly HashSet<IReference> references = new HashSet<IReference>();
            private readonly HashSet<Type> types;

            public RuntimeDependenciesCollector(IEnumerable<Type> enumerable)
            {
                types = new HashSet<Type>(enumerable);
            }

            public IEnumerable<IReference> GetDependencies(AssetItem item)
            {
                Visit(item.Asset);
                return references;
            }

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                var enteringRuntimeObject = visitedRuntimeObject == null && types.Any(x => x.IsInstanceOfType(obj));
                if (enteringRuntimeObject)
                {
                    //from now on we want store references
                    visitedRuntimeObject = obj;
                }

                if (visitedRuntimeObject == null)
                {
                    base.VisitObject(obj, descriptor, visitMembers);
                }
                else
                {
                    // references and base
                    IReference reference = obj as AssetReference;
                    if (reference != null)
                    {
                        references.Add(reference);
                    }
                    else if (AssetRegistry.IsContentType(obj.GetType()))
                    {
                        reference = AttachedReferenceManager.GetAttachedReference(obj);
                        if (reference != null)
                        {
                            references.Add(reference);
                        }
                    }
                    else
                    {
                        base.VisitObject(obj, descriptor, visitMembers);
                    }
                }

                if (enteringRuntimeObject)
                {
                    //from now on we stop storing references
                    visitedRuntimeObject = null;
                }
            }
        }
    }
}
