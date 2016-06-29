// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Physics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Assets.Physics
{
    [DataContract("ColliderShapeAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(ColliderShapeAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.4.0-beta")]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(UpgraderShapeDescriptions))]
    [AssetUpgrader(XenkoConfig.PackageName, 1, 2, typeof(Box2DRemovalUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.2", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    [Display("Collider Shape")]
    public class ColliderShapeAsset : Asset, IAssetCompileTimeDependencies
    {
        public const string FileExtension = ".xkphy;pdxphy";

        protected override int InternalBuildOrder => 600; //make sure we build after Models

        /// <userdoc>
        /// The collection of shapes in this asset, a collection shapes will automatically generate a compound shape.
        /// </userdoc>
        [DataMember(10)]
        [Category]
        [NotNullItems]
        public List<IAssetColliderShapeDesc> ColliderShapes { get; set; } = new List<IAssetColliderShapeDesc>();

        private class UpgraderShapeDescriptions : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.ColliderShapes == null)
                    return;

                foreach (var colliderShape in asset.ColliderShapes)
                {
                    if (colliderShape.Node.Tag == "!Box2DColliderShapeDesc")
                    {
                        var size = 2f * DynamicYamlExtensions.ConvertTo<Vector2>(colliderShape.HalfExtent);
                        colliderShape.Size = DynamicYamlExtensions.ConvertFrom(size);
                        colliderShape.HalfExtent = DynamicYamlEmpty.Default;
                    }
                    if (colliderShape.Node.Tag == "!BoxColliderShapeDesc")
                    {
                        var size = 2f * DynamicYamlExtensions.ConvertTo<Vector3>(colliderShape.HalfExtents);
                        colliderShape.Size = DynamicYamlExtensions.ConvertFrom(size);
                        colliderShape.HalfExtents = DynamicYamlEmpty.Default;
                    }
                    if (colliderShape.Node.Tag == "!CapsuleColliderShapeDesc" || colliderShape.Node.Tag == "!CylinderColliderShapeDesc")
                    {
                        var upVector = DynamicYamlExtensions.ConvertTo<Vector3>(colliderShape.UpAxis);
                        if (upVector == Vector3.UnitX)
                            colliderShape.Orientation = ShapeOrientation.UpX;
                        if (upVector == Vector3.UnitZ)
                            colliderShape.Orientation = ShapeOrientation.UpZ;

                        colliderShape.UpAxis = DynamicYamlEmpty.Default;
                    }
                    if (colliderShape.Node.Tag == "!CapsuleColliderShapeDesc" && colliderShape.Height != null)
                    {
                        colliderShape.Length = 2f * (float)colliderShape.Height;
                        colliderShape.Height = DynamicYamlEmpty.Default;
                    }
                    if (colliderShape.Node.Tag == "!CylinderColliderShapeDesc")
                    {
                        colliderShape.Radius = (float)colliderShape.HalfExtents.X;
                        colliderShape.Height = 2f * (float)colliderShape.HalfExtents.Y;
                        colliderShape.HalfExtents = DynamicYamlEmpty.Default;
                    }
                }
            }
        }

        private class Box2DRemovalUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.ColliderShapes == null)
                    return;

                foreach (dynamic shape in asset.ColliderShapes)
                {
                    var tag = shape.Node.Tag;
                    if (tag != "!Box2DColliderShapeDesc") continue;

                    shape.Node.Tag = "!BoxColliderShapeDesc";
                    shape.Is2D = true;
                    shape.Size.X = shape.Size.X;
                    shape.Size.Y = shape.Size.Y;
                    shape.Size.Z = 0.01f;
                }
            }
        }

        public IEnumerable<IReference> EnumerateCompileTimeDependencies()
        {
            foreach (var shapeDesc in ColliderShapes.OfType<ConvexHullColliderShapeDesc>())
            {
                var reference = AttachedReferenceManager.GetAttachedReference(shapeDesc.Model);
                yield return new AssetReference<Asset>(reference.Id, reference.Url);
            }
        }
    }
}
