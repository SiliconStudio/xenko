// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Game entity. It usually aggregates multiple EntityComponent
    /// </summary>
    //[ContentSerializer(typeof(EntityContentSerializer))]
    //[ContentSerializer(typeof(DataContentSerializer<Entity>))]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Entity>))]
    [DataStyle(DataStyle.Normal)]
    [DataContract("Entity")]
    public class Entity : ComponentBase, IEnumerable
    {
        protected TransformComponent transform;

        /// <summary>
        /// The components stored in this entity.
        /// </summary>
        [DataMember(100, DataMemberMode.Content)]
        public PropertyContainer Components;

        static Entity()
        {
            PropertyContainer.AddAccessorProperty(typeof(Entity), TransformComponent.Key);
        }

        /// <summary>
        /// Create a new <see cref="Entity"/> instance.
        /// </summary>
        public Entity()
            : this(null)
        {
        }

        /// <summary>
        /// Create a new <see cref="Entity"/> instance having the provided name.
        /// </summary>
        /// <param name="name">The name to give to the entity</param>
        public Entity(string name)
            : this(Vector3.Zero, name)
        {
        }

        /// <summary>
        /// Create a new <see cref="Entity" /> instance having the provided name and initial position.
        /// </summary>
        /// <param name="position">The initial position of the entity</param>
        /// <param name="name">The name to give to the entity</param>
        public Entity(Vector3 position, string name = null)
            : base(name)
        {
            Components = new PropertyContainer(this);
            Components.PropertyUpdated += EntityPropertyUpdated;

            Transform = new TransformComponent();
            transform.Position = position;

            Group = EntityGroup.Group0;
        }

        [DataMember(-10), Display(Browsable = false)]
        public override Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [DataMember(0)] // Name is serialized
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Transform"/> associated to this entity.
        /// Added for convenience over usual Get/Set method.
        /// </summary>
        [DataMemberIgnore]
        public TransformComponent Transform
        {
            get { return transform; }
            set
            {
                var transformationOld = transform;
                transform = value;
                Components.RaisePropertyContainerUpdated(TransformComponent.Key, transform, transformationOld);
            }
        }

        /// <summary>
        /// Gets or sets the group of this entity.
        /// </summary>
        /// <value>The group.</value>
        [DataMember(10)]
        [DefaultValue(EntityGroup.Group0)]
        public EntityGroup Group { get; set; }

        /// <summary>
        /// Gets or create a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the entity component</typeparam>
        /// <returns>A new or existing instance of {T}</returns>
        public T GetOrCreate<T>() where T : EntityComponent, new()
        {
            var key = EntityComponent.GetDefaultKey<T>();
            var component = Components.Get(key);
            if (component == null)
            {
                component = new T();
                Components.SetObject(key, component);
            }

            return (T)component;
        }

        /// <summary>
        /// Gets or create a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the entity component</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>A new or existing instance of {T}</returns>
        public T GetOrCreate<T>(PropertyKey<T> key) where T : EntityComponent, new()
        {
            var component = Components.Get(key);
            if (component == null)
            {
                component = new T();
                Components.Set(key, component);
            }

            return component;
        }

        /// <summary>
        /// Adds the specified component using the <see cref="EntityComponent.DefaultKey" />.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <exception cref="System.ArgumentNullException">component</exception>
        public void Add(EntityComponent component)
        {
            if (component == null) throw new ArgumentNullException("component");
            Components.SetObject(component.GetDefaultKey(), component);
        }

        /// <summary>
        /// Gets a component by the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <returns>The component or null if does no exist</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public T Get<T>() where T : EntityComponent, new()
        {
            return (T)Components.Get(EntityComponent.GetDefaultKey<T>());
        }

        /// <summary>
        /// Gets a component by the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The component or null if does no exist</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public T Get<T>(PropertyKey<T> key) where T : EntityComponent
        {
            if (key == null) throw new ArgumentNullException("key");
            return Components.Get(key);
        }

        /// <summary>
        /// Sets a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public void Add<T>(PropertyKey<T> key, T value) where T : EntityComponent
        {
            if (key == null) throw new ArgumentNullException("key");
            Components.SetObject(key, value);
        }

        /// <summary>
        /// Sets a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        [Obsolete("Use Add() method instead")]
        public void Set<T>(PropertyKey<T> key, T value) where T : EntityComponent
        {
            if (key == null) throw new ArgumentNullException("key");
            Components.SetObject(key, value);
        }

        /// <summary>
        /// Removes a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <param name="key">The key.</param>
        /// <returns><c>True</c> if the component was removed, <c>False</c> otherwise.</returns>
        public bool Remove<T>(PropertyKey<T> key)
        {
            return Components.Remove(key);
        }
        
        private void EntityPropertyUpdated(ref PropertyContainer propertyContainer, PropertyKey propertyKey, object newValue, object oldValue)
        {
            // Remove entity owner from previous EntityComponent.
            if (oldValue is EntityComponent)
            {
                ((EntityComponent)oldValue).Entity = null;
            }

            // Set entity owner to this new EntityComponent.
            if (newValue is EntityComponent)
            {
                ((EntityComponent)newValue).Entity = this;
            }
        }

        internal class EntityDebugView
        {
            private readonly Entity entity;

            public EntityDebugView(Entity entity)
            {
                this.entity = entity;
            }

            public string Name
            {
                get { return entity.Name; }
            }

            public Entity[] Children
            {
                get
                {
                    var transformationComponent = entity.Transform;
                    if (transformationComponent == null)
                        return null;

                    return transformationComponent.Children.Select(x => x.Entity).ToArray();
                }
            }

            public EntityComponent[] Components
            {
                get
                {
                    return entity.Components.Select(x => x.Value).OfType<EntityComponent>().ToArray();
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Entity {0}", Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Components.Values.OfType<EntityComponent>().GetEnumerator();
        }
    }
}