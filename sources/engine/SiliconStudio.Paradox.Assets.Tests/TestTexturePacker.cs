// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using NUnit.Framework;

using SiliconStudio.Paradox.Assets.Texture;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestTexturePacker
    {
        [Test]
        public void TestMaxRectsPackWithoutRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, false);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 80, 100), new RotatableRectangle(0, 0, 100, 20),
            };

            maxRectPacker.Insert(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(1, maxRectPacker.UsedRectangles.Count);
        }

        [Test]
        public void TestMaxRectsPackWithRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 80, 100) { Key = "A" }, new RotatableRectangle(0, 0, 100, 20) { Key = "B"},
            };

            maxRectPacker.Insert(packRectangles);

            Assert.AreEqual(0, packRectangles.Count);
            Assert.AreEqual(2, maxRectPacker.UsedRectangles.Count);
            Assert.AreEqual(true, maxRectPacker.UsedRectangles.Find(rectangle => rectangle.Key == "B").IsRotated);
        }

        /// <summary>
        /// Test packing 7 rectangles
        /// </summary>
        [Test]
        public void TestMaxRectsPackArbitaryRectangles()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 55, 70), new RotatableRectangle(0, 0, 55, 30),
                new RotatableRectangle(0, 0, 25, 30), new RotatableRectangle(0, 0, 20, 30),
                new RotatableRectangle(0, 0, 45, 30),
                new RotatableRectangle(0, 0, 25, 40), new RotatableRectangle(0, 0, 20, 40)
            };

            maxRectPacker.Insert(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(6, maxRectPacker.UsedRectangles.Count);
        }

        [Test]
        public void TestTexturePackerFitAllElements()
        {
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = false,
                UseRotation = true,
                PivotType = PivotType.Center,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var texturePacker = new TexturePacker();

            texturePacker.Initialize(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.AreEqual(true, canPackAllTextures);
        }

        public Dictionary<string, IntemediateTextureElement> CreateFakeTextureElements()
        {
            var textureElements = new Dictionary<string, IntemediateTextureElement>();

            textureElements.Add("A", new IntemediateTextureElement
            {
                TextureName = "A",
                Texture = new FakeTexture2D { Width = 100, Height = 200 }
            });

            textureElements.Add("B", new IntemediateTextureElement
            {
                TextureName = "B",
                Texture = new FakeTexture2D { Width = 400, Height = 300 }
            });

            return textureElements;
        }

        [Test]
        public void TestTexturePackerWithMultiPack()
        {
            var textureAtlases = new List<TextureAtlas>();
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 300,
                MaxWidth = 300
            };

            var texturePacker = new TexturePacker();

            texturePacker.Initialize(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.AreEqual(1, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);
            Assert.AreEqual(false, canPackAllTextures);

            // The current bin cant fit all of textures, resize the bin
            packConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 1500,
                MaxWidth = 800
            };

            texturePacker.Initialize(packConfiguration);

            canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.AreEqual(true, canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(2, textureAtlases.Count);

            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[0].Width));
            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[0].Height));

            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[1].Width));
            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[1].Height));
        }

        private bool IsPowerOfTwo(int value)
        {
            return (value & (value - 1)) == 0;
        }

        [Test]
        public void TestTexturePackerWithBorder()
        {
            var textureAtlases = new List<TextureAtlas>();

            var textureElements = new Dictionary<string, IntemediateTextureElement>();

            textureElements.Add("A", new IntemediateTextureElement
            {
                TextureName = "A",
                Texture = new FakeTexture2D { Width = 100, Height = 200 },
            });

            textureElements.Add("B", new IntemediateTextureElement
            {
                TextureName = "B",
                Texture = new FakeTexture2D { Width = 57, Height = 22 },
            });

            var packConfiguration = new Configuration
            {
                BorderSize = 2,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 512,
                MaxWidth = 512
            };

            var texturePacker = new TexturePacker();

            texturePacker.Initialize(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.AreEqual(true, canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);

            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[0].Width));
            Assert.AreEqual(true, IsPowerOfTwo(textureAtlases[0].Height));

            // Test if border is applied in width and height
            var textureA = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "A");
            var textureB = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "B");

            Assert.AreEqual(textureA.Texture.Width + 2 * packConfiguration.BorderSize, textureA.Region.Value.Width);
            Assert.AreEqual(textureA.Texture.Height + 2 * packConfiguration.BorderSize, textureA.Region.Value.Height);

            Assert.AreEqual(textureB.Texture.Width + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Width : textureB.Region.Value.Height);
            Assert.AreEqual(textureB.Texture.Height + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Height : textureB.Region.Value.Width);
        }
    }
}
