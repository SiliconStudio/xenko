// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Physics;
using System;

namespace SiliconStudio.Paradox.Assets.Physics
{
    [DataContract("ColliderShapeAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(ColliderShapeAssetCompiler))]
    [ObjectFactory(typeof(ColliderShapeFactory))]
    [AssetFormatVersion(1)]
    [AssetUpgrader(0, 1, typeof(UpgraderShapeDescriptions))]
    [Display("Collider Shape", "A physics collider shape")]
    public class ColliderShapeAsset : Asset
    {
        public const string FileExtension = ".pdxphy";

        public ColliderShapeAsset()
        {
            ColliderShapes = new List<IColliderShapeDesc>();
        }

        protected override int InternalBuildOrder
        {
            get { return 600; } //make sure we build after Models
        }

        /// <userdoc>
        /// The collection of shapes in this asset, a collection shapes will automatically generate a compound shape.
        /// </userdoc>
        [DataMember(10)]
        [Category]
        public List<IColliderShapeDesc> ColliderShapes { get; set; }

        private class ColliderShapeFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new ColliderShapeAsset();
            }
        }

        class UpgraderShapeDescriptions : AssetUpgraderBase
        {
            protected override void UpgradeAsset(int currentVersion, int targetVersion, ILogger log, dynamic asset)
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
                        var upVector = DynamicYamlExtensions.ConvertTo <Vector3>( colliderShape.UpAxis);
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
    }
}