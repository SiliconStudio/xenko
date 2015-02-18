// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Effects.Tessellation;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [ReferenceToEffects]
    public class TestTesselation : TestGameBase
    {
        private List<Entity> models = new List<Entity>();
        private List<Material> materials = new List<Material>();

        private Entity currentModel;
        private Material currentMaterial;

        private int currentModelIndex;

        private TestCamera camera;

        private int currentMaterialIndex;

        private bool isWireframe;

        private RasterizerState wireframeState;

        private SpriteBatch spriteBatch;

        private SpriteFont font;

        public TestTesselation()
        {
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Asset.Load<SpriteFont>("Font");

            wireframeState = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe });

            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.White});
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "ParadoxBaseShader"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderDebugInfo } );

            models.Add(Asset.Load<Entity>("Cube/cube"));

            materials.Add(Asset.Load<Material>("NoTessellation"));
            materials.Add(Asset.Load<Material>("FlatTessellation"));
            materials.Add(Asset.Load<Material>("FlatTessellationAET"));
            materials.Add(Asset.Load<Material>("PNTessellation"));
            materials.Add(Asset.Load<Material>("PNTessellationAEP"));

            Script.Add(camera = new TestCamera(Services));

            LightingKeys.EnableFixedAmbientLight(GraphicsDevice.Parameters, true);
            GraphicsDevice.Parameters.Set(EnvironmentLightKeys.GetParameterKey(LightSimpleAmbientKeys.AmbientLight, 0), (Color3)Color.White);

            ChangeModel(0);
            ChangeMaterial(0);
        }

        private void RenderDebugInfo(RenderContext obj)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Desired triangle size: {0}".ToFormat(currentMaterial.Parameters.Get(TessellationKeys.DesiredTriangleSize)), new Vector2(0), Color.Black);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.Up))
                ChangeModel(-1);

            if (Input.IsKeyPressed(Keys.Down))
                ChangeModel(1);

            if (Input.IsKeyPressed(Keys.Left))
                ChangeMaterial(-1);

            if (Input.IsKeyPressed(Keys.Right))
                ChangeMaterial(1);

            if (Input.IsKeyDown(Keys.NumPad1))
                ChangeDesiredTriangleSize(-0.2f);

            if (Input.IsKeyDown(Keys.NumPad2))
                ChangeDesiredTriangleSize(0.2f);

            if (Input.IsKeyPressed(Keys.Space))
            {
                isWireframe = !isWireframe;
                currentMaterial.Parameters.Set(Effect.RasterizerStateKey, isWireframe ? wireframeState : GraphicsDevice.RasterizerStates.CullBack);
            }
        }

        private void ChangeDesiredTriangleSize(float f)
        {
            var oldValue = currentMaterial.Parameters.Get(TessellationKeys.DesiredTriangleSize);
            currentMaterial.Parameters.Set(TessellationKeys.DesiredTriangleSize, oldValue + f);
        }

        private void ChangeModel(int offset)
        {
            currentModelIndex = (currentModelIndex + offset + models.Count) % models.Count;
            currentModel = models[currentModelIndex];

            Entities.Clear();
            Entities.Add(currentModel);

            camera.SetTarget(currentModel, true);
        }

        private void ChangeMaterial(int i)
        {
            currentMaterialIndex = ((currentMaterialIndex + i + materials.Count) % materials.Count);
            currentMaterial = materials[currentMaterialIndex];

            var modelComponent = currentModel.Get<ModelComponent>();
            modelComponent.Materials.Clear();
            modelComponent.Materials.Add(currentMaterial);
        }

        static public void Main()
        {
            using (var game = new TestTesselation())
            {
                game.Run();
            }
        }
    }
}