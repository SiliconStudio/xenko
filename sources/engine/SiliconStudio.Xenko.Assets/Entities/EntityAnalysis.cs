// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    // TODO: Move this somewhere else.
    public static class EntityAnalysis
    {
        /// <summary>
        /// Remaps the entities identifier.
        /// </summary>
        /// <param name="entityHierarchy">The entity hierarchy.</param>
        /// <param name="idRemapping">The identifier remapping.</param>
        public static void RemapEntitiesId(AssetCompositeHierarchyData<EntityDesign, Entity> entityHierarchy, Dictionary<Guid, Guid> idRemapping)
        {
            Guid newId;

            // Remap entities in asset2 with new Id
            for (int i = 0; i < entityHierarchy.RootPartIds.Count; ++i)
            {
                if (idRemapping.TryGetValue(entityHierarchy.RootPartIds[i], out newId))
                    entityHierarchy.RootPartIds[i] = newId;
            }

            foreach (var entity in entityHierarchy.Parts)
            {
                if (idRemapping.TryGetValue(entity.Entity.Id, out newId))
                    entity.Entity.Id = newId;
            }

            // Sort again the EntityCollection (since ID changed)
            entityHierarchy.Parts.Sort();
        }
    }
}
