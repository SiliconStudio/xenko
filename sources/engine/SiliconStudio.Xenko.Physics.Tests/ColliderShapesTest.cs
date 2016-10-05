using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

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
                game.ScreenShotAutomationEnabled = false;

                await game.Script.NextFrame();
                await game.Script.NextFrame();
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

                var cylinder = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "CylinderPrefab1");

                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.55f, 0.0f), cylinder.Transform.Position + new Vector3(0.0f, 0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.5f, 0.0f), cylinder.Transform.Position + new Vector3(0.0f, 0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.55f, 0.0f), cylinder.Transform.Position + new Vector3(0.0f, -0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.5f, 0.0f), cylinder.Transform.Position + new Vector3(0.0f, -0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.55f, 0.0f, 0.0f), cylinder.Transform.Position + new Vector3(0.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.5f, 0.0f, 0.0f), cylinder.Transform.Position + new Vector3(0.5f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.55f, 0.0f, 0.0f), cylinder.Transform.Position + new Vector3(-0.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cylinder.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.5f, 0.0f, 0.0f), cylinder.Transform.Position + new Vector3(-0.5f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                var capsule = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "CapsulePrefab1");

                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.65f, 0.0f), capsule.Transform.Position + new Vector3(0.0f, 0.65f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.6f, 0.0f), capsule.Transform.Position + new Vector3(0.0f, 0.6f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.65f, 0.0f), capsule.Transform.Position + new Vector3(0.0f, -0.65f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.6f, 0.0f), capsule.Transform.Position + new Vector3(0.0f, -0.6f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.40f, 0.0f, 0.0f), capsule.Transform.Position + new Vector3(0.40f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.35f, 0.0f, 0.0f), capsule.Transform.Position + new Vector3(0.35f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.40f, 0.0f, 0.0f), capsule.Transform.Position + new Vector3(-0.40f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((capsule.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.35f, 0.0f, 0.0f), capsule.Transform.Position + new Vector3(-0.35f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                var cone = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "ConePrefab1");

                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.55f, 0.0f), cone.Transform.Position + new Vector3(0.0f, 0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 0.5f, 0.0f), cone.Transform.Position + new Vector3(0.0f, 0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.55f, 0.0f), cone.Transform.Position + new Vector3(0.0f, -0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.5f, 0.0f), cone.Transform.Position + new Vector3(0.0f, -0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.35f, 0.0f, 0.0f), cone.Transform.Position + new Vector3(0.35f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.3f, 0.0f, 0.0f), cone.Transform.Position + new Vector3(0.3f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.35f, 0.0f, 0.0f), cone.Transform.Position + new Vector3(-0.35f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((cone.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.3f, 0.0f, 0.0f), cone.Transform.Position + new Vector3(-0.3f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                var compound1 = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "Compound1");

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 1.55f, 0.0f), compound1.Transform.Position + new Vector3(0.0f, 1.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 1.49f, 0.0f), compound1.Transform.Position + new Vector3(0.0f, 1.49f, 0.0f)); //compound margin is different
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.55f, 0.0f), compound1.Transform.Position + new Vector3(0.0f, -0.55f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.5f, 0.0f), compound1.Transform.Position + new Vector3(0.0f, -0.5f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(1.55f, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(1.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(1.49f, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(1.49f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.55f, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(-0.55f, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.5f, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(-0.5f, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                var scaling = new Vector3(3, 2, 2);

                compound1.Transform.Scale = scaling;
                compound1.Transform.UpdateWorldMatrix();
                compound1.Get<PhysicsComponent>().UpdatePhysicsTransformation();

                await game.Script.NextFrame();

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 1.55f * 2, 0.0f), compound1.Transform.Position + new Vector3(0.0f, 1.55f * 2, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, 1.49f * 2, 0.0f), compound1.Transform.Position + new Vector3(0.0f, 1.49f * 2, 0.0f)); //compound margin is different
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.55f * 2, 0.0f), compound1.Transform.Position + new Vector3(0.0f, -0.55f * 2, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(0.0f, -0.5f * 2, 0.0f), compound1.Transform.Position + new Vector3(0.0f, -0.5f * 2, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(1.55f * 3, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(1.55f * 3, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(1.49f * 3, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(1.49f * 3, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.55f * 3, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(-0.55f * 3, 0.0f, 0.0f));
                Assert.IsFalse(hit.Succeeded);
                hit = simulation.Raycast((compound1.Transform.Position - Vector3.UnitZ * 2) + new Vector3(-0.5f * 3, 0.0f, 0.0f), compound1.Transform.Position + new Vector3(-0.5f * 3, 0.0f, 0.0f));
                Assert.IsTrue(hit.Succeeded);

                game.Exit();
            });
            RunGameTest(game);
        }
    }
}
