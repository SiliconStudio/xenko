// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine.Tests
{
    public class SpriteProviderTests : GameTestBase
    {
        [Test]
        public void SpriteFromSheetTests()
        {
            var fromNullSheet = new SpriteFromSheet();
            Assert.AreEqual(0, fromNullSheet.SpritesCount);
            Assert.IsNull(fromNullSheet.GetSprite(0));

            var emptySheet = new SpriteSheet();
            var fromEmptySheet = new SpriteFromSheet { Sheet = emptySheet };
            Assert.AreEqual(0, fromEmptySheet.SpritesCount);
            Assert.IsNull(fromEmptySheet.GetSprite(0));

            var nullList = new SpriteSheet { Sprites = null };
            var fromnullListSheet = new SpriteFromSheet { Sheet = nullList };
            Assert.AreEqual(0, fromnullListSheet.SpritesCount);
            Assert.IsNull(fromnullListSheet.GetSprite(0));
            
            var emptyList = new SpriteSheet { Sprites = new List<Sprite>() };
            var fromEmptyListSheet = new SpriteFromSheet { Sheet = emptyList };
            Assert.AreEqual(0, fromEmptyListSheet.SpritesCount);
            Assert.IsNull(fromEmptyListSheet.GetSprite(0));

            var validSheet = new SpriteSheet() { Sprites = new List<Sprite> { new Sprite("0"), new Sprite("1") } };
            var fromValidSheet = new SpriteFromSheet { Sheet = validSheet };
            Assert.AreEqual(2, fromValidSheet.SpritesCount);
            for (int i = 0; i < fromValidSheet.SpritesCount; i++)
            {
                Assert.AreEqual(i.ToString(), fromValidSheet.GetSprite(i).Name);
            }
        }

        [Test]
        public void SpriteFromTextureTests()
        {
            PerformTest(game =>
            {
                var provider = new SpriteFromTexture();
                Assert.AreEqual(1, provider.SpritesCount);
                Assert.IsNotNull(provider.GetSprite(0));

                var sprite = provider.GetSprite(0);
                Assert.IsNull(sprite.Texture);
                Assert.AreEqual(new RectangleF(), sprite.Region);
                Assert.AreEqual(Vector2.Zero, sprite.Center);

                var texture1 = Texture.New2D(game.GraphicsDevice, 123, 234, 1, PixelFormat.B8G8R8A8_UNorm);
                provider.Texture = texture1;
                Assert.AreEqual(texture1, sprite.Texture);
                Assert.AreEqual(new RectangleF(0, 0, texture1.Width, texture1.Height), sprite.Region);
                Assert.AreEqual(new Vector2(texture1.Width, texture1.Height) / 2, sprite.Center);

                var texture2 = Texture.New2D(game.GraphicsDevice, 12, 23, 1, PixelFormat.B8G8R8A8_UNorm);
                provider.Texture = texture2;
                Assert.AreEqual(texture2, sprite.Texture);
                Assert.AreEqual(new RectangleF(0, 0, texture2.Width, texture2.Height), sprite.Region);
                Assert.AreEqual(new Vector2(texture2.Width, texture2.Height) / 2, sprite.Center);

                provider.IsTransparent = false;
                Assert.IsFalse(sprite.IsTransparent);
                provider.IsTransparent = true;
                Assert.IsTrue(sprite.IsTransparent);

                provider.CenterFromMiddle = false;
                Assert.AreEqual(Vector2.Zero, sprite.Center);
                provider.Center = new Vector2(43, 54);
                Assert.AreEqual(provider.Center, sprite.Center);
                provider.CenterFromMiddle = true;
                Assert.AreEqual(provider.Center + new Vector2(texture2.Width, texture2.Height) / 2, sprite.Center);

                Assert.AreEqual(new Vector2(provider.PixelsPerUnit), sprite.PixelsPerUnit);
                provider.PixelsPerUnit = 1;
                Assert.AreEqual(new Vector2(provider.PixelsPerUnit), sprite.PixelsPerUnit);
                Assert.AreEqual(new Vector2(texture2.Width, texture2.Height), sprite.Size);
            });
        }
    }
}