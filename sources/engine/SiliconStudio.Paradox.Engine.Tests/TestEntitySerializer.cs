// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    class TestEntitySerializer
    {
        /// <summary>
        /// Initializes the asset database. Similar to <see cref="Game.InitializeAssetDatabase"/>, but accessible without using internals.
        /// </summary>
        private static void InitializeAssetDatabase()
        {
            // Create and mount database file system
            var objDatabase = new ObjectDatabase("/data/db", "index", "/local/db");

            // Only set a mount path if not mounted already
            var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
            var databaseFileProvider = new DatabaseFileProvider(objDatabase, mountPath);

            AssetManager.GetFileProvider = () => databaseFileProvider;
        }

#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
        [Ignore]
#endif
        [Test]
        public void TestSaveAndLoadEntities()
        {
            InitializeAssetDatabase();
            var assetManager = new AssetManager();

            var entity = new Entity();
            entity.Transformation.Translation = new Vector3(100.0f, 0.0f, 0.0f);
            assetManager.Save("EntityAssets/Entity", entity);

            GC.Collect();

            var entity2 = assetManager.Load<Entity>("EntityAssets/Entity");
            Assert.AreEqual(entity.Transformation.Translation, entity2.Transformation.Translation);
        }
    }
}
