// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// Game entity. It usually aggregates multiple EntityComponent
    /// </summary>
    //[ContentSerializer(typeof(EntityContentSerializer))]
    //[ContentSerializer(typeof(DataContentSerializer<Entity>))]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    [DataConverter(AutoGenerate = false, ContentReference = true)]
    [DataSerializer(typeof(EntitySerializer))]
    [ContentSerializer(typeof(DataContentConverterSerializer<EntityHierarchyData, Entity>))]
    public class Entity : ComponentBase, IContentUrl, IEnumerable
    {
        protected TransformationComponent transformation;
        internal List<Task> prepareTasks;

        static Entity()
        {
            PropertyContainer.AddAccessorProperty(typeof(Entity), TransformationComponent.Key);
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
        /// Create a new <see cref="Entity"/> instance having the provided name and initial position.
        /// </summary>
        /// <param name="position">The initial position of the entity</param>
        /// <param name="name">The name to give to the entity</param>
        public Entity(Vector3 position, string name = null)
            : base(name)
        {
            Tags.PropertyUpdated += EntityPropertyUpdated;

            Transformation = new TransformationComponent();
            transformation.Translation = position;
        }

        /// <summary>
        /// Gets or sets the <see cref="Transformation"/> associated to this entity.
        /// Added for convenience over usual Get/Set method.
        /// </summary>
        public TransformationComponent Transformation
        {
            get { return transformation; }
            set
            {
                var transformationOld = transformation;
                transformation = value;
                Tags.RaisePropertyContainerUpdated(TransformationComponent.Key, transformation, transformationOld);
            }
        }

        /// <summary>
        /// Gets or create a component with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the entity component</typeparam>
        /// <returns>A new or existing instance of {T}</returns>
        public T GetOrCreate<T>() where T : EntityComponent, new()
        {
            var key = EntityComponent.GetDefaultKey<T>();
            var component = Tags.Get(key);
            if (component == null)
            {
                component = new T();
                Tags.SetObject(key, component);
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
            var component = Tags.Get(key);
            if (component == null)
            {
                component = new T();
                Tags.Set(key, component);
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
            Tags.SetObject(component.DefaultKey, component);
        }

        /// <summary>
        /// Gets a component by the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <returns>The component or null if does no exist</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public T Get<T>() where T : EntityComponent, new()
        {
            return (T)Tags.Get(EntityComponent.GetDefaultKey<T>());
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
            return Tags.Get(key);
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
            Tags.SetObject(key, value);
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
            Tags.SetObject(key, value);
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
                    var transformationComponent = entity.Transformation;
                    if (transformationComponent == null)
                        return null;

                    return transformationComponent.Children.Select(x => x.Entity).ToArray();
                }
            }

            public EntityComponent[] Components
            {
                get
                {
                    return entity.Tags.Select(x => x.Value).OfType<EntityComponent>().ToArray();
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Entity {0}", Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tags.Values.OfType<EntityComponent>().GetEnumerator();
        }

        string IContentUrl.Url { get; set; }
    }
}