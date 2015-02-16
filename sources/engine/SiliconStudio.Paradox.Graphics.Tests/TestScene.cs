using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.ProceduralModels;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.Engine.Graphics.Materials;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestScene : TestGameBase
    {
        private Entity cubeEntity;

        public TestScene()
        {
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Instantiate a scene with a single entity and model component
            var scene = new Scene();

            // Create a cube entity
            cubeEntity = new Entity();

            // Create a procedural model with a diffuse material
            var model = new Model();
            var material = Material.New(
                new MaterialDescriptor
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.White)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature()
                    }
                });
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new CubeProceduralModel());
            modelDescriptor.GenerateModel(Services, model);

            // Add the cube to the scene
            scene.AddChild(cubeEntity);

            // Create a camera entity and add it to the scene
            var cameraEntity = new Entity { new CameraComponent() };
            cameraEntity.Transform.Position = new Vector3(0, 0, 5);
            scene.AddChild(cameraEntity);

            // Create a light
            var lightEntity = new Entity()
            {
                new LightComponent()
            };

            lightEntity.Transform.Position = new Vector3(0, 0, 1);
            lightEntity.Transform.Rotation = Quaternion.RotationY(MathUtil.DegreesToRadians(45));
            scene.AddChild(lightEntity);

            // Create a graphics compositor
            var graphicsCompositor = new SceneGraphicsCompositorLayers();
            graphicsCompositor.Master.Renderers.Add(new ClearRenderFrameRenderer());
            graphicsCompositor.Master.Renderers.Add(new SceneCameraRenderer()
            {
                Camera = cameraEntity.Get<CameraComponent>()
            });

            // Use this graphics compositor
            scene.Settings.GraphicsCompositor = graphicsCompositor;

            // Create a scene instance
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var time = (float)gameTime.Total.TotalSeconds;
            cubeEntity.Transform.Rotation = Quaternion.RotationY(time) * Quaternion.RotationX(time * 0.5f);

            //if(!ScreenShotAutomationEnabled)
            //    DrawCustomEffect();
        }

        //private void DrawCustomEffect()
        //{
        //    GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
        //    GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
        //    GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

        //    effectParameters.Set(MyCustomShaderKeys.ColorFactor2, (Vector4)Color.Red);
        //    effectParameters.Set(CustomShaderKeys.SwitchEffectLevel, switchEffectLevel);
        //    effectParameters.Set(TexturingKeys.Texture0, UVTexture);
        //    // TODO: Add switch Effect to test and capture frames
        //    //switchEffectLevel++;
        //    dynamicEffectCompiler.Update(effectInstance, null);

        //    GraphicsDevice.DrawQuad(effectInstance.Effect, effectParameters);
        //}

        public static void Main()
        {
            using (var game = new TestScene())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunCustomEffect()
        {
            RunGameTest(new TestScene());
        }
    }
}