// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    class TestEntitySerializer
    {
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
        [Ignore]
#endif
        [Test]
        public void TestSaveAndLoadEntities()
        {
            var assetManager = new AssetManager();

            var entity = new Entity();
            entity.Transformation.Translation = new Vector3(100.0f, 0.0f, 0.0f);
            assetManager.Save("EntityAssets/Entity", entity);

            GC.Collect();

            var entity2 = assetManager.Load<Entity>("EntityAssets/Entity");
        }
    }
}
