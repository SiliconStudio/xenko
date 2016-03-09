// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A base class for an entity prefab that contains entities used by <see cref="Prefab"/> and <see cref="Scene"/>
    /// </summary>
    [DataContract]
    public abstract class PrefabBase : ComponentBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Prefab"/>.
        /// </summary>
        protected PrefabBase()
        {
            Entities = new TrackingCollection<Entity>();
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        public TrackingCollection<Entity> Entities { get; }

        // Note: Added for compatibility with previous code
        [Obsolete]
        public void AddChild(Entity entity)
        {
            Entities.Add(entity);
        }

        [Obsolete]
        public void RemoveChild(Entity entity)
        {
            Entities.Remove(entity);
        }
    }
}