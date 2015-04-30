// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Paradox.Assets.Entities;
using SiliconStudio.Paradox.Assets.Model;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestEntity
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
            AssetRegistry.RegisterAssembly(typeof(ModelAsset).Assembly);
        }

        [Test]
        public void TestEntitySerialization()
        {
            var entityAsset = new EntityAsset();

            var entity1 = new Entity { Id = Guid.NewGuid() };
            var entity2 = new Entity { Id = Guid.NewGuid() };

            entity1.Transform.Children.Add(entity2.Transform);

            entityAsset.Hierarchy.Entities.Add(entity1);
            entityAsset.Hierarchy.Entities.Add(entity2);

            using (var stream = new MemoryStream())
            {
                AssetSerializer.Save(stream, entityAsset);

                stream.Position = 0;
                var serializedVersion = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine(serializedVersion);

                stream.Position = 0;
                var entityAsset2 = AssetSerializer.Load(stream, "pdxentity");
            }
        }
    }
}