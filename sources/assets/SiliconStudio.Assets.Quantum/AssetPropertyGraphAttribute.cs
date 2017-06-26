// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Quantum
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(AssetPropertyGraph))]
    public class AssetPropertyGraphAttribute : Attribute
    {
        public AssetPropertyGraphAttribute(Type assetType)
        {
            if (assetType == null) throw new ArgumentNullException(nameof(assetType));
            if (!typeof(Asset).IsAssignableFrom(assetType)) throw new ArgumentException($"The given type must be assignable to the {nameof(Asset)} type.");
            AssetType = assetType;
        }

        public Type AssetType { get; }
    }
}
