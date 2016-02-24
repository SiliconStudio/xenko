// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A prefab that contains entities.
    /// </summary>
    [DataContract("Prefab")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Prefab>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Prefab>), Profile = "Asset")]
    public class Prefab : ComponentBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Prefab"/>.
        /// </summary>
        public Prefab()
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