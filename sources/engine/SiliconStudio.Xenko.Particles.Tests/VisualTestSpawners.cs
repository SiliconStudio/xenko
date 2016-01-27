// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestSpawners : GameTest
    {
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var assetManager = Services.GetSafeServiceAs<AssetManager>();

            SceneSystem.SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>("Scene01"));
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestSpawners());
        }
    }
}
