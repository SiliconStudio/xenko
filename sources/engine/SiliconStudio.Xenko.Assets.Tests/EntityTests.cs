// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Tests
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void TestEntitySerialization()
        {
            // Basic test of entity serialization with links between entities (entity-entity, entity-component)
            // E1
            //   | E2 + link to E1 via TestEntityComponent
            // E3
            // E4 + link to E3.Transform component via TestEntityComponent

            var originAsset = new EntityGroupAsset();

            var entity1 = new Entity() { Name = "E1" };
            var entity2 = new Entity() { Name = "E2" };
            var entity3 = new Entity() { Name = "E3" };
            var entity4 = new Entity() { Name = "E4" };

            entity1.Transform.Children.Add(entity2.Transform);

            // Test a link between entity1 and entity2
            entity2.Add(new TestEntityComponent() { EntityLink = entity1 });

            // Test a component link between entity4 and entity 3
            entity4.Add(new TestEntityComponent() { EntityComponentLink = entity3.Transform });

            originAsset.Hierarchy.Entities.Add(entity1);
            originAsset.Hierarchy.Entities.Add(entity2);
            originAsset.Hierarchy.Entities.Add(entity3);
            originAsset.Hierarchy.Entities.Add(entity4);

            originAsset.Hierarchy.RootEntities.Add(entity1.Id);
            originAsset.Hierarchy.RootEntities.Add(entity3.Id);
            originAsset.Hierarchy.RootEntities.Add(entity4.Id);

            using (var stream = new MemoryStream())
            {
                AssetSerializer.Save(stream, originAsset);

                stream.Position = 0;
                var serializedVersion = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine(serializedVersion);

                stream.Position = 0;
                var newAsset = (EntityGroupAsset)AssetSerializer.Load(stream, "xkentity");

                // Check that we have exactly the same root entities
                Assert.AreEqual(originAsset.Hierarchy.RootEntities, newAsset.Hierarchy.RootEntities);

                Assert.AreEqual(originAsset.Hierarchy.Entities.Count, newAsset.Hierarchy.Entities.Count);

                var newEntityDesign1 = newAsset.Hierarchy.Entities[entity1.Id];
                var newEntityDesign2 = newAsset.Hierarchy.Entities[entity2.Id];
                var newEntityDesign3 = newAsset.Hierarchy.Entities[entity3.Id];
                var newEntityDesign4 = newAsset.Hierarchy.Entities[entity4.Id];
                Assert.NotNull(newEntityDesign1);
                Assert.NotNull(newEntityDesign2);
                Assert.NotNull(newEntityDesign3);
                Assert.NotNull(newEntityDesign4);

                {
                    var component = newEntityDesign2.Entity.Get<TestEntityComponent>();
                    Assert.NotNull(component);
                    Assert.AreEqual(newEntityDesign1.Entity, component.EntityLink);
                }

                {
                    var component = newEntityDesign4.Entity.Get<TestEntityComponent>();
                    Assert.NotNull(component);
                    Assert.AreEqual(newEntityDesign3.Entity.Transform, component.EntityComponentLink);
                }
            }
        }
    }
}