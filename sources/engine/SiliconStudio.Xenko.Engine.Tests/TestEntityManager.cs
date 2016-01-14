// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine.Tests
{
    public class CustomEntityManager : EntityManager
    {
        public CustomEntityManager(IServiceRegistry registry) : base(registry)
        {
        }
    }

    public class CustomEntityComponentProcessor<TCustom> : EntityProcessor<TCustom> where TCustom : CustomEntityComponentBase
    {
        protected override TCustom GenerateComponentData(Entity entity, TCustom component)
        {
            if (component.ComponentDataGenerated != null)
            {
                component.ComponentDataGenerated(component);
            }

            return component;
        }

        protected override void OnEntityComponentAdding(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            if (data.EntityComponentAdded != null)
            {
                data.EntityComponentAdded(component);
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentRemoved(entity, component, data);
            if (data.EntityComponentRemoved != null)
            {
                data.EntityComponentRemoved(component);
            }
        }
    }

    public class CustomEntityComponentProcessor : CustomEntityComponentProcessor<CustomEntityComponent>
    {
    }

    [TestFixture]
    public partial class TestEntityManager
    {
        [Test]
        public void TestSingleComponent()
        {
            var registry = new ServiceRegistry();
            var entityManager = new CustomEntityManager(registry);

            entityManager.Processors.Add(new HierarchicalProcessor()); // Order: -1000  - Important to pre-register this processor
            entityManager.Processors.Add(new TransformProcessor());    // Order: -100

            int dataCount = 0;
            int entityAddedCount = 0;
            int entityRemovedCount = 0;

            var entity = new Entity
            {
                new CustomEntityComponent()
                {
                    ComponentDataGenerated = e => dataCount++,
                    EntityComponentAdded = e => entityAddedCount++,
                    EntityComponentRemoved = e => entityRemovedCount++
                }
            };

            // Add component
            entityManager.Add(entity);

            // Check that component was correctly processed
            Assert.AreEqual(1, entityManager.Count);
            Assert.AreEqual(1, dataCount);
            Assert.AreEqual(1, entityAddedCount);
            Assert.AreEqual(0, entityRemovedCount);

            // Add a second component
            entity.Components.Add(new CustomEntityComponent()
            {
                ComponentDataGenerated = e => dataCount++,
                EntityComponentAdded = e => entityAddedCount++,
                EntityComponentRemoved = e => entityRemovedCount++
            }
                );

            Assert.AreEqual(2, dataCount);
            Assert.AreEqual(2, entityAddedCount);
            Assert.AreEqual(0, entityRemovedCount);

            // Test remove first component
            var customComponent = entity.Components.Get<CustomEntityComponent>();
            entity.Components.Remove(customComponent);

            Assert.AreEqual(null, customComponent.Entity);
            Assert.AreEqual(2, dataCount);
            Assert.AreEqual(2, entityAddedCount);
            Assert.AreEqual(1, entityRemovedCount);

            // Test remove second component
            customComponent = entity.Components.Get<CustomEntityComponent>();
            entity.Components.Remove(customComponent);

            Assert.AreEqual(null, customComponent.Entity);
            Assert.AreEqual(2, dataCount);
            Assert.AreEqual(2, entityAddedCount);
            Assert.AreEqual(2, entityRemovedCount);

            // Check that processor has been removed after removing the last component
            //var processor = entityManager.GetProcessorsByEntity(entity).OfType<CustomEntityComponentProcessor>().FirstOrDefault();
            //Assert.AreEqual(null, processor);
        }

        public static void Main()
        {
        }
    }
}