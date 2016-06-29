using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public abstract class SceneBaseFactory : AssetFactory<SceneAsset>
    {
        public const string SkyboxEntityName = "Skybox";
        public const string CameraEntityName = "Camera";
        public const string SunEntityName = "Directional light";

        protected static SceneAsset CreateBase(float skyIntensity, float sunIntensity)
        {
            // Create the skybox
            var skyboxEntity = new Entity(SkyboxEntityName)
            {
                new SkyboxComponent { Intensity = skyIntensity },
            };
            skyboxEntity.Transform.Position = new Vector3(0.0f, 2.0f, -2.0f);

            // Create default camera
            var cameraEntity = new Entity(CameraEntityName) { new CameraComponent { Projection = CameraProjectionMode.Perspective } };
            cameraEntity.Transform.Position = new Vector3(-1.0f, 1.2f, 2.7f);
            cameraEntity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-10)) * Quaternion.RotationY(MathUtil.DegreesToRadians(-20));

            // Create default light (with shadows)
            var lightEntity = new Entity(SunEntityName) { new LightComponent
            {
                Intensity = sunIntensity,
                Type = new LightDirectional
                {
                    Shadow =
                    {
                        Enabled = true,
                        Size = LightShadowMapSize.XLarge,
                        Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter5x5 },
                    }
                }
            } };
            lightEntity.Transform.Position = new Vector3(0, 2.0f, 0);
            lightEntity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-70)) * Quaternion.RotationY(MathUtil.DegreesToRadians(30));

            var sceneAsset = new SceneAsset
            {
                SceneSettings =
                {
                    // Setup Graphics Compositor
                    GraphicsCompositor = new SceneGraphicsCompositorLayers
                    {
                        Cameras =
                        {
                            cameraEntity.Get<CameraComponent>(),
                        },
                    }
                }
            };

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(cameraEntity));
            sceneAsset.Hierarchy.RootPartIds.Add(cameraEntity.Id);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(lightEntity));
            sceneAsset.Hierarchy.RootPartIds.Add(lightEntity.Id);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(skyboxEntity));
            sceneAsset.Hierarchy.RootPartIds.Add(skyboxEntity.Id);

            return sceneAsset;
        }
    }

    public class SceneLDRFactory : SceneBaseFactory
    {
        private const string AmbientEntityName = "Ambient light";
        private const float SkyIntensity = 1.0f;
        private const float AmbientIntensity = 0.05f;
        private const float SuntIntensity = 0.8f;

        public static SceneAsset Create()
        {
            var sceneAsset = CreateBase(SkyIntensity, SuntIntensity);

            // Add an ambient light
            var ambientLight = new Entity(AmbientEntityName)
                {
                    new LightComponent
                    {
                        Intensity = AmbientIntensity,
                        Type = new LightAmbient { Color = new ColorRgbProvider(new Color(196, 215, 255)) }
                    }
                };
            ambientLight.Transform.Position = new Vector3(-2.0f, 2.0f, 0.0f);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(ambientLight));
            sceneAsset.Hierarchy.RootPartIds.Add(ambientLight.Id);

            // Clear and render scene
            var graphicsCompositorLayers = (SceneGraphicsCompositorLayers)sceneAsset.SceneSettings.GraphicsCompositor;
            graphicsCompositorLayers.Master.Add(new ClearRenderFrameRenderer());
            graphicsCompositorLayers.Master.Add(new SceneCameraRenderer());

            return sceneAsset;
        }

        public override SceneAsset New()
        {
            return Create();
        }
    }

    public class SceneHDRFactory : SceneBaseFactory
    {
        private const float SkyIntensity = 3.0f;
        private const float SunIntensity = 5.0f;

        public static SceneAsset Create()
        {
            var sceneAsset = CreateBase(SkyIntensity, SunIntensity);

            // Add a sky light to the scene
            var skyboxEntity = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).Single(x => x.Name == SkyboxEntityName);
            skyboxEntity.Add(new LightComponent
            {
                Intensity = 0.25f,
                Type = new LightSkybox(),
            });

            // Switch editor settings to HDR mode
            var hdrSettings = new SceneEditorGraphicsModeHDRSettings();
            hdrSettings.PostProcessingEffects.DisableAll();

            // enable only the tonemap
            hdrSettings.PostProcessingEffects.ColorTransforms.Enabled = true;
            hdrSettings.PostProcessingEffects.ColorTransforms.Transforms.Add(new ToneMap());
            hdrSettings.PostProcessingEffects.ColorTransforms.Transforms.Add(new FilmGrain { Enabled = false });
            hdrSettings.PostProcessingEffects.ColorTransforms.Transforms.Add(new Vignetting { Enabled = false });
            sceneAsset.SceneSettings.EditorSettings.Mode = hdrSettings;

            // Add separate layer for rendering to HDR
            var graphicsCompositorLayers = (SceneGraphicsCompositorLayers)sceneAsset.SceneSettings.GraphicsCompositor;
            graphicsCompositorLayers.Layers.Add(new SceneGraphicsLayer
            {
                Output = new LocalRenderFrameProvider { Descriptor = { Format = RenderFrameFormat.HDR } },
                Renderers =
                    {
                        new ClearRenderFrameRenderer(),
                        new SceneCameraRenderer(),
                    }
            });

            // Also add post effects
            graphicsCompositorLayers.Master.Add(new SceneEffectRenderer
            {
                Effect = new PostProcessingEffects
                {
                    // Disable DoF and setup default tone mapping
                    DepthOfField = { Enabled = false },
                    ColorTransforms =
                        {
                            Transforms =
                            {
                                new ToneMap(),
                                new FilmGrain { Enabled = false },
                                new Vignetting { Enabled =  false },
                            }
                        }
                }
            });

            return sceneAsset;
        }

        public override SceneAsset New()
        {
            return Create();
        }
    }
}
