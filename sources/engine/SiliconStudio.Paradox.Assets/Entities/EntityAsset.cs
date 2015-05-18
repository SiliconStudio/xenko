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
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;
using IObjectFactory = SiliconStudio.Core.Reflection.IObjectFactory;

namespace SiliconStudio.Paradox.Assets.Entities
{
    [DataContract("EntityAsset")]
    [AssetDescription(FileExtension, false)]
    [AssetCompiler(typeof(EntityAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.EntityThumbnailCompilerQualifiedName, true)]
    [Display("Entity", "An entity")]
    //[AssetFormatVersion(AssetFormatVersion, typeof(Upgrader))]
    public class EntityAsset : AssetImportTracked, IDiffResolver, IAssetComposer
    {
        public const int AssetFormatVersion = 0;

        /// <summary>
        /// The default file extension used by the <see cref="EntityAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxentity";

        public EntityAsset()
        {
            SerializedVersion = AssetFormatVersion;
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
        public Dictionary<Guid, EntityBase> AssetBases = new Dictionary<Guid, EntityBase>();

        private class EntityFactory : IObjectFactory
        {
            public object New(Type type)
            {
                // Create a new root entity, and make sure transformation component is created
                var rootEntity = new Entity();
                rootEntity.Name = "Root";
                rootEntity.GetOrCreate(TransformComponent.Key);

                return new EntityAsset
                {
                    Hierarchy =
                    {
                        Entities = { rootEntity },
                        RootEntity = rootEntity.Id,
                    }
                };
            }
        }

        void IDiffResolver.BeforeDiff(Asset baseAsset, Asset asset1, Asset asset2)
        {
            Guid newId;
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

        class Upgrader : IAssetUpgrader
        {
            public void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode)
            {
                dynamic asset = new DynamicYamlMapping(yamlAssetNode);

                // Get the EntityData, and generate an Id
                var oldEntityData = asset.Data;
                oldEntityData.Id = Guid.NewGuid().ToString().ToLowerInvariant();

                // Create a new EntityDataHierarchy object
                asset.Hierarchy = new YamlMappingNode();
                asset.Hierarchy.Entities = new YamlSequenceNode();
                asset.Hierarchy.Entities.Add(oldEntityData);

                asset["~Base"] = DynamicYamlEmpty.Default;

                // Bump asset version -- make sure it is stored right after Id
                asset.SerializedVersion = AssetFormatVersion;
                asset.MoveChild("SerializedVersion", asset.IndexOf("Id") + 1);

                // Currently not final, so enable at your own risk
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IContentReference> GetCompositionBases()
        {
            return AssetBases.Values.Select(assetBase => assetBase.Base);
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