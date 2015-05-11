// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SharpYaml;
using SharpYaml.Serialization;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.ProceduralModels;

namespace SiliconStudio.Paradox.Assets.ProceduralModels
{
    /// <summary>
    /// The geometric primitive asset.
    /// </summary>
    [DataContract("ProceduralModelAsset")]
    [AssetDescription(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.ProceduralModelThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(ProceduralModelAssetCompiler))]
    [Display(185, "Procedural Model", "A procedural model")]
    [AssetFormatVersion(2)]
    [AssetUpgrader(0, 1, 2, typeof(Upgrader))]
    public sealed class ProceduralModelAsset : Asset, IModelAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxpromodel";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProceduralModelAsset"/> class.
        /// </summary>
        public ProceduralModelAsset()
        {
            Type = new CubeProceduralModel();
            SerializedVersion = AssetFormatVersion;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type", AlwaysExpand = true)]
        public IProceduralModel Type { get; set; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get { return Type != null ? Type.MaterialInstances : Enumerable.Empty<KeyValuePair<string, MaterialInstance>>(); } }

        class Upgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(int currentVersion, int targetVersion, ILogger log, dynamic asset)
            {
                // Introduction of MaterialInstance
                var material = asset.Type.Material;
                if (material != null)
                {
                    asset.Type.MaterialInstance = new YamlMappingNode();
                    asset.Type.MaterialInstance.Material = material;
                    asset.Type.Material = DynamicYamlEmpty.Default;
                }
                var type = asset.Type.Node as YamlMappingNode;
                if (type != null && type.Tag == "!CubeProceduralModel")
                {
                    // Size changed from scalar to vector3
                    var size = asset.Type.Size as DynamicYamlScalar;
                    if (size != null)
                    {
                        var vecSize = new YamlMappingNode
                        {
                            { new YamlScalarNode("X"), new YamlScalarNode(size.Node.Value) },
                            { new YamlScalarNode("Y"), new YamlScalarNode(size.Node.Value) },
                            { new YamlScalarNode("Z"), new YamlScalarNode(size.Node.Value) }
                        };
                        vecSize.Style = YamlStyle.Flow;
                        asset.Type.Size = vecSize;
                    }
                }
            }
        }
    }
}