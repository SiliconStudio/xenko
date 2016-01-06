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

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(CustomEntityComponentProcessor))]
    public class CustomEntityComponent : EntityComponent
    {
        [DataMemberIgnore]
        public Action<Entity> AssociatedDataGenerated;

        [DataMemberIgnore]
        public Action<Entity> EntityAdded;

        [DataMemberIgnore]
        public Action<Entity> EntityRemoved;
    }

    public class CustomEntityComponentProcessor : EntityProcessor<CustomEntityComponent, CustomEntityComponent>
    {
        protected override CustomEntityComponent GenerateAssociatedData(Entity entity, CustomEntityComponent component)
        {
            if (component.AssociatedDataGenerated != null)
            {
                component.AssociatedDataGenerated(entity);
            }

            return component;
        }

        protected override bool IsAssociatedDataValid(Entity entity, CustomEntityComponent component, CustomEntityComponent associatedData)
        {
            return component == associatedData;
        }

        protected override void OnEntityAdding(Entity entity, CustomEntityComponent data)
        {
            base.OnEntityAdding(entity, data);
            if (data.EntityAdded != null)
            {
                data.EntityAdded(entity);
            }
        }

        protected override void OnEntityRemoved(Entity entity, CustomEntityComponent data)
        {
            base.OnEntityRemoved(entity, data);
            if (data.EntityRemoved != null)
            {
                data.EntityRemoved(entity);
            }
        }
    }


    [TestFixture]
    public class TestEntityManager
    {
        [Test]
        public void TestSingleComponent()
        {
            var registry = new ServiceRegistry();
            var entityManager = new CustomEntityManager(registry);

            entityManager.AddProcessor(new HierarchicalProcessor()); // Order: -1000  - Important to pre-register this processor
            entityManager.AddProcessor(new TransformProcessor());    // Order: -100

            int dataCount = 0;
            int entityAddedCount = 0;
            int entityRemovedCount = 0;

            var entity = new Entity
            {
                new CustomEntityComponent()
                {
                    AssociatedDataGenerated = e => dataCount++,
                    EntityAdded = e => entityAddedCount++,
                    EntityRemoved = e => entityRemovedCount++
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
                AssociatedDataGenerated = e => dataCount++,
                EntityAdded = e => entityAddedCount++,
                EntityRemoved = e => entityRemovedCount++
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
            var processor = entityManager.GetProcessorsByEntity(entity).OfType<CustomEntityComponentProcessor>().FirstOrDefault();
            Assert.AreEqual(null, processor);
        }


        public static void DumpGC()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var collect0 = GC.CollectionCount(0);
            var collect1 = GC.CollectionCount(1);
            var collect2 = GC.CollectionCount(2);

            Console.WriteLine($"Memory: {totalMemory} GC0: {collect0} GC1: {collect1} GC2: {collect2}");
        }

        public static void Main()
        {
            DumpGC();
            var registry = new ServiceRegistry();
            var entityManager = new CustomEntityManager(registry);

            entityManager.AddProcessor(new HierarchicalProcessor()); // Order: -1000  - Important to pre-register this processor
            entityManager.AddProcessor(new TransformProcessor());    // Order: -100

            int dataCount = 0;
            int entityAddedCount = 0;
            int entityRemovedCount = 0;

            var clock = Stopwatch.StartNew();

            for (int i = 0; i < 10000; i++)
            {
                var entity = new Entity
                {
                    new CustomEntityComponent()
                    {
                        AssociatedDataGenerated = e => dataCount++,
                        EntityAdded = e => entityAddedCount++,
                        EntityRemoved = e => entityRemovedCount++
                    }
                };

                entityManager.Add(entity);
            }

            Console.WriteLine($"Elapsed: {clock.ElapsedMilliseconds}");
            DumpGC();
            Console.ReadKey();
        }
    }
}