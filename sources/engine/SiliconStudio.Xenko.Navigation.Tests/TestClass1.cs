using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Navigation.Tests
{
    public class NavigationTest : GameTestBase
    {
        private Entity entityA;
        private Entity entityB;
        private PlayerController controllerA;
        private PlayerController controllerB;

        private Entity filterB;
        private Entity filterAB;

        private Vector3 targetPosition = new Vector3(1.4f, 0.0f, 0.0f);

        private DynamicNavigationMeshSystem dynamicNavigation;

        public NavigationTest()
        {
            AutoLoadDefaultSettings = true;
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            entityA = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "A");
            entityB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "B");

            entityA.Add(controllerA = new PlayerController());
            entityB.Add(controllerB = new PlayerController());

            filterAB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "FilterAB");
            filterB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "FilterB");

            dynamicNavigation = (DynamicNavigationMeshSystem)GameSystems.FirstOrDefault(x => x is DynamicNavigationMeshSystem);
            if (dynamicNavigation == null)
                throw new Exception("Failed to find dynamic navigation mesh system");

            dynamicNavigation.Enabled = true;
            dynamicNavigation.AutomaticRebuild = false;

            Script.AddTask(RunAsyncTests);
        }

        private async Task RunAsyncTests()
        {
            await Script.NextFrame();

            // Enabled a wall that blocks A and B
            RecursiveToggle(filterAB, true);
            RecursiveToggle(filterB, false);
            var buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);
            Assert.AreEqual(2, buildResult.UpdatedLayers.Count);

            await Task.WhenAll(controllerA.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }),
                controllerB.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            await Reset();

            // Enabled a wall that only blocks B
            RecursiveToggle(filterAB, false);
            RecursiveToggle(filterB, true);
            buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);

            await Task.WhenAll(controllerA.TryMove(targetPosition).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            Exit();
        }

        private async Task Reset()
        {
            controllerA.Reset();
            controllerB.Reset();
            await Script.NextFrame();
        }

        private void RecursiveToggle(Entity entity, bool enabled)
        {
            var model = entity.Get<ModelComponent>();
            if (model != null)
                model.Enabled = enabled;
            var collider = entity.Get<StaticColliderComponent>();
            if (collider != null)
                collider.Enabled = enabled;

            foreach (var c in entity.GetChildren())
                RecursiveToggle(c, enabled);
        }

        [Test]
        public static void Main()
        {
            NavigationTest game = new NavigationTest();
            game.Run();
        }
    }
}