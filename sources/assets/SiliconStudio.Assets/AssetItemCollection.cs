// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A collection of <see cref="AssetItem"/> that contains only absolute location without any drive information. This class cannot be inherited.
    /// </summary>
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    [DataContract("AssetItems")]
    public sealed class AssetItemCollection : List<AssetItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItemCollection"/> class.
        /// </summary>
        public AssetItemCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public AssetItemCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItemCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public AssetItemCollection(IEnumerable<AssetItem> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Converts this instance to a YAML text.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public string ToText()
        {
            var stream = new MemoryStream();
            AssetSerializer.Default.Save(stream, this);
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        /// <summary>
        /// Parses items from the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>SiliconStudio.Assets.AssetItemCollection.</returns>
        public static AssetItemCollection FromText(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            bool aliasOccurred;
            var assetItems = (AssetItemCollection)AssetSerializer.Default.Load(stream, null, null, out aliasOccurred);
            if (aliasOccurred)
            {
                foreach (var assetItem in assetItems)
                {
                    assetItem.IsDirty = true;
                }
            }
            return assetItems;
        }
    }
}