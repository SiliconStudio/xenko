// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An <see cref="AssetItem"/> accessor to get member value and overrides.
    /// </summary>
    public class AssetItemAccessor
    {
        private readonly List<AssetItem> baseItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItemAccessor"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="System.ArgumentNullException">item</exception>
        public AssetItemAccessor(AssetItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            Item = item;
            baseItems = new List<AssetItem>();

            var nextBaseItem = Item;
            // Process the hierarchy and try to find 
            while ((nextBaseItem = nextBaseItem.FindBase()) != null)
            {
                baseItems.Add(nextBaseItem);
            }
        }

        /// <summary>
        /// Gets the item associated to this instance.
        /// </summary>
        /// <value>The item associated to this instance.</value>
        public AssetItem Item { get; private set; }

        /// <summary>
        /// Try to gets the value of an asset member and provides the assets that
        /// <see cref="OverrideType" /> information for this particular
        /// member.
        /// </summary>
        /// <param name="path">The path to the member.</param>
        /// <returns>AssetMemberValue.</returns>
        /// <exception cref="System.ArgumentNullException">path</exception>
        public AssetMemberValue TryGetMemberValue(MemberPath path)
        {
            if (path == null) throw new ArgumentNullException("path");

            object value;
            OverrideType overrideType;

            if (path.TryGetValue(Item.Asset, out value, out overrideType))
            {
                // If the member is new, we don't need to check further the inheritance.
                if (overrideType.IsNew())
                {
                    return new AssetMemberValue(value, overrideType, null);
                }

                AssetItem overriderItem = null;

                // Else check bases and find the first new or sealed base
                foreach (var nextBaseItem in baseItems)
                {
                    object parentValue;
                    OverrideType parentOverrideType;
                    if (path.TryGetValue(nextBaseItem.Asset, out parentValue, out parentOverrideType))
                    {
                        overriderItem = nextBaseItem;

                        // If we found a base asset with a sealed member, sets the orriderItem to this item and
                        // make sure the overrideType of the current asset item instance is set to Base|Sealed
                        if (parentOverrideType.IsSealed())
                        {
                            overrideType = OverrideType.Base | OverrideType.Sealed;
                            break;
                        }

                        // Check if value is coming from a member with a new value
                        if (parentOverrideType.IsNew())
                        {
                            break;
                        }
                    }
                }
                return new AssetMemberValue(value, overrideType, overriderItem);
            }

            // Not valid, return an empty value
            return new AssetMemberValue();
        }
    }
}