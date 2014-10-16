// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Describes dependencies (in/out/miss) for a specific asset.
    /// </summary>
    /// <remarks>There are 3 types of dependencies:
    /// <ul>
    /// <li><c>in</c> dependencies: through the <see cref="Parents"/> property, contains assets                                 
    /// that are referencing this asset.</li>
    /// <li><c>out</c> dependencies: directly from the instance of this object, contains assets 
    /// that are referenced this asset.</li>
    /// <li><c>missing</c> dependencies: through the <see cref="MissingReferences"/> property, 
    /// contains assets referenced by this asset and that are missing.</li>
    /// </ul>
    /// </remarks>
    [DebuggerDisplay("In [{Parents.Count}] / Out [{Count}] Miss [{MissingReferenceCount}]")]
    public class AssetDependencySet : HashSet<AssetItem>
    {
        private readonly AssetItem item;
        private readonly HashSet<AssetItem> parents;
        private List<IContentReference> missingReferences;

        public AssetDependencySet(AssetItem assetItem)
            : base(AssetItem.DefaultComparerById)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            item = assetItem;
            parents = new HashSet<AssetItem>(AssetItem.DefaultComparerById);
        }

        public AssetDependencySet(AssetDependencySet set)
            : base(AssetItem.DefaultComparerById)
        {
            if (set == null) throw new ArgumentNullException("set");
            item = set.Item;
            parents = new HashSet<AssetItem>(AssetItem.DefaultComparerById);

            // Copy Output refs
            foreach (var child in set)
            {
                Add(child.Clone(true));
            }

            // Copy missing refs
            if (set.missingReferences != null)
            {
                foreach (var missingRef in set.missingReferences)
                {
                    AddMissingReference(missingRef);
                }
            }

            // Copy Input refs
            foreach (var parent in set.parents)
            {
                parents.Add(parent.Clone(true));
            }
        }

        public Guid Id
        {
            get
            {
                return item.Id;
            }
        }

        /// <summary>
        /// Gets the itemReferenced.
        /// </summary>
        /// <value>The itemReferenced.</value>
        public AssetItem Item
        {
            get
            {
                return item;
            }
        }

        public HashSet<AssetItem> Parents
        {
            get
            {
                return parents;
            }
        }

        /// <summary>
        /// Resets this instance and clear all dependencies (including missing)
        /// </summary>
        public void Reset(bool keepParents)
        {
            missingReferences = null;
            Clear();
            if (!keepParents)
            {
                parents.Clear();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has missing references.
        /// </summary>
        /// <value><c>true</c> if this instance has missing references; otherwise, 
        /// <c>false</c>.</value>
        public bool HasMissingReferences
        {
            get
            {
                return missingReferences != null && missingReferences.Count > 0;
            }
        }

        public int MissingReferenceCount
        {
            get
            {
                return missingReferences != null ? missingReferences.Count : 0;
            }
        }

        /// <summary>
        /// Gets the missing references.
        /// </summary>
        /// <value>The missing references.</value>
        public IEnumerable<IContentReference> MissingReferences
        {
            get
            {
                return missingReferences ?? Enumerable.Empty<IContentReference>();
            }
        }

        /// <summary>
        /// Adds a missing reference.
        /// </summary>
        /// <param name="contentReference">The content reference.</param>
        /// <exception cref="System.ArgumentNullException">contentReference</exception>
        public void AddMissingReference(IContentReference contentReference)
        {
            if (contentReference == null) throw new ArgumentNullException("contentReference");
            if (missingReferences == null)
                missingReferences = new List<IContentReference>();
            missingReferences.Add(contentReference);
        }

        /// <summary>
        /// Removes a missing reference
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void RemoveMissingReference(Guid guid)
        {
            if (missingReferences == null) return;
            for (int i = missingReferences.Count - 1; i >= 0; i--)
            {
                if (missingReferences[i].Id == guid)
                {
                    missingReferences.RemoveAt(i);
                    break;
                }
            }

            // Remove list if no longer used
            if (missingReferences.Count == 0)
            {
                missingReferences = null;
            }
        }
    }
}