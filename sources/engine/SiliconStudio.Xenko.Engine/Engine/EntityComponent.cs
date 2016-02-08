// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Base class for <see cref="Entity"/> components.
    /// </summary>
    [DataSerializer(typeof(Serializer))]
    [DataContract(Inherited = true)]
    public abstract class EntityComponent
    {
        /// <summary>
        /// Gets or sets the owner entity.
        /// </summary>
        /// <value>
        /// The owner entity.
        /// </value>
        [DataMemberIgnore]
        public Entity Entity { get; internal set; }

        /// <summary>
        /// Gets the entity and throws an exception if the entity is null.
        /// </summary>
        /// <value>The entity.</value>
        /// <exception cref="System.InvalidOperationException">Entity on this instance is null</exception>
        [DataMemberIgnore]
        protected Entity EnsureEntity
        {
            get
            {
                if (Entity == null)
                    throw new InvalidOperationException($"Entity on this instance [{GetType().Name}] cannot be null");
                return Entity;
            }
        }

        internal class Serializer : DataSerializer<EntityComponent>
        {
            public override void Serialize(ref EntityComponent obj, ArchiveMode mode, SerializationStream stream)
            {
                var entity = obj.Entity;

                // Force containing Entity to be collected by serialization, no need to reassign it to EntityComponent.Entity
                stream.SerializeExtended(ref entity, mode);
            }
        }
    }
}