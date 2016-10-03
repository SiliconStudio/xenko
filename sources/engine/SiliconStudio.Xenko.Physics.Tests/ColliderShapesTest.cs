using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Tests
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
            PerformTest(game =>
            {
                var camera = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "Camera").Get<CameraComponent>();
                camera.Update();
                var simulation = game.SceneSystem.SceneInstance.Scene.Entities.First(ent => ent.Name == "Simulation").Get<StaticColliderComponent>().Simulation;
                Assert.IsFalse(ScreenPositionToWorldPositionRaycast(new Vector2(0.5164062f, 0.4236111f), camera, simulation));
                Assert.IsTrue(ScreenPositionToWorldPositionRaycast(new Vector2(0.5195313f, 0.4152778f), camera, simulation));
                game.DebugConsoleSystem.Print("Test", new Vector2());
            });
        }
    }
}
