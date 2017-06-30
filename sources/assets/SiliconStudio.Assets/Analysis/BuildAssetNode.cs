// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Analysis
{
    public struct BuildAssetLink
    {
        public BuildAssetLink(BuildAssetNode source, BuildAssetNode target, BuildDependencyType dependencyType)
        {
            Source = source;
            Target = target;
            DependencyType = dependencyType;
        }

        public BuildDependencyType DependencyType { get; }

        public BuildAssetNode Source { get; }

        public BuildAssetNode Target { get; }

        public bool HasOne(BuildDependencyType type)
        {
            return (DependencyType & type) != 0;
        }

        public bool HasAll(BuildDependencyType type)
        {
            return (DependencyType & type) == type;
        }
    }

    public class BuildAssetNode
    {
        public static PropertyKey<bool> VisitRuntimeTypes = new PropertyKey<bool>("VisitRuntimeTypes", typeof(BuildAssetNode));

        private readonly BuildDependencyManager buildDependencyManager;
        //private readonly ConcurrentDictionary<AssetId, BuildAssetNode> references = new ConcurrentDictionary<AssetId, BuildAssetNode>();
        private readonly ConcurrentDictionary<BuildAssetLink, BuildAssetLink> references = new ConcurrentDictionary<BuildAssetLink, BuildAssetLink>();
        private long version = -1;

        public AssetItem AssetItem { get; }

        public Type CompilationContext { get; }

        public ICollection<BuildAssetLink> References => references.Values;

        public BuildAssetNode(AssetItem assetItem, Type compilationContext, BuildDependencyManager dependencyManager)
        {
            AssetItem = assetItem;
            CompilationContext = compilationContext;
            buildDependencyManager = dependencyManager;
        }

        /// <summary>
        /// Performs analysis on the asset to figure out all the needed dependencies
        /// </summary>
        /// <param name="context">The compiler context</param>
        public void Analyze(AssetCompilerContext context)
        {
            var assetVersion = AssetItem.Version;
            if (Interlocked.Exchange(ref version, assetVersion) == assetVersion)
                return; //same version, skip analysis, do not clear links

            var mainCompiler = BuildDependencyManager.AssetCompilerRegistry.GetCompiler(AssetItem.Asset.GetType(), CompilationContext);
            if (mainCompiler == null)
                return; //scripts and such don't have compiler

            var typesToInclude = new HashSet<BuildDependencyInfo>(mainCompiler.GetInputTypes(AssetItem));
            var typesToExclude = new HashSet<Type>(mainCompiler.GetInputTypesToExclude(AssetItem));

            //clean up our references
            references.Clear();

            //DependencyManager check
            //for now we use the dependency manager itself to resolve runtime dependencies, in the future we might want to unify the builddependency manager with the dependency manager
            var dependencies = AssetItem.Package.Session.DependencyManager.ComputeDependencies(AssetItem.Id, AssetDependencySearchOptions.Out);
            if (dependencies != null)
            {
                foreach (var assetDependency in dependencies.LinksOut)
                {
                    var assetType = assetDependency.Item.Asset.GetType();
                    if (!typesToExclude.Contains(assetType)) //filter out what we do not need
                    {
                        foreach (var input in typesToInclude.Where(x => x.AssetType == assetType))
                        {
                            var node = buildDependencyManager.FindOrCreateNode(assetDependency.Item, input.CompilationContext);
                            var link = new BuildAssetLink(this, node, input.DependencyType);
                            references.TryAdd(link, link);
                        }
                    }
                }
            }

            //Input files required
            foreach (var inputFile in new HashSet<ObjectUrl>(mainCompiler.GetInputFiles(AssetItem))) //directly resolve by input files, in the future we might just want this pass
            {
                if (inputFile.Type == UrlType.Content || inputFile.Type == UrlType.ContentLink)
                {
                    var asset = AssetItem.Package.Session.FindAsset(inputFile.Path); //this will search all packages
                    if (asset == null)
                        continue; //this might be an error tho... but in the end compilation might fail so we let the build engine do the error reporting if it really was a issue

                    if (!typesToExclude.Contains(asset.GetType()))
                    {
                        // TODO: right now, we consider that assets returned by GetInputFiles must be compiled in AssetCompilationContext. At some point, we might need to be able to specify a custom context.
                        var dependencyType = inputFile.Type == UrlType.Content ? BuildDependencyType.CompileContent : BuildDependencyType.CompileAsset; //Content means we need to load the content, the rest is just asset dependency
                        var node = buildDependencyManager.FindOrCreateNode(asset, typeof(AssetCompilationContext));
                        var link = new BuildAssetLink(this, node, dependencyType);
                        references.TryAdd(link, link);
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
                        // TODO: right now, we consider that assets found with RuntimeDependenciesCollector must be compiled in AssetCompilationContext. At some point, we might need to be able to specify a custom context.
                        var dependencyType = BuildDependencyType.Runtime;
                        var node = buildDependencyManager.FindOrCreateNode(asset, typeof(AssetCompilationContext));
                        var link = new BuildAssetLink(this, node, dependencyType);
                        references.TryAdd(link, link);
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
