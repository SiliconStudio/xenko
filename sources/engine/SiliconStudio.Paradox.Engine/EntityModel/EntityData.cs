// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Data;

namespace SiliconStudio.Paradox.EntityModel.Data
{
    // Entities as they are stored in the git shared scene file
    public partial class EntityData
    {
        /// <summary>
        /// The <see cref="EntityHierarchyData"/> that is containing this entity.
        /// </summary>
        [DataMemberIgnore]
        public EntityHierarchyData Container;

        /// <summary>
        /// The identifier of this entity.
        /// </summary>
        [DataMember(5)]
        public Guid Id;

        /// <summary>
        /// The name of this entity.
        /// </summary>
        /// <userdoc>
        /// The name of this entity.
        /// </userdoc>
        [DataMember(10)]
        public string Name;

        /// <summary>
        /// Gets the collection of <see cref="EntityComponentData"/> of this entity.
        /// </summary>
        /// <userdoc>
        /// The collection of components of this entity.
        /// </userdoc>
        [DataMember(20)]
        public TrackingDictionary<PropertyKey, EntityComponentData> Components { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityData"/> class.
        /// </summary>
        public EntityData()
        {
            Id = Guid.NewGuid();
            Components = new TrackingDictionary<PropertyKey, EntityComponentData>();
            Components.CollectionChanged += ComponentsCollectionChanged;
        }

        /// <summary>
        /// Gets the transformation component of the <see cref="EntityData"/>. If there is no transformation component in
        /// the entity and <see cref="createIfMissing"/> is true, it will create a new <see cref="TransformationComponentData"/>
        /// and add it to the entity before returning it.
        /// </summary>
        /// <param name="createIfMissing">If <c>true</c> and if the entity does not have a transformation component, this method will create and add a new one.</param>
        /// <returns>The transformation component of this entity.</returns>
        public TransformationComponentData GetTransformation(bool createIfMissing = true)
        {
            EntityComponentData component;
            if (!Components.TryGetValue(TransformationComponent.Key, out component) && createIfMissing)
            {
                var result = new TransformationComponentData();
                Components.Add(TransformationComponent.Key, result);
                return result;
            }
            return (TransformationComponentData)component;
        }

        void ComponentsCollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Id, Name, string.Join(", ", Components.Values.Select(x => x.GetType().Name)));
        }
    }
}