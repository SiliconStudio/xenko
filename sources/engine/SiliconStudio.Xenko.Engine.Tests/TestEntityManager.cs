// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine.Tests
{
    /// <summary>
    /// Tests for the <see cref="EntityManager"/>.
    /// </summary>
    [TestFixture]
    public partial class TestEntityManager
    {
        /// <summary>
        /// Check when adding an entity that the TransformProcessor and HierarchicalProcessor are corerctly added.
        /// </summary>
        [Test]
        public void TestDefaultProcessors()
        {
            var registry = new ServiceRegistry();
            var entityManager = new CustomEntityManager(registry);

            // Create events collector to check fired events on EntityManager
            var componentTypes = new List<Type>();
            var entityAdded = new List<Entity>();
            var entityRemoved = new List<Entity>();

            entityManager.ComponentTypeAdded += (sender, type) => componentTypes.Add(type);
            entityManager.EntityAdded += (sender, entity1) => entityAdded.Add(entity1);
            entityManager.EntityRemoved += (sender, entity1) => entityRemoved.Add(entity1);

            Assert.AreEqual(0, entityManager.Processors.Count);

            // ================================================================
            // 1) Add an entity with the default TransformComponent to the Entity Manager
            // ================================================================

            var entity = new Entity();
            entityManager.Add(entity);

            // Check types are correctly registered
            Assert.AreEqual(1, componentTypes.Count);
            Assert.AreEqual(typeof(TransformComponent), componentTypes[0]);

            // Check entity correctly added
            Assert.AreEqual(1, entityManager.Count);
            Assert.True(entityManager.Contains(entity));
            Assert.AreEqual(1, entityAdded.Count);
            Assert.AreEqual(entity, entityAdded[0]);
            Assert.AreEqual(0, entityRemoved.Count);

            // We should have 2 processors
            Assert.AreEqual(2, entityManager.Processors.Count);

            Assert.True(entityManager.Processors[0] is HierarchicalProcessor);
            Assert.True(entityManager.Processors[1] is TransformProcessor);

            // Check internal mapping of component types => EntityProcessor
            Assert.AreEqual(1, entityManager.MapComponentTypeToProcessors.Count);
            Assert.True(entityManager.MapComponentTypeToProcessors.ContainsKey(typeof(TransformComponent)));

            var processorListForTransformComponentType = entityManager.MapComponentTypeToProcessors[typeof(TransformComponent)];
            Assert.AreEqual(2, processorListForTransformComponentType.Count);
            Assert.True(processorListForTransformComponentType[0] is HierarchicalProcessor);
            Assert.True(processorListForTransformComponentType[1] is TransformProcessor);

            // clear events collector
            componentTypes.Clear();
            entityAdded.Clear();
            entityRemoved.Clear();

            // ================================================================
            // 2) Add another empty entity
            // ================================================================

            var newEntity = new Entity();
            entityManager.Add(newEntity);

            // We should not have new component types registered
            Assert.AreEqual(0, componentTypes.Count);

            // Check entity correctly added
            Assert.AreEqual(2, entityManager.Count);
            Assert.True(entityManager.Contains(newEntity));
            Assert.AreEqual(1, entityAdded.Count);
            Assert.AreEqual(newEntity, entityAdded[0]);
            Assert.AreEqual(0, entityRemoved.Count);

            // We should still have 2 processors
            Assert.AreEqual(2, entityManager.Processors.Count);

            Assert.True(entityManager.Processors[0] is HierarchicalProcessor);
            Assert.True(entityManager.Processors[1] is TransformProcessor);

            componentTypes.Clear();
            entityAdded.Clear();
            entityRemoved.Clear();

            // ================================================================
            // 3) Remove previous entity
            // ================================================================

            entityManager.Remove(newEntity);

            // Check entity correctly removed
            Assert.AreEqual(1, entityManager.Count);
            Assert.False(entityManager.Contains(newEntity));
            Assert.AreEqual(0, entityAdded.Count);
            Assert.AreEqual(1, entityRemoved.Count);
            Assert.AreEqual(newEntity, entityRemoved[0]);

            componentTypes.Clear();
            entityAdded.Clear();
            entityRemoved.Clear();
        }

        /// <summary>
        /// Tests adding/removing multiple components of the same type on an entity handled by the EntityManager
        /// </summary>
        [Test]
        public void TestMultipleComponents()
        {
            var registry = new ServiceRegistry();
            var entityManager = new CustomEntityManager(registry);

            var events = new List<CustomEntityComponentEventArgs>();

            var entity = new Entity
            {
                new CustomEntityComponent()
                {
                    Changed = e =>events.Add(e)
                }
            };
            var customComponent = entity.Get<CustomEntityComponent>();

            // ================================================================
            // 1) Add an entity with a component to the Entity Manager
            // ================================================================

            // Add component
            entityManager.Add(entity);

            // Check that component was correctly processed when first adding the entity
            Assert.AreEqual(1, entityManager.Count);
            Assert.AreEqual(3, entityManager.Processors.Count);

            // Verify that the processor has correctly registered the component
            var customProcessor = entityManager.GetProcessor<CustomEntityComponentProcessor>();
            Assert.NotNull(customProcessor);

            Assert.AreEqual(1, customProcessor.RegisteredComponents.Count);
            Assert.True(customProcessor.RegisteredComponents.ContainsKey(customComponent));

            // Verify that events are correctly propagated
            var expectedEvents = new List<CustomEntityComponentEventArgs>()
            {
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.GenerateComponentData, entity, customComponent),
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentAdding, entity, customComponent),
            };
            Assert.AreEqual(expectedEvents, events);
            events.Clear();

            // ================================================================
            // 2) Add a component to the entity that is already handled by the Entity Manager
            // ================================================================

            // Check that component is correctly processed when adding it after the entity is already into the EntityManager
            var customComponent2 = new CustomEntityComponent()
            {
                Changed = e => events.Add(e)
            };
            entity.Components.Add(customComponent2);

            // Verify that the processor has correctly registered the component
            Assert.AreEqual(2, customProcessor.RegisteredComponents.Count);
            Assert.True(customProcessor.RegisteredComponents.ContainsKey(customComponent2));

            expectedEvents = new List<CustomEntityComponentEventArgs>()
            {
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.GenerateComponentData, entity, customComponent2),
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentAdding, entity, customComponent2),
            };
            Assert.AreEqual(expectedEvents, events);
            events.Clear();

            // ================================================================
            // 3) Remove the 1st CustomEntityComponent from the entity
            // ================================================================

            // Test remove first component
            entity.Components.Remove(customComponent);

            // Verify that the processor has correctly removed the component
            Assert.AreEqual(1, customProcessor.RegisteredComponents.Count);
            Assert.False(customProcessor.RegisteredComponents.ContainsKey(customComponent));

            Assert.AreEqual(null, customComponent.Entity);
            expectedEvents = new List<CustomEntityComponentEventArgs>()
            {
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentRemoved, entity, customComponent),
            };
            Assert.AreEqual(expectedEvents, events);
            events.Clear();

            // ================================================================
            // 4) Remove the 2nd CustomEntityComponent from the entity
            // ================================================================

            // Test remove second component
            entity.Components.Remove(customComponent2);

            // Verify that the processor has correctly removed the component
            Assert.AreEqual(0, customProcessor.RegisteredComponents.Count);
            Assert.False(customProcessor.RegisteredComponents.ContainsKey(customComponent2));

            Assert.AreEqual(null, customComponent2.Entity);
            expectedEvents = new List<CustomEntityComponentEventArgs>()
            {
                new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentRemoved, entity, customComponent2),
            };
            Assert.AreEqual(expectedEvents, events);
            events.Clear();

            // The processor is still registered but is not running on any component
            Assert.AreEqual(3, entityManager.Processors.Count);
            Assert.NotNull(entityManager.GetProcessor<CustomEntityComponentProcessor>());
        }

        public static void Main()
        {
        }
    }

    public class CustomEntityManager : EntityManager
    {
        public CustomEntityManager(IServiceRegistry registry) : base(registry)
        {
        }
    }

    public enum CustomEntityComponentEventType
    {
        GenerateComponentData,

        EntityComponentAdding,

        EntityComponentRemoved
    }

    public struct CustomEntityComponentEventArgs : IEquatable<CustomEntityComponentEventArgs>
    {
        public CustomEntityComponentEventArgs(CustomEntityComponentEventType type, Entity entity, EntityComponent component)
        {
            Type = type;
            Entity = entity;
            Component = component;
        }

        public readonly CustomEntityComponentEventType Type;

        public readonly Entity Entity;

        public readonly EntityComponent Component;

        public bool Equals(CustomEntityComponentEventArgs other)
        {
            return Type == other.Type && Equals(Entity, other.Entity) && Equals(Component, other.Component);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CustomEntityComponentEventArgs && Equals((CustomEntityComponentEventArgs)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Entity != null ? Entity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Component != null ? Component.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(CustomEntityComponentEventArgs left, CustomEntityComponentEventArgs right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CustomEntityComponentEventArgs left, CustomEntityComponentEventArgs right)
        {
            return !left.Equals(right);
        }
    }

    public class CustomEntityComponentProcessor<TCustom> : EntityProcessor<TCustom> where TCustom : CustomEntityComponentBase
    {
        public Dictionary<TCustom, TCustom> RegisteredComponents => ComponentDatas;

        public CustomEntityComponentProcessor(params Type[] requiredAdditionalTypes) : base(requiredAdditionalTypes)
        {
        }

        protected override TCustom GenerateComponentData(Entity entity, TCustom component)
        {
            component.Changed?.Invoke(new CustomEntityComponentEventArgs(CustomEntityComponentEventType.GenerateComponentData, entity, component));
            return component;
        }

        protected override void OnEntityComponentAdding(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            component.Changed?.Invoke(new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentAdding, entity, component));
        }

        protected override void OnEntityComponentRemoved(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentRemoved(entity, component, data);
            component.Changed?.Invoke(new CustomEntityComponentEventArgs(CustomEntityComponentEventType.EntityComponentRemoved, entity, component));
        }
    }

    public class CustomEntityComponentProcessor : CustomEntityComponentProcessor<CustomEntityComponent>
    {
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(CustomEntityComponentProcessorWithDependency))]
    [AllowMultipleComponent]
    public sealed class CustomEntityComponentWithDependency : CustomEntityComponentBase
    {
        public TransformComponent Link;
    }

    public class CustomEntityComponentProcessorWithDependency : CustomEntityComponentProcessor<CustomEntityComponentWithDependency>
    {
        public CustomEntityComponentProcessorWithDependency() : base(typeof(TransformComponent))
        {
        }

        protected override bool IsAssociatedDataValid(Entity entity, CustomEntityComponentWithDependency component, CustomEntityComponentWithDependency associatedData)
        {
            return base.IsAssociatedDataValid(entity, component, associatedData) && associatedData.Link == entity.Transform;
        }
    }
}