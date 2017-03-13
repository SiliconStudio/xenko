// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Holds the cached result of building a scene into a navigation mesh, with input vertex data to allow incremental builds.
    /// </summary>
    [DataContract]
    internal class NavigationMeshCache
    {
        public Dictionary<Guid, NavigationMeshCachedObject> Objects =
            new Dictionary<Guid, NavigationMeshCachedObject>();
        
        public List<BoundingBox> BoundingBoxes = new List<BoundingBox>();

        public int SettingsHash = 0;

        /// <summary>
        /// Registers a new processed object that is build into the navigation mesh
        /// </summary>
        /// <param name="collider">The collider that was processed</param>
        /// <param name="data">The collider vertex data that is generated for this entity</param>
        /// <param name="planes">Collection of infinite planes for this colliders, these are special since their size is not known until the bounding box are known</param>
        /// <param name="entityColliderHash">The hash of the entity and collider obtained with <see cref="NavigationMeshBuildUtils.HashEntityCollider"/></param>
        public void Add(StaticColliderComponent collider, NavigationMeshInputBuilder data, ICollection<Plane> planes, int entityColliderHash)
        {
            Objects.Add(collider.Id, new NavigationMeshCachedObject()
            {
                Guid = collider.Id,
                ParameterHash = entityColliderHash,
                Planes = new List<Plane>(planes),
                InputBuilder = data
            });
        }


        /// <summary>
        /// Checks if an entity has moved in any way or it's collider settings/composition changed
        /// </summary>
        /// <param name="newEntity">The entity to check</param>
        /// <returns>true if entity is new, or one of its settings changed affecting the collider's shape</returns>
        public bool IsUpdatedOrNew(StaticColliderComponent collider)
        {
            var entity = collider.Entity;
            NavigationMeshCachedObject existingObject;
            if (Objects.TryGetValue(entity.Id, out existingObject))
            {
                int hash = NavigationMeshBuildUtils.HashEntityCollider(collider);
                return (hash != existingObject.ParameterHash);
            }
            return true;
        }

        /// <summary>
        /// When called on the old build with the list of new entities it wil determine the bounding boxes 
        /// for the areas that should be updated due to removals of colliders
        /// </summary>
        /// <param name="entities">The new list of entities to compare to</param>
        /// <returns>A list of the areas that are updated due to them being removed in the input list</returns>
        public List<BoundingBox> GetRemovedAreas(List<StaticColliderComponent> colliders)
        {
            List<BoundingBox> ret = new List<BoundingBox>();

            HashSet<Guid> inputHashSet = new HashSet<Guid>();
            foreach (StaticColliderComponent collider in colliders)
            {
                inputHashSet.Add(collider.Id);
            }

            foreach (var p in Objects)
            {
                if (!inputHashSet.Contains(p.Key))
                {
                    ret.Add(p.Value.InputBuilder.BoundingBox);
                }
            }

            return ret;
        }
    }
}