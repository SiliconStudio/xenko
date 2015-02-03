// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestGameBase : GraphicsTestBase
    {
        public Texture UVTexture { get; private set; }

        public TestGameBase()
        {
            // Enable profiling
            Profiler.EnableAll();

            GraphicsDeviceManager.PreferredBackBufferWidth = 800;
            GraphicsDeviceManager.PreferredBackBufferHeight = 480;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0, GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            UVTexture = Asset.Load<Texture>("uv");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
        }

        protected void SaveTexture(Texture texture, string filename)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            using (var image = texture.GetDataAsImage())
            {
                using (var resultFileStream = File.OpenWrite(filename))
                {
                    image.Save(resultFileStream, ImageFileType.Png);
                }
            }
#endif
        }
    }
}