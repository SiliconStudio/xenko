// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.EntityModel
{
    //[DataSerializer(typeof(CloneEntityComponentSerializer<>), Mode = DataSerializerGenericMode.Type)]
    [DataContract]
    public class EntityComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponent"/> class.
        /// </summary>
        public EntityComponent()
        {
        }

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
        [DataMemberIgnore]
        public virtual PropertyKey DefaultKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
            public static readonly PropertyKey DefaultKey = new T().DefaultKey;
        }
    }
}