using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Graphics;

namespace CustomEffect
{
    /// <summary>
    /// The script in charge of rendering the custom effect
    /// </summary>
    public class EffectScript : StartupScript
    {
        private Effect customEffect;
        private SpriteBatch spriteBatch;
        private EffectInstance customEffectInstance;
        private SceneDelegateRenderer renderer;
        private SamplerState samplerState;

        public Texture Logo;

        public override void Start()
        {
            base.Start();

            customEffect = EffectSystem.LoadEffect("Effect").WaitForResult();
            customEffectInstance = new EffectInstance(customEffect);

            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = new Vector3(1) };

            // set fixed parameters once
            customEffectInstance.Parameters.Set(TexturingKeys.Sampler, samplerState);
            customEffectInstance.Parameters.Set(EffectKeys.Center, new Vector2(0.5f, 0.5f));
            customEffectInstance.Parameters.Set(EffectKeys.Frequency, 40);
            customEffectInstance.Parameters.Set(EffectKeys.Spread, 0.5f);
            customEffectInstance.Parameters.Set(EffectKeys.Amplitude, 0.015f);
            customEffectInstance.Parameters.Set(EffectKeys.InvAspectRatio, GraphicsDevice.Presenter.BackBuffer.Height / (float)GraphicsDevice.Presenter.BackBuffer.Width);

            // NOTE: Linear-Wrap sampling is not available for non-square non-power-of-two textures on opengl es 2.0
            samplerState = SamplerState.New(GraphicsDevice, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Clamp));
            
            // Add Effect rendering to the end of the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            renderer = new SceneDelegateRenderer(RenderQuad);
            compositor.Master.Renderers.Add(renderer);
        }

        public override void Cancel()
        {
            // Cleanup when script is removed
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Remove(renderer);

            base.Cancel();
        }

        private void RenderQuad(RenderDrawContext renderContext, RenderFrame frame)
        {
            customEffectInstance.Parameters.Set(EffectKeys.Phase, -3 * (float)Game.UpdateTime.Total.TotalSeconds);

            spriteBatch.Begin(renderContext.GraphicsContext, blendState: BlendStates.NonPremultiplied, depthStencilState: DepthStencilStates.None, effect: customEffectInstance);
            spriteBatch.Draw(Logo, new RectangleF(0, 0, 1, 1), Color.White);
            spriteBatch.End();
        }
    }
}