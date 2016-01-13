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
    public abstract class CustomEntityComponentBase : EntityComponent
    {
        [DataMemberIgnore]
        public Action<Entity> AssociatedDataGenerated;

        [DataMemberIgnore]
        public Action<Entity> EntityAdded;

        [DataMemberIgnore]
        public Action<Entity> EntityRemoved;
    }


    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(CustomEntityComponentProcessor<CustomEntityComponent>))]
    [AllowMultipleComponent]
    public sealed class CustomEntityComponent : CustomEntityComponentBase
    {
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent1 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent1>
        {
        }
    }
    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent2 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent2>
        {
        }
    }
    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent3 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent3>
        {
        }
    }
    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent4 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent4>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent5 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent5>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent6 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent6>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent7 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent7>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent8 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent8>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent9 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent9>
        {
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(Processor))]
    public sealed class MonoComponent10 : CustomEntityComponentBase
    {
        public class Processor : CustomEntityComponentProcessor<MonoComponent10>
        {
        }
    }

    public class CustomEntityComponentProcessor<TCustom> : EntityProcessor<TCustom> where TCustom : CustomEntityComponentBase
    {
        protected override TCustom GenerateComponentData(Entity entity, TCustom component)
        {
            if (component.AssociatedDataGenerated != null)
            {
                component.AssociatedDataGenerated(entity);
            }

            return component;
        }

        protected override bool IsAssociatedDataValid(Entity entity, TCustom component, TCustom associatedData)
        {
            return component == associatedData;
        }

        protected override void OnEntityComponentAdding(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            if (data.EntityAdded != null)
            {
                data.EntityAdded(entity);
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, TCustom component, TCustom data)
        {
            base.OnEntityComponentRemoved(entity, component, data);
            if (data.EntityRemoved != null)
            {
                data.EntityRemoved(entity);
            }
        }
    }

    public class CustomEntityComponentProcessor : EntityProcessor<CustomEntityComponent>
    {
        protected override CustomEntityComponent GenerateComponentData(Entity entity, CustomEntityComponent component)
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

        protected override void OnEntityComponentAdding(Entity entity, CustomEntityComponent component, CustomEntityComponent data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            if (data.EntityAdded != null)
            {
                data.EntityAdded(entity);
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, CustomEntityComponent component, CustomEntityComponent data)
        {
            base.OnEntityComponentRemoved(entity, component, data);
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
            //var processor = entityManager.GetProcessorsByEntity(entity).OfType<CustomEntityComponentProcessor>().FirstOrDefault();
            //Assert.AreEqual(null, processor);
        }

        public static void DumpGC(string text)
        {
            var totalMemory = GC.GetTotalMemory(false);
            var collect0 = GC.CollectionCount(0);
            var collect1 = GC.CollectionCount(1);
            var collect2 = GC.CollectionCount(2);

            Console.WriteLine($"{text}Memory: {totalMemory} GC0: {collect0} GC1: {collect1} GC2: {collect2}");
        }

        public static void Main()
        {
            const int TestCount = 5;
            const int TestEntityCount = 10000;

            long totalTime = 0;
            long stepTime = 0;
            var clock = Stopwatch.StartNew();
            Console.WriteLine($"Test1 -> Add {TestEntityCount} entities + 4 custom components x {TestCount} times");
            Console.WriteLine($"Test2 -> Add {TestEntityCount} entities, (Add 5 custom component, Remove 2 custom component x {TestCount} times)");

            DumpGC($"Start Test1 - ");
            for (int j = 0; j < TestCount; j++)
            {
                var registry = new ServiceRegistry();
                var entityManager = new CustomEntityManager(registry);

                entityManager.AddProcessor(new HierarchicalProcessor()); // Order: -1000  - Important to pre-register this processor
                entityManager.AddProcessor(new TransformProcessor()); // Order: -100

                int dataCount = 0;
                int entityAddedCount = 0;
                int entityRemovedCount = 0;

                clock.Restart();
                for (int i = 0; i < TestEntityCount; i++)
                {
                    var entity = new Entity
                    {
                        new MonoComponent1()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent2()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent3()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent4()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent5()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent6()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent7()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent8()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent9()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                        new MonoComponent10()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        },
                    };

                    entityManager.Add(entity);
                }
                var elapsed = clock.ElapsedMilliseconds;
                stepTime += elapsed;
                DumpGC($"\t[{j}] Elapsed: {elapsed}ms ");
            }
            DumpGC($"End - Elapsed {stepTime}ms ");
            totalTime += stepTime;
            stepTime = 0;

            Console.WriteLine();

            DumpGC($"Start Test2 - ");
            {
                var registry = new ServiceRegistry();
                var entityManager = new CustomEntityManager(registry);

                for (int i = 0; i < TestEntityCount; i++)
                {
                    var entity = new Entity();
                    entityManager.Add(entity);
                }

                for (int j = 0; j < TestCount; j++)
                {
                    int dataCount = 0;
                    int entityAddedCount = 0;
                    int entityRemovedCount = 0;

                    clock.Restart();
                    foreach (var entity in entityManager)
                    {
                        entity.Add(new MonoComponent1()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });
                        entity.Add(new MonoComponent2()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });
                        entity.Add(new MonoComponent3()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent4()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent5()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent6()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent7()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent8()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent9()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });

                        entity.Add(new MonoComponent10()
                        {
                            AssociatedDataGenerated = e => dataCount++,
                            EntityAdded = e => entityAddedCount++,
                            EntityRemoved = e => entityRemovedCount++
                        });
                    }
                    var elapsedAdd = clock.ElapsedMilliseconds;
                    stepTime += elapsedAdd;
                    clock.Restart();

                    foreach (var entity in entityManager)
                    {
                        entity.Remove<MonoComponent1>();
                        entity.Remove<MonoComponent2>();
                        entity.Remove<MonoComponent3>();
                        entity.Remove<MonoComponent4>();
                        entity.Remove<MonoComponent5>();
                        entity.Remove<MonoComponent6>();
                        entity.Remove<MonoComponent7>();
                        entity.Remove<MonoComponent8>();
                        entity.Remove<MonoComponent9>();
                        entity.Remove<MonoComponent10>();
                    }

                    var elapsedRemove = clock.ElapsedMilliseconds;
                    stepTime += elapsedRemove;
                    DumpGC($"\t[{j}] ElapsedAdd: {elapsedAdd} ElapsedRemove: {elapsedRemove} ");
                }
            }
            DumpGC($"End - Elapsed {stepTime}ms ");
            totalTime += stepTime;
            stepTime = 0;

            Console.WriteLine($"Total Time: {totalTime}ms");
        }
    }
}