// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A renderer for a skybox.
    /// </summary>
    public class SkyboxBackgroundRenderer : Renderer
    {
        private ImageEffectShader skyboxEffect;
        private readonly SkyboxProcessor skyboxProcessor;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxBackgroundRenderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public SkyboxBackgroundRenderer(IServiceRegistry services)
            : base(services)
        {
            throw new NotImplementedException("TODO: UPDATE TO USE Scene and Graphics Composer");
            //skyboxProcessor = EntitySystem.GetProcessor<SkyboxProcessor>();
        }

        public Texture Target { get; set; }

        public override void Load()
        {
            base.Load();

            skyboxEffect = new ImageEffectShader("SkyboxEffect");
            skyboxEffect.Initialize(DrawEffectContext.GetShared(Services));
        }

        public override void Unload()
        {
            base.Unload();

            skyboxEffect.Dispose();
        }

        protected override void OnRendering(RenderContext context)
        {
            if (skyboxProcessor == null)
            {
                return;
            }

            var skybox = skyboxProcessor.ActiveSkyboxBackground;

            if (skybox != null)
            {
                // Copy camera/pass parameters
                context.CurrentPass.Parameters.CopySharedTo(skyboxEffect.Parameters);

                // Show irradiance in the background
                if (skybox.Background == SkyboxBackground.Irradiance)
                {
                    foreach (var parameterKeyValue in skybox.Skybox.DiffuseLightingParameters)
                    {
                        if (parameterKeyValue.Key == SkyboxKeys.Shader)
                        {
                            skyboxEffect.Parameters.Set(SkyboxKeys.Shader, (ShaderSource)parameterKeyValue.Value);
                        }
                        else
                        {
                            skyboxEffect.Parameters.SetObject(parameterKeyValue.Key.ComposeWith("skyboxColor"), parameterKeyValue.Value);
                        }
                    }
                }
                else
                {
                    // TODO: Should we better use composition on "skyboxColor" for parameters?

                    // Copy Skybox parameters
                    if (skybox.Skybox != null)
                    {
                        foreach (var parameterKeyValue in skybox.Skybox.Parameters)
                        {
                            if (parameterKeyValue.Key == SkyboxKeys.Shader)
                            {
                                skyboxEffect.Parameters.Set(SkyboxKeys.Shader, (ShaderSource)parameterKeyValue.Value);
                            }
                            else
                            {
                                skyboxEffect.Parameters.SetObject(parameterKeyValue.Key, parameterKeyValue.Value);
                            }
                        }
                    }
                }

                // Setup the intensity
                skyboxEffect.Parameters.Set(SkyboxKeys.Intensity, skybox.Intensity);
                    
                // Setup the rotation
                skyboxEffect.Parameters.Set(SkyboxKeys.SkyMatrix, Matrix.RotationQuaternion(skybox.Entity.Transform.Rotation));

                skyboxEffect.SetOutput(Target ?? GraphicsDevice.BackBuffer);
                skyboxEffect.Draw();
            }
        }
    }
}