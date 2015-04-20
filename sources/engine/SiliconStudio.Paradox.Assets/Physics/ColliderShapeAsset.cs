// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Physics;
using System;

namespace SiliconStudio.Paradox.Assets.Physics
{
    [DataContract("ColliderShapeAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(ColliderShapeAssetCompiler))]
    [ObjectFactory(typeof(ColliderShapeFactory))]
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
    }
}