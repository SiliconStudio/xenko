// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Base class for <see cref="Entity"/> components.
    /// </summary>
    [DataSerializer(typeof(EntityComponent.Serializer))]
    [DataContract]
    public abstract class EntityComponent : ComponentBase
    {
        /// <summary>
        /// Gets or sets the owner entity.
        /// </summary>
        /// <value>
        /// The owner entity.
        /// </value>
        [DataMemberIgnore]
        public Entity Entity { get; set; }

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
                    throw new InvalidOperationException(string.Format("Entity on this instance [{0}] cannot be null", GetType().Name));
                return Entity;
            }
        }

        /// <summary>
        /// The default key this component is associated to.
        /// </summary>
        public abstract PropertyKey GetDefaultKey();

        /// <summary>
        /// Gets the default key for the specified entity component type.
        /// </summary>
        /// <typeparam name="T">An entity component type</typeparam>
        /// <returns>PropertyKey.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyKey GetDefaultKey<T>() where T : EntityComponent, new()
        {
            return EntityComponentHelper<T>.DefaultKey;
        }

        struct EntityComponentHelper<T> where T : EntityComponent, new()
        {
            public static readonly PropertyKey DefaultKey = new T().GetDefaultKey();
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