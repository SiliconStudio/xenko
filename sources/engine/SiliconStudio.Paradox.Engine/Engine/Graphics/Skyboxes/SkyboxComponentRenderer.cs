// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Engine.Graphics.Skyboxes
{
    /// <summary>
    /// A renderer for a skybox.
    /// </summary>
    public class SkyboxComponentRenderer : EntityComponentRendererBase
    {
        private ImageEffectShader skyboxEffect;
        private SkyboxProcessor skyboxProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponentRenderer" /> class.
        /// </summary>
        public SkyboxComponentRenderer()
        {
        }

        public override void Load(RenderContext context)
        {
            base.Load(context);

            skyboxProcessor = SceneInstance.GetProcessor<SkyboxProcessor>();
            skyboxEffect = new ImageEffectShader("SkyboxEffect");
            skyboxEffect.Load(context);
        }

        public override void Unload()
        {
            base.Unload();

            skyboxEffect.Dispose();
        }

        protected override void DrawCore(RenderContext context)
        {
            var skybox = skyboxProcessor.ActiveSkyboxBackground;

            if (skybox != null)
            {
                // Copy camera/pass parameters
                context.Parameters.CopySharedTo(skyboxEffect.Parameters);

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

                skyboxEffect.SetOutput(CurrentRenderFrame.RenderTarget);
                skyboxEffect.Draw();

                // Make sure to fully restore the current render frame
                CurrentRenderFrame.Activate(context);
            }
        }
    }
}