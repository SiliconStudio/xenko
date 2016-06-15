using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;

namespace SimpleDynamicTexture
{
    /// <summary>
    /// The script in charge of updating the dynamic texture and displaying it.
    /// This sample shows how to create and write data to a texture on CPU side, and then use it for rendering.
    /// A 16*16 texture is created and stretched to cover a screen. You could tap on the screen to lit/dim a pixel of the texture.
    /// </summary>
    public class TextureUpdateScript : SyncScript
    {
        /// <summary>
        /// The size of the texture.
        /// </summary>
        private const int RenderTextureSize = 16;

        /// <summary>
        /// An array containing indices to draw a default shape to a texture
        /// </summary>
        private static readonly int[] SymmetricDefaultShape =
        {
            6, 1, 7, 1,
            0, 2, 5, 2, 6, 2, 7, 2,
            1, 3, 4, 3, 5, 3, 6, 3, 7, 3,
            1, 4, 3, 4, 4, 4, 7, 4,
            2, 5, 3, 5,
            2, 6, 3, 6, 5, 6, 6, 6,
            2, 7, 3, 7,
            2, 8, 3, 8,
            3, 9, 4, 9, 7, 9,
            4, 10, 5, 10, 6, 10, 7, 10,
            5, 11, 6, 11, 7, 11,
            4, 12, 5, 12, 7, 12,
            4, 13, 7, 13,
            6, 14, 7, 14,
            6, 15
        };

        /// <summary>
        /// Lit color
        /// </summary>
        private static readonly Color XenkoColor = new Color(0xff3008da);

        /// <summary>
        /// Dim color
        /// </summary>
        private static readonly Color TransparentColor = Color.Transparent;

        /// <summary>
        /// A sprite batch that is used to draw a texture
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// The dynamic texture
        /// </summary>
        private Texture renderTexture;

        /// <summary>
        /// The data of the texture.
        /// </summary>
        private readonly Color[] textureData = new Color[RenderTextureSize * RenderTextureSize];

        // Complete the graphic pipeline, initialize texture data
        public override void Start()
        {
            // create the sprite batch used in our custom rendering function
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // insert the custom renderer in between the 2 camera renderer.
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Insert(2, new SceneDelegateRenderer(RenderTexture));
            
            // Create and initialize the dynamic texture
            renderTexture = Texture.New2D(GraphicsDevice, RenderTextureSize, RenderTextureSize, 1, PixelFormat.R8G8B8A8_UNorm_SRgb, usage: GraphicsResourceUsage.Dynamic);

            // Setup initial data in "SymmetricDefaultShape" to the texture
            for (var i = 0; i < SymmetricDefaultShape.Length; i += 2)
            {
                TogglePixel(SymmetricDefaultShape[i], SymmetricDefaultShape[i + 1]);
                if (SymmetricDefaultShape[i] != (RenderTextureSize - 1) - SymmetricDefaultShape[i])
                    TogglePixel((RenderTextureSize - 1) - SymmetricDefaultShape[i], SymmetricDefaultShape[i + 1]);
            }

            renderTexture.SetData(Game.GraphicsContext.CommandList, textureData);
        }

        public override void Update()
        {
            if (Input.PointerEvents.Count == 0) 
                return;

            var destinationRectangle = new RectangleF(0, 0, GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);

            // Process pointer event
            foreach (var pointerEvent in Input.PointerEvents)
            {
                var pixelPosition = pointerEvent.Position * new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);

                if (pointerEvent.State != PointerState.Down || !destinationRectangle.Contains(pixelPosition)) continue;

                var relativePosition = (pixelPosition - destinationRectangle.TopLeft);

                var pixelX = (int)((relativePosition.X / destinationRectangle.Width) * RenderTextureSize);
                var pixelY = (int)((relativePosition.Y / destinationRectangle.Height) * RenderTextureSize);

                TogglePixel(pixelX, pixelY);
            }

            renderTexture.SetData(Game.GraphicsContext.CommandList, textureData);
        }

        public override void Cancel()
        {
            // Remove the custom renderer from the pipeline.
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.RemoveAt(2);

            // destroy graphic objects
            spriteBatch.Dispose();
            renderTexture.Dispose();
        }

        /// <summary>
        /// Lids or Dims a pixel in the texture for a given coordinate (x, y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void TogglePixel(int x, int y)
        {
            var index = RenderTextureSize * y + x;
            textureData[index] = (textureData[index] != XenkoColor) ? XenkoColor : TransparentColor;
        }


        /// <summary>
        /// Renders the dynamic texture to the screen with sprite batch 
        /// </summary>
        /// <param name="renderContext">The render context</param>
        /// <param name="frame">The render frame</param>
        private void RenderTexture(RenderDrawContext renderContext, RenderFrame frame)
        {
            spriteBatch.Begin(renderContext.GraphicsContext, SpriteSortMode.Texture, null, GraphicsDevice.SamplerStates.PointClamp, DepthStencilStates.None);

            spriteBatch.Draw(renderTexture, new RectangleF(0, 0, GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height), Color.White);

            spriteBatch.End();
        }
    }
}