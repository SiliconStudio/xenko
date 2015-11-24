// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SharpYaml.Serialization;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using IObjectFactory = SiliconStudio.Core.Reflection.IObjectFactory;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("EntityAsset")]
    [AssetDescription(FileExtension, false)]
    //[AssetCompiler(typeof(SceneAssetCompiler))]
    //[ThumbnailCompiler(PreviewerCompilerNames.EntityThumbnailCompilerQualifiedName, true)]
    [Display("Entity")]
    //[AssetFormatVersion(AssetFormatVersion, typeof(Upgrader))]
    public class EntityAsset : EntityAssetBase
    {
        public const int AssetFormatVersion = 0;

        /// <summary>
        /// The default file extension used by the <see cref="EntityAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkentity;.pdxentity";
    }

    [DataContract()]
    public abstract class EntityAssetBase : Asset, IDiffResolver, IAssetPartContainer
    {
        protected EntityAssetBase()
        {
            Hierarchy = new EntityHierarchyData();
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DataMember(20)]
        public EntityHierarchyData Hierarchy { get; set; }

        /// <summary>
        /// The various <see cref="EntityAsset"/> that are instantiated in this one.
        /// </summary>
        [DataMemberIgnore]
        [Obsolete]
        public Dictionary<Guid, EntityBase> AssetBases = new Dictionary<Guid, EntityBase>();

        public override Asset CreateChildAsset(string location)
        {
            var newAsset = (EntityAssetBase)base.CreateChildAsset(location);

            // Process entities to create new ids for entities and base id
            for (int i = 0; i < Hierarchy.Entities.Count; i++)
            {
                var oldEntityDesign = Hierarchy.Entities[i];
                var newEntityDesign = newAsset.Hierarchy.Entities[i];
                // Assign a new guid
                newEntityDesign.Entity.Id = Guid.NewGuid();

                // Store the baseid of the new version
                newEntityDesign.Design.BaseId = oldEntityDesign.Entity.Id;

                // If entity is root, update RootEntities
                // TODO: might not be optimal if many root entities (should use dictionary and second pass on RootEntities)
                int indexRoot = newAsset.Hierarchy.RootEntities.IndexOf(oldEntityDesign.Entity.Id);
                if (indexRoot >= 0)
                {
                    newAsset.Hierarchy.RootEntities[indexRoot] = newEntityDesign.Entity.Id;
                }
            }

            return newAsset;
        }

        void IDiffResolver.BeforeDiff(Asset baseAsset, Asset asset1, Asset asset2)
        {
            var baseEntityAsset = (EntityAsset)baseAsset;
            var entityAsset1 = (EntityAsset)asset1;
            var entityAsset2 = (EntityAsset)asset2;

            // Let's remap IDs in asset2 (if it comes from a FBX or such, we need to do that)
            var oldBaseTree = new EntityTreeAsset(baseEntityAsset.Hierarchy);
            var newBaseTree = new EntityTreeAsset(entityAsset2.Hierarchy);

            var idRemapping = new Dictionary<Guid, Guid>();

            // Try to transfer ID from old base to new base
            var mergeResult = AssetMerge.Merge(oldBaseTree, newBaseTree, oldBaseTree, node =>
            {
                if (typeof(Guid).IsAssignableFrom(node.InstanceType) && node.BaseNode != null && node.Asset1Node != null)
                {
                    idRemapping.Add((Guid)node.Asset1Node.Instance, (Guid)node.BaseNode.Instance);
                }

                return AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1(node);
            });

            if (mergeResult.HasErrors)
            {
                //mergeResult.CopyTo();
            }

            EntityAnalysis.RemapEntitiesId(entityAsset2.Hierarchy, idRemapping);
        }

        public IEnumerable<AssetPart> CollectParts()
        {
            foreach (var entityDesign in Hierarchy.Entities)
            {
                yield return new AssetPart(entityDesign.Entity.Id, entityDesign.Design.BaseId);
            }
        }

        public bool ContainsPart(Guid id)
        {
            return Hierarchy.Entities.ContainsKey(id);
        }
    }

    [DataContract("EntityBase")]
    public class EntityBase
    {
        /// <summary>
        /// The <see cref="EntityAsset"/> base.
        /// </summary>
        public AssetBase Base;

        public Guid SourceRoot;

        /// <summary>
        /// Maps <see cref="Entity.Id"/> from this asset to base asset one.
        /// </summary>
        public Dictionary<Guid, Guid> IdMapping;
    }
}