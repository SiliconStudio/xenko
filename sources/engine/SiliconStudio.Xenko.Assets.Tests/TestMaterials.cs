// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Tests
{
    /// <summary>
    /// Test serialization for MaterialNull.
    /// </summary>
    [TestFixture]
    public class TestMaterials
    {
        /// <summary>
        /// Test serialization for <see cref="MaterialNull"/>
        /// </summary>
        [Test]
        public void Test()
        {
            var prefab = new PrefabAsset();

            var modelComponent = new ModelComponent();
            var entity = new Entity()
            {
                modelComponent
            };
            prefab.Hierarchy.Parts.Add(new EntityDesign(entity));
            prefab.Hierarchy.RootPartIds.Add(entity.Id);

            var material1 = new MaterialNull();
            IdentifiableHelper.SetId(material1, new Guid("39E2B226-8752-4678-8E93-76FFBFBA337B"));
            var material2 = new MaterialNull();
            IdentifiableHelper.SetId(material2, new Guid("CC4F1B31-FBB7-4360-A3E7-060BDFDA0695"));
            modelComponent.Materials.Add(material1);
            modelComponent.Materials.Add(material2);

            Action<PrefabAsset> checkPrefab = (newPrefab) =>
            {
                var previousEntityDesign = newPrefab.Hierarchy.Parts.FirstOrDefault();

                Assert.NotNull(previousEntityDesign);

                var previousEntity = previousEntityDesign.Entity;


                var component = previousEntity.Get<ModelComponent>();
                Assert.NotNull(component);

                Assert.AreEqual(2, component.Materials.Count);

                var newMaterial1 = component.Materials[0];
                Assert.AreEqual(IdentifiableHelper.GetId(material1), IdentifiableHelper.GetId(newMaterial1));
                var newMaterial2 = component.Materials[1];
                Assert.AreEqual(IdentifiableHelper.GetId(material2), IdentifiableHelper.GetId(newMaterial2));
            };

            // Test yaml serialization
            {
                using (var stream = new MemoryStream())
                {
                    AssetSerializer.Save(stream, prefab);

                    stream.Position = 0;
                    var serializedVersion = Encoding.UTF8.GetString(stream.ToArray());
                    Console.WriteLine(serializedVersion);

                    stream.Position = 0;
                    var newPrefab = (PrefabAsset)AssetSerializer.Load(stream, "myentity");
                    checkPrefab(newPrefab);
                }
            }

            // Test cloning
            var newPrefabClone = (PrefabAsset)AssetCloner.Clone(prefab);
            checkPrefab(newPrefabClone);

            // Test runtime serialization (runtime serialization is removing MaterialNull and replacing it by a null)
            {
                var stream = new MemoryStream();
                var writer = new BinarySerializationWriter(stream) { Context = { SerializerSelector = SerializerSelector.AssetWithReuse } };
                writer.SerializeExtended(entity, ArchiveMode.Serialize);
                writer.Flush();
                stream.Position = 0;

                var reader = new BinarySerializationReader(stream) { Context = { SerializerSelector = SerializerSelector.AssetWithReuse } };

                Entity newEntity = null;
                reader.SerializeExtended(ref newEntity, ArchiveMode.Deserialize);

                Assert.NotNull(newEntity);

                var component = newEntity.Get<ModelComponent>();
                Assert.NotNull(component);

                Assert.AreEqual(2, component.Materials.Count);

                Assert.Null(component.Materials[0]);
                Assert.Null(component.Materials[1]);
            }
        }
    }
}
