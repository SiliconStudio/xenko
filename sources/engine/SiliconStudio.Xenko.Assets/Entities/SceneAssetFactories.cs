// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Compositing;
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
                new BackgroundComponent { Intensity = skyIntensity },
            };
            skyboxEntity.Transform.Position = new Vector3(0.0f, 2.0f, -2.0f);

            // Create default camera
            var cameraEntity = new Entity(CameraEntityName) { new CameraComponent { Projection = CameraProjectionMode.Perspective } };
            cameraEntity.Transform.Position = new Vector3(2.6f, 0.6f, -1.0f);
            cameraEntity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(0)) * Quaternion.RotationY(MathUtil.DegreesToRadians(112.0f));

            // Create default light (with shadows)
            var lightEntity = new Entity(SunEntityName) { new LightComponent
            {
                Intensity = sunIntensity,
                Type = new LightDirectional
                {
                    Shadow =
                    {
                        Enabled = true,
                        Size = LightShadowMapSize.Large,
                        Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter5x5 },
                    }
                }
            } };
            lightEntity.Transform.Position = new Vector3(0, 2.0f, 0);
            lightEntity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-30.0f)) * Quaternion.RotationY(MathUtil.DegreesToRadians(-180.0f));

            var sceneAsset = new SceneAsset();

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(cameraEntity));
            sceneAsset.Hierarchy.RootParts.Add(cameraEntity);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(lightEntity));
            sceneAsset.Hierarchy.RootParts.Add(lightEntity);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(skyboxEntity));
            sceneAsset.Hierarchy.RootParts.Add(skyboxEntity);

            return sceneAsset;
        }
    }

    public class SceneLDRFactory : SceneBaseFactory
    {
        private const string AmbientEntityName = "Ambient light";
        private const float SkyIntensity = 1.0f;
        private const float AmbientIntensity = 0.1f;
        private const float SuntIntensity = 1.0f;

        public static SceneAsset Create()
        {
            var sceneAsset = CreateBase(SkyIntensity, SuntIntensity);

            // Add an ambient light
            var ambientLight = new Entity(AmbientEntityName)
                {
                    new LightComponent
                    {
                        Intensity = AmbientIntensity,
                        Type = new LightAmbient { Color = new ColorRgbProvider(Color.FromBgra(0xA5C9F0)) }
                    }
                };
            ambientLight.Transform.Position = new Vector3(-2.0f, 2.0f, 0.0f);

            sceneAsset.Hierarchy.Parts.Add(new EntityDesign(ambientLight));
            sceneAsset.Hierarchy.RootParts.Add(ambientLight);

            return sceneAsset;
        }

        public override SceneAsset New()
        {
            return Create();
        }
    }

    public class SceneHDRFactory : SceneBaseFactory
    {
        private const float SkyIntensity = 1.0f;
        private const float SunIntensity = 20.0f;

        public static SceneAsset Create()
        {
            var sceneAsset = CreateBase(SkyIntensity, SunIntensity);

            // Add a sky light to the scene
            var skyboxEntity = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).Single(x => x.Name == SkyboxEntityName);
            skyboxEntity.Add(new LightComponent
            {
                Intensity = 1.0f,
                Type = new LightSkybox(),
            });

            return sceneAsset;
        }

        public override SceneAsset New()
        {
            return Create();
        }
    }
}
