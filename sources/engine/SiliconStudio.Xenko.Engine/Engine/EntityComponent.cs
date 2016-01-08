// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine
{

    public interface IEntityComponentNode
    {
        IEntityComponentNode Next { get; set; }

        EntityComponent Component { get; }
    }

    /// <summary>
    /// Allows a component of the same type to be added multiple time to the same entity (default is <c>false</c>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowMultipleComponentAttribute : Attribute
    {
    }

    public struct EntityComponentAttributes
    {
        private static readonly Dictionary<Type, EntityComponentAttributes> ComponentAttributes = new Dictionary<Type, EntityComponentAttributes>();
        public EntityComponentAttributes(bool allowMultipleComponent)
        {
            AllowMultipleComponent = allowMultipleComponent;
        }

        public readonly bool AllowMultipleComponent ;

        public static EntityComponentAttributes Get<T>() where T : EntityComponent
        {
            return GetInternal(typeof(T));
        }

        public static EntityComponentAttributes Get(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof(EntityComponent).IsAssignableFrom(type)) throw new ArgumentException("The type must be of EntityComponent", "type");
            return GetInternal(type);
        }

        private static EntityComponentAttributes GetInternal(Type type)
        {
            lock (ComponentAttributes)
            {
                EntityComponentAttributes attributes;
                if (!ComponentAttributes.TryGetValue(type, out attributes))
                {
                    attributes = new EntityComponentAttributes(type.GetTypeInfo().GetCustomAttribute<AllowMultipleComponentAttribute>() != null);
                    ComponentAttributes.Add(type, attributes);
                    return attributes;
                }
            }
            return new EntityComponentAttributes();
        }
    }

    /// <summary>
    /// Base class for <see cref="Entity"/> components.
    /// </summary>
    [DataSerializer(typeof(EntityComponent.Serializer))]
    [DataContract]
    public abstract class EntityComponent : IEntityComponentNode
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

        internal class Serializer : DataSerializer<EntityComponent>
        {
            public override void Serialize(ref EntityComponent obj, ArchiveMode mode, SerializationStream stream)
            {
                var entity = obj.Entity;

                // Force containing Entity to be collected by serialization, no need to reassign it to EntityComponent.Entity
                stream.SerializeExtended(ref entity, mode);
            }
        }

        IEntityComponentNode IEntityComponentNode.Next { get; set; }

        EntityComponent IEntityComponentNode.Component => this;
    }
}