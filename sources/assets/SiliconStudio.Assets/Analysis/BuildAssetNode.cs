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
    /// <summary>
    /// A structure representing a link (a dependency) between two <see cref="BuildAssetNode"/> instances (assets).
    /// </summary>
    public struct BuildAssetLink : IEquatable<BuildAssetLink>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="BuildAssetLink"/> structure.
        /// </summary>
        /// <param name="source">The source asset of the dependency.</param>
        /// <param name="target">The target asset of the dependency.</param>
        /// <param name="dependencyType">The type of dependency.</param>
        public BuildAssetLink(BuildAssetNode source, BuildAssetNode target, BuildDependencyType dependencyType)
        {
            Source = source;
            Target = target;
            DependencyType = dependencyType;
        }

        /// <summary>
        /// The type of dependency.
        /// </summary>
        public BuildDependencyType DependencyType { get; }

        /// <summary>
        /// The source asset of the dependency.
        /// </summary>
        public BuildAssetNode Source { get; }

        /// <summary>
        /// The target asset of the dependency.
        /// </summary>
        public BuildAssetNode Target { get; }

        /// <summary>
        /// Indicates whether this <see cref="BuildAssetLink"/> has at least one of the dependency of the given flags.
        /// </summary>
        /// <param name="type">A bitset of <see cref="BuildDependencyType"/>.</param>
        /// <returns>True if it has at least one of the given dependencies, false otherwise.</returns>
        public bool HasOne(BuildDependencyType type)
        {
            return (DependencyType & type) != 0;
        }

        /// <summary>
        /// Indicates whether this <see cref="BuildAssetLink"/> has at all dependencies of the given flags.
        /// </summary>
        /// <param name="type">A bitset of <see cref="BuildDependencyType"/>.</param>
        /// <returns>True if it has all the given dependencies, false otherwise.</returns>
        public bool HasAll(BuildDependencyType type)
        {
            return (DependencyType & type) == type;
        }

        /// <inheritdoc/>
        public bool Equals(BuildAssetLink other)
        {
            return DependencyType == other.DependencyType && Equals(Source, other.Source) && Equals(Target, other.Target);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BuildAssetLink && Equals((BuildAssetLink)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)DependencyType;
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Target != null ? Target.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(BuildAssetLink left, BuildAssetLink right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(BuildAssetLink left, BuildAssetLink right)
        {
            return !left.Equals(right);
        }
    }

    public class BuildAssetNode
    {
        public static PropertyKey<bool> VisitRuntimeTypes = new PropertyKey<bool>("VisitRuntimeTypes", typeof(BuildAssetNode));

        private readonly BuildDependencyManager buildDependencyManager;
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
