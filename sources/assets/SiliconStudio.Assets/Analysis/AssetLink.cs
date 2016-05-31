// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Represent a link between Assets.
    /// </summary>
    public struct AssetLink : IContentLink
    {
        /// <summary>
        /// The asset item pointed by the dependency.
        /// </summary>
        public readonly AssetItem Item;

        private ContentLinkType type;

        private readonly IReference reference;

        /// <summary>
        /// Create an asset dependency of type <paramref name="type"/> and pointing to <paramref name="item"/>
        /// </summary>
        /// <param name="item">The item the dependency is pointing to</param>
        /// <param name="type">The type of the dependency between the items</param>
        public AssetLink(AssetItem item, ContentLinkType type)
        {
            if (item == null) throw new ArgumentNullException("item");

            Item = item;
            this.type = type;
            reference = item.ToReference();
        }

        // This constructor exists for better factorization of code in AssetDependencies. 
        // It should not be turned into public as AssetItem is not valid.
        internal AssetLink(IReference reference, ContentLinkType type)
        {
            if (reference == null) throw new ArgumentNullException("reference");

            Item = null;
            this.type = type;
            this.reference = reference;
        }

        public ContentLinkType Type
        {
            get { return type; }
            set { type = value; }
        }

        public IReference Element { get { return reference; } }

        /// <summary>
        /// Gets a clone copy of the asset dependency.
        /// </summary>
        /// <returns>the clone instance</returns>
        public AssetLink Clone()
        {
            return new AssetLink(Item.Clone(true), Type);
        }
    }
}
