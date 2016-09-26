// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Selectors
{
    /// <summary>
    /// An <see cref="AssetSelector"/> using tags stored in <see cref="Asset.Tags"/>
    /// </summary>
    [DataContract("TagSelector")]
    public class TagSelector : AssetSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagSelector"/> class.
        /// </summary>
        public TagSelector()
        {
            Tags = new TagCollection();
        }

        /// <summary>
        /// Gets the tags that will be used to select an asset.
        /// </summary>
        /// <value>The tags.</value>
        public TagCollection Tags { get; private set; }

        public override IEnumerable<string> Select(PackageSession packageSession, IContentIndexMap contentIndexMap)
        {
            return packageSession.Packages
                .SelectMany(package => package.Assets) // Select all assets
                .Where(assetItem => assetItem.Asset.Tags.Any(tag => Tags.Contains(tag))) // Matches tags
                .Select(x => x.Location.FullPath); // Convert to string;
        }
    }
}