// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("Entity")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(EntityAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.EntityThumbnailCompilerQualifiedName)]
    [AssetFactory(typeof(EntityFactory))]
    [AssetDescription("Entity", "An entity", true)]
    [AssetFormatVersion(AssetFormatVersion, typeof(Upgrader))]
    public class EntityAsset : AssetImport
    {
        public const int AssetFormatVersion = 1;

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

        private class EntityFactory : IAssetFactory
        {
            public Asset New()
            {
                return new EntityAsset();
            }
        }

        class Upgrader : IAssetUpgrader
        {
            public void Upgrade(ILogger log, YamlMappingNode yamlAssetNode)
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
    }
}