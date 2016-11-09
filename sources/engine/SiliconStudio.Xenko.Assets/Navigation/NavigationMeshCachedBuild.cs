// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    /// <summary>
    /// Holds the cached result of building a scene into a navigation mesh, with input vertex data to allow incremental builds.
    /// </summary>
    [DataContract]
    internal class NavigationMeshCachedBuild
    {
        public Dictionary<Guid, NavigationMeshCachedBuildObject> Objects =
            new Dictionary<Guid, NavigationMeshCachedBuildObject>();

        public NavigationMesh NavigationMesh;

        public int SettingsHash = 0;

        /// <summary>
        /// Registers a new processed object that is build into the navigation mesh
        /// </summary>
        /// <param name="entity">The entity that was processed</param>
        /// <param name="data">The collider vertex data that is generated for this entity</param>
        public void Add(Entity entity, NavigationMeshInputBuilder data)
        {
            StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
            if (collider != null)
            {
                int hash = NavigationMeshBuildUtils.HashEntityCollider(collider);
                Objects.Add(entity.Id, new NavigationMeshCachedBuildObject()
                {
                    Guid = entity.Id,
                    ParameterHash = hash,
                    Data = data
                });
            }
        }

        /// <summary>
        /// Checks if an entity has moved in any way or it's collider settings/composition changed
        /// </summary>
        /// <param name="newEntity">The entity to check</param>
        /// <returns>true if entity is new, or one of its settings changed affecting the collider's shape</returns>
        public bool IsUpdatedOrNew(Entity newEntity)
        {
            NavigationMeshCachedBuildObject existingObject;
            StaticColliderComponent collider = newEntity.Get<StaticColliderComponent>();
            if (Objects.TryGetValue(newEntity.Id, out existingObject))
            {
                if (collider != null)
                {
                    int hash = NavigationMeshBuildUtils.HashEntityCollider(collider);
                    return (hash != existingObject.ParameterHash);
                }
            }
            return true;
        }

        /// <summary>
        /// When called on the old build with the list of new entities it wil determine the bounding boxes 
        /// for the areas that should be updated due to removals of colliders
        /// </summary>
        /// <param name="entities">The new list of entities to compare to</param>
        /// <returns>A list of the areas that are updated due to them being removed in the input list</returns>
        public List<BoundingBox> GetRemovedAreas(List<Entity> entities)
        {
            List<BoundingBox> ret = new List<BoundingBox>();

            HashSet<Guid> inputHashSet = new HashSet<Guid>();
            foreach (Entity e in entities)
            {
                StaticColliderComponent collider = e.Get<StaticColliderComponent>();
                if (collider != null)
                    inputHashSet.Add(e.Id);
            }
            foreach (var p in Objects)
            {
                if (!inputHashSet.Contains(p.Key))
                {
                    ret.Add(p.Value.Data.BoundingBox);
                }
            }

            return ret;
        }
    }
}