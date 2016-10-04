using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Tests;

namespace SiliconStudio.Xenko.Physics.Tests
{
    public class ColliderShapesTest : GameTest
    {
        public ColliderShapesTest() : base("ColliderShapesTest")
        {
        }

        public static bool ScreenPositionToWorldPositionRaycast(Vector2 screenPos, CameraComponent camera, Simulation simulation)
        {
            var invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

            Vector3 sPos;
            sPos.X = screenPos.X * 2f - 1f;
            sPos.Y = 1f - screenPos.Y * 2f;

            sPos.Z = 0f;
            var vectorNear = Vector3.Transform(sPos, invViewProj);
            vectorNear /= vectorNear.W;

            sPos.Z = 1f;
            var vectorFar = Vector3.Transform(sPos, invViewProj);
            vectorFar /= vectorFar.W;

            var result = simulation.RaycastPenetrating(vectorNear.XYZ(), vectorFar.XYZ());
            foreach (var hitResult in result)
            {
                var staticBody = hitResult.Collider as StaticColliderComponent;
                if (hitResult.Succeeded)
                {
                    return true;
                }                
            }

            return false;
        }

        [Test]
        public void ColliderShapesTest1()
        {
            var game = new ColliderShapesTest();
            game.Script.AddTask(async () =>
            {
                await game.Script.NextFrame();
                await game.Script.NextFrame();
                await game.Script.NextFrame();
                await game.Script.NextFrame();
                var camera = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "Camera").Get<CameraComponent>();
                var simulation = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "Simulation").Get<StaticColliderComponent>().Simulation;

                HitResult hit;

                var cube = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "CubePrefab1");

                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.55f, 0.0f), cube.Transform.Position + new Vector3(0.0f, 0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.5f, 0.0f), cube.Transform.Position + new Vector3(0.0f, 0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.55f, 0.0f), cube.Transform.Position + new Vector3(0.0f, -0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.5f, 0.0f), cube.Transform.Position + new Vector3(0.0f, -0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.55f, 0.0f, 0.0f), cube.Transform.Position + new Vector3(0.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.5f, 0.0f, 0.0f), cube.Transform.Position + new Vector3(0.5f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.55f, 0.0f, 0.0f), cube.Transform.Position + new Vector3(-0.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cube.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.5f, 0.0f, 0.0f), cube.Transform.Position + new Vector3(-0.5f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                //simulation.ColliderShapesRendering = true;
                //Cube
                //                Assert.IsFalse(ScreenPositionToWorldPositionRaycast(new Vector2(0.35625f, 0.4347222f), camera, simulation));
                //                Assert.IsFalse(ScreenPositionToWorldPositionRaycast(new Vector2(0.4039063f, 0.5333334f), camera, simulation));
                //                Assert.IsFalse(ScreenPositionToWorldPositionRaycast(new Vector2(0.359375f, 0.5652778f), camera, simulation));
                //                Assert.IsFalse(ScreenPositionToWorldPositionRaycast(new Vector2(0.3195313f, 0.5027778f), camera, simulation));
                //                Assert.IsTrue(ScreenPositionToWorldPositionRaycast(new Vector2(0.3585938f, 0.4361111f), camera, simulation));
                //                Assert.IsTrue(ScreenPositionToWorldPositionRaycast(new Vector2(0.403125f, 0.5069444f), camera, simulation));
                //                Assert.IsTrue(ScreenPositionToWorldPositionRaycast(new Vector2(0.365625f, 0.5625f), camera, simulation));
                //                Assert.IsTrue(ScreenPositionToWorldPositionRaycast(new Vector2(0.3210937f, 0.5055556f), camera, simulation));

                game.DebugConsoleSystem.Print("Test", new Vector2());
            });
            RunGameTest(game);
        }
    }
}
