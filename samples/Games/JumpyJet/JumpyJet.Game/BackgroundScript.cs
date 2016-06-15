using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Graphics;

namespace JumpyJet
{
    /// <summary>
    /// The script in charge of creating and updating the background.
    /// </summary>
    public class BackgroundScript : SyncScript
    {
        // Entities' depth
        private const int Pal0Depth = 0;
        private const int Pal1Depth = 1;
        private const int Pal2Depth = 2;
        private const int Pal3Depth = 3;

        public SpriteSheet ParallaxBackgrounds;

        private SpriteBatch spriteBatch;

        private readonly List<BackgroundSection> backgroundParallax = new List<BackgroundSection>();

        private SceneDelegateRenderer delegateRenderer;

        public override void Start()
        {
            var virtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 20f);

            // Create Parallax Background
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[0], virtualResolution, GameScript.GameSpeed / 4f, Pal0Depth));
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[1], virtualResolution, GameScript.GameSpeed / 3f, Pal1Depth));
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[2], virtualResolution, GameScript.GameSpeed / 1.5f, Pal2Depth));

            // For pal3Sprite: Ground, move it downward so that its bottom edge is at the bottom screen.
            var screenHeight = virtualResolution.Y;
            var pal3Height = ParallaxBackgrounds.Sprites[3].SizeInPixels.Y;
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[3], virtualResolution, GameScript.GameSpeed, Pal3Depth, Vector2.UnitY * (screenHeight - pal3Height) / 2));
            
            // allocate the sprite batch in charge of drawing the backgrounds.
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };

            // register the renderer in the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Insert(1, delegateRenderer = new SceneDelegateRenderer(DrawParallax));
        }

        public override void Update()
        {
            var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Update Parallax backgrounds
            foreach (var parallax in backgroundParallax)
                parallax.Update(elapsedTime);
        }

        public override void Cancel()
        {
            // remove the delegate renderer from the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Remove(delegateRenderer);

            // free graphic objects
            spriteBatch.Dispose();
        }

        public void DrawParallax(RenderDrawContext context, RenderFrame frame)
        {
            spriteBatch.Begin(context.GraphicsContext);

            foreach (var pallaraxBackground in backgroundParallax)
                pallaraxBackground.DrawSprite(spriteBatch);

            spriteBatch.End();
        }

        public void StartScrolling()
        {
            EnableAllParallaxesUpdate(true);
        }

        public void StopScrolling()
        {
            EnableAllParallaxesUpdate(false);
        }

        private void EnableAllParallaxesUpdate(bool isEnable)
        {
            foreach (var pallarax in backgroundParallax)
            {
                pallarax.IsUpdating = isEnable;
            }
        }
    }
}