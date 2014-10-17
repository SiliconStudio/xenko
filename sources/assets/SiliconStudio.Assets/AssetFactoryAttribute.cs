// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Attribute to define for a <see cref="IAssetFactory"/> for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AssetFactoryAttribute : Attribute
    {
        private readonly string typeName;

        /// <summary>
        /// Gets the name of the <see cref="SiliconStudio.Assets.AssetFactoryAttribute"/> type
        /// </summary>
        /// <value>The name of the serializable type.</value>
        public string FactoryTypeName
        {
            get { return this.typeName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconStudio.Assets.AssetFactoryAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must be of type <see cref="IAssetFactory"/>.</param>
        public AssetFactoryAttribute(Type type)
        {
            this.typeName = type != null ? type.AssemblyQualifiedName : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconStudio.Assets.AssetFactoryAttribute"/> class.
        /// </summary>
        /// <param name="typeName">The type must be of type <see cref="IAssetFactory"/>.</param>
        public AssetFactoryAttribute(string typeName)
        {
            this.typeName = typeName != null ? typeName.ToUpperInvariant() : null;
        }
    }
}