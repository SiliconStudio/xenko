// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.EntityModel.Data
{
    // Entities as they are stored in the git shared scene file
    public partial class EntityData
    {
        [DataMemberIgnore]
        public EntityHierarchyData Container;

        [DataMember(5)]
        public Guid Id;

        [DataMember(10)]
        public string Name;

        [DataMember(20)]
        public TrackingDictionary<PropertyKey, EntityComponentData> Components { get; private set; }

        public EntityData()
        {
            Id = Guid.NewGuid();
            Components = new TrackingDictionary<PropertyKey, EntityComponentData>();
            Components.CollectionChanged += Components_CollectionChanged;
        }

        void Components_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Remove entity owner from previous EntityComponentData.
            if (e.OldItem is EntityComponentData)
            {
                ((EntityComponentData)e.OldItem).Entity = null;
            }

            // Set entity owner to this new EntityComponentData.
            if (e.Item is EntityComponentData)
            {
                ((EntityComponentData)e.Item).Entity = this;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Id, Name, string.Join(", ", Components.Values.Select(x => x.GetType().Name)));
        }
    }
}