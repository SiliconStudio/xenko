// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using NUnit.Framework;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Textures.Packing;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestTexturePacker
    {
        public static void LoadParadoxAssemblies()
        {
            RuntimeHelpers.RunModuleConstructor(typeof(Asset).Module.ModuleHandle);
        }

        [TestFixtureSetUp]
        public void InitializeTest()
        {
            LoadParadoxAssemblies();
            TestCommon.InitializeAssetDatabase();
        }

        [Test]
        public void TestMaxRectsPackWithoutRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, false);

            // This data set remain only 1 rectangle that cant be packed
            var packRectangles = new List<RotableRectangle>
            {
                new RotableRectangle(0, 0, 80, 100), 
                new RotableRectangle(0, 0, 100, 20),
            };
            var elementToPack = packRectangles.Select(r => new AtlasTextureElement { DestinationRegion = r }).ToList();

            maxRectPacker.PackRectangles(elementToPack);

            Assert.AreEqual(1, elementToPack.Count);
            Assert.AreEqual(1, maxRectPacker.PackedElements.Count);
        }

        [Test]
        public void TestMaxRectsPackWithRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rectangle that cant be packed
            var packRectangles = new List<AtlasTextureElement>
            {
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 80, 100), Name = "A" }, 
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 100, 20), Name = "B" }, 
            };

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(0, packRectangles.Count);
            Assert.AreEqual(2, maxRectPacker.PackedElements.Count);
            Assert.IsTrue(maxRectPacker.PackedElements.Find(e => e.Name == "B").DestinationRegion.IsRotated);
        }

        /// <summary>
        /// Test packing 7 rectangles
        /// </summary>
        [Test]
        public void TestMaxRectsPackArbitaryRectangles()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rectangle that cant be packed
            var packRectangles = new List<AtlasTextureElement>
            {
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 55, 70) }, 
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 55, 30) },
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 25, 30) }, 
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 20, 30) },
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 45, 30) },
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 25, 40) }, 
                new AtlasTextureElement { DestinationRegion = new RotableRectangle(0, 0, 20, 40) }
            };

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(6, maxRectPacker.PackedElements.Count);
        }

        [Test]
        public void TestTexturePackerFitAllElements()
        {
            var textureElements = CreateFakeTextureElements();

            var texturePacker = new TexturePacker
            {
                AllowMultipack = false,
                AllowRotation = true,
                AllowNonPowerOfTwo = true,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Dispose image
            foreach (var texture in texturePacker.AtlasTextureLayouts.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        public List<AtlasTextureElement> CreateFakeTextureElements()
        {
            var textureElements = new List<AtlasTextureElement>();

            textureElements.Add(new AtlasTextureElement
            {
                Name = "A",
                DestinationRegion =  new RotableRectangle { Width = 100, Height = 200 },
                Texture = Image.New2D(100, 200, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "B",
                DestinationRegion = new RotableRectangle { Width = 400, Height = 300 },
                Texture = Image.New2D(400, 300, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            return textureElements;
        }

        [Test]
        public void TestTexturePackerWithMultiPack()
        {
            var textureElements = CreateFakeTextureElements();

            var texturePacker = new TexturePacker
            {
                AllowMultipack = true,
                AllowRotation = true,
                MaxHeight = 300,
                MaxWidth = 300,
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.AreEqual(2, textureElements.Count);
            Assert.AreEqual(0, texturePacker.AtlasTextureLayouts.Count);
            Assert.IsFalse(canPackAllTextures);

            texturePacker.Reset();
            texturePacker.MaxWidth = 1500;
            texturePacker.MaxHeight = 800;

            canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(1, texturePacker.AtlasTextureLayouts.Count);
            Assert.AreEqual(textureElements.Count, texturePacker.AtlasTextureLayouts[0].Textures.Count);

            Assert.IsTrue(MathUtil.IsPow2(texturePacker.AtlasTextureLayouts[0].Width));
            Assert.IsTrue(MathUtil.IsPow2(texturePacker.AtlasTextureLayouts[0].Height));

            // Dispose image
            foreach (var texture in texturePacker.AtlasTextureLayouts.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        [Test]
        public void TestTexturePackerWithBorder()
        {
            var textureAtlases = new List<AtlasTextureLayout>();

            var textureElements = new List<AtlasTextureElement>();

            textureElements.Add(new AtlasTextureElement("A", null, new Rectangle(0, 0, 100, 200), 2, TextureAddressMode.Clamp, TextureAddressMode.Clamp));
            textureElements.Add(new AtlasTextureElement("B", null, new Rectangle(0, 0, 57, 22), 2, TextureAddressMode.Clamp, TextureAddressMode.Clamp));

            var texturePacker = new TexturePacker
            {
                AllowMultipack = true,
                AllowRotation = true,
                MaxHeight = 512,
                MaxWidth = 512
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.AtlasTextureLayouts);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(2, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);

            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Width));
            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Height));

            // Test if border is applied in width and height
            var textureA = textureAtlases[0].Textures.Find(rectangle => rectangle.Name == "A");
            var textureB = textureAtlases[0].Textures.Find(rectangle => rectangle.Name == "B");

            Assert.AreEqual(textureA.SourceRegion.Width + 2 * textureA.BorderSize, textureA.DestinationRegion.Width);
            Assert.AreEqual(textureA.SourceRegion.Height + 2 * textureA.BorderSize, textureA.DestinationRegion.Height);

            Assert.AreEqual(textureB.SourceRegion.Width + 2 * textureB.BorderSize,
                (!textureB.DestinationRegion.IsRotated) ? textureB.DestinationRegion.Width : textureB.DestinationRegion.Height);
            Assert.AreEqual(textureB.SourceRegion.Height + 2 * textureB.BorderSize,
                (!textureB.DestinationRegion.IsRotated) ? textureB.DestinationRegion.Height : textureB.DestinationRegion.Width);
        }

        public Image CreateMockTexture(int width, int height, Color color)
        {
            var texture = Image.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm);

            unsafe
            {
                var ptr = (Color*)texture.DataPointer;

                // Fill in mock data
                for (var y = 0; y < height; ++y)
                    for (var x = 0; x < width; ++x)
                    {
                        ptr[y * width + x] = y < height / 2 ? color : Color.White;
                    }
            }

            return texture;
        }

        [Test]
        public void TestTextureAtlasFactory()
        {
            var textureElements = new List<AtlasTextureElement>();

            var mockTexture = CreateMockTexture(100, 200, Color.MediumPurple);

            // Load a test texture asset
            textureElements.Add(new AtlasTextureElement
            {
                Name = "A", 
                Texture = mockTexture,
                SourceRegion = new Rectangle(0, 0, 100, 200)
            });

            var texturePacker = new TexturePacker
            {
                AllowMultipack = false,
                AllowRotation = true,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.AtlasTextureLayouts;

            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Width));
            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Height));

            // Create atlas texture
            var atlasTexture = AtlasTextureFactory.CreateTextureAtlas(textureAtlases[0]);
            //atlasTexture.Save(new FileStream(@"C:/Users/Peeranut/Desktop/super_output/img.png", FileMode.CreateNew), ImageFileType.Png);

            Assert.AreEqual(textureAtlases[0].Width, atlasTexture.Description.Width);
            Assert.AreEqual(textureAtlases[0].Height, atlasTexture.Description.Height);

            mockTexture.Dispose();
            atlasTexture.Dispose();
        }

        [Test]
        public void TestNullSizeTexture()
        {
            var textureElements = new List<AtlasTextureElement>();

            var mockTexture = CreateMockTexture(100, 200, Color.MediumPurple);

            // Load a test texture asset
            textureElements.Add(new AtlasTextureElement
            {
                Name = "A", 
                Texture = mockTexture
            });

            var texturePacker = new TexturePacker
            {
                AllowMultipack = false,
                AllowRotation = true,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.AtlasTextureLayouts;

            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Width));
            Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Height));

            // Create atlas texture
            var atlasTexture = AtlasTextureFactory.CreateTextureAtlas(textureAtlases[0]);
            //atlasTexture.Save(new FileStream(@"C:/Users/Peeranut/Desktop/super_output/img.png", FileMode.CreateNew), ImageFileType.Png);

            Assert.AreEqual(textureAtlases[0].Width, atlasTexture.Description.Width);
            Assert.AreEqual(textureAtlases[0].Height, atlasTexture.Description.Height);

            mockTexture.Dispose();
            atlasTexture.Dispose();
        }

        [Test]
        public void TestWrapBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(0, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(5, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(15, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(25, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(29, 10, TextureAddressMode.Wrap));

            // Negative sets
            Assert.AreEqual(6, AtlasTextureFactory.GetSourceTextureCoordinate(-4, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, AtlasTextureFactory.GetSourceTextureCoordinate(-14, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, AtlasTextureFactory.GetSourceTextureCoordinate(-24, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-29, 10, TextureAddressMode.Wrap));
        }

        [Test]
        public void TestClampBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(0, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(5, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(15, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(25, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(29, 10, TextureAddressMode.Clamp));

            // Negative sets
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-4, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-14, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-24, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-29, 10, TextureAddressMode.Clamp));
        }

        [Test]
        public void TestMirrorBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(0, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(5, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(9, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, AtlasTextureFactory.GetSourceTextureCoordinate(11, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(7, AtlasTextureFactory.GetSourceTextureCoordinate(12, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, AtlasTextureFactory.GetSourceTextureCoordinate(21, 10, TextureAddressMode.Mirror));

            // Negative Sets
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-1, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(2, AtlasTextureFactory.GetSourceTextureCoordinate(-2, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(3, AtlasTextureFactory.GetSourceTextureCoordinate(-3, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-9, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-11, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(-20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-21, 10, TextureAddressMode.Mirror));
        }

        [Test]
        public void TestMirrorOnceBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, AtlasTextureFactory.GetSourceTextureCoordinate(0, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(5, AtlasTextureFactory.GetSourceTextureCoordinate(5, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(9, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(11, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(12, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(21, 10, TextureAddressMode.MirrorOnce));

            // Negative Sets
            Assert.AreEqual(1, AtlasTextureFactory.GetSourceTextureCoordinate(-1, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(2, AtlasTextureFactory.GetSourceTextureCoordinate(-2, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(3, AtlasTextureFactory.GetSourceTextureCoordinate(-3, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-9, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-11, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, AtlasTextureFactory.GetSourceTextureCoordinate(-21, 10, TextureAddressMode.MirrorOnce));
        }

        [Test]
        public void TestImageCreationGetAndSet()
        {
            const int Width = 256;
            const int Height = 128;

            var source = Image.New2D(Width, Height, 1, PixelFormat.R8G8B8A8_UNorm);

            Assert.AreEqual(source.TotalSizeInBytes, PixelFormat.R8G8B8A8_UNorm.SizeInBytes() * Width * Height);
            Assert.AreEqual(source.PixelBuffer.Count, 1);

            Assert.AreEqual(1, source.Description.MipLevels);
            Assert.AreEqual(1, source.Description.ArraySize);

            Assert.AreEqual(Width * Height * 4,
                source.PixelBuffer[0].Width * source.PixelBuffer[0].Height * source.PixelBuffer[0].PixelSize);

            // Set Pixel
            var pixelBuffer = source.PixelBuffer[0];
            pixelBuffer.SetPixel(0, 0, (byte)255);

            // Get Pixel
            var fromPixels = pixelBuffer.GetPixels<byte>();
            Assert.AreEqual(fromPixels[0], 255);

            // Dispose images
            source.Dispose();
        }

        [Test]
        public void TestImageDataPointerManipulation()
        {
            const int Width = 256;
            const int Height = 128;

            var source = Image.New2D(Width, Height, 1, PixelFormat.R8G8B8A8_UNorm);

            Assert.AreEqual(source.TotalSizeInBytes, PixelFormat.R8G8B8A8_UNorm.SizeInBytes() * Width * Height);
            Assert.AreEqual(source.PixelBuffer.Count, 1);

            Assert.AreEqual(1, source.Description.MipLevels);
            Assert.AreEqual(1, source.Description.ArraySize);

            Assert.AreEqual(Width * Height * 4,
                source.PixelBuffer[0].Width * source.PixelBuffer[0].Height * source.PixelBuffer[0].PixelSize);

            unsafe
            {
                var ptr = (Color*)source.DataPointer;

                // Clean the data
                for (var i = 0; i < source.PixelBuffer[0].Height * source.PixelBuffer[0].Width; ++i)
                    ptr[i] = Color.Transparent;

                // Set a specific pixel to red
                ptr[0] = Color.Red;
            }

            var pixelBuffer = source.PixelBuffer[0];

            // Get Pixel
            var fromPixels = pixelBuffer.GetPixels<Color>();
            Assert.AreEqual(Color.Red, fromPixels[0]);

            // Dispose images
            source.Dispose();
        }

        [Test]
        public void TestCreateTextureAtlasToOutput()
        {
            const string OutputPath = "./output.png";
            var textureElements = new List<AtlasTextureElement>();

            // Load a test texture asset
            textureElements.Add(new AtlasTextureElement
            {
                Name = "MediumPurple",
                BorderSize = 10,
                Texture = CreateMockTexture(130, 158, Color.MediumPurple),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Red",
                BorderSize = 10,
                Texture = CreateMockTexture(127, 248, Color.Red),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Blue",
                BorderSize = 10,
                Texture = CreateMockTexture(212, 153, Color.Blue),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Gold",
                BorderSize = 10,
                Texture = CreateMockTexture(78, 100, Color.Gold),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "RosyBrown",
                BorderSize = 10,
                Texture = CreateMockTexture(78, 100, Color.RosyBrown),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "SaddleBrown",
                BorderSize = 10,
                Texture = CreateMockTexture(400, 100, Color.SaddleBrown),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Salmon",
                BorderSize = 10,
                Texture = CreateMockTexture(400, 200, Color.Salmon),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "PowderBlue",
                BorderSize = 10,
                Texture = CreateMockTexture(190, 200, Color.PowderBlue),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Orange",
                BorderSize = 10,
                Texture = CreateMockTexture(200, 230, Color.Orange),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Silver",
                BorderSize = 10,
                Texture = CreateMockTexture(100, 170, Color.Silver),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "SlateGray",
                BorderSize = 10,
                Texture = CreateMockTexture(100, 170, Color.SlateGray),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            textureElements.Add(new AtlasTextureElement
            {
                Name = "Tan",
                BorderSize = 10,
                Texture = CreateMockTexture(140, 110, Color.Tan),
                BorderModeU = TextureAddressMode.Border,
                BorderModeV = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue
            });

            var texturePacker = new TexturePacker
            {
                AllowMultipack = false,
                AllowRotation = true,
                MaxHeight = 1024,
                MaxWidth = 1024
            };

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.AtlasTextureLayouts;

            Assert.AreEqual(1, textureAtlases.Count);

            if (!texturePacker.AllowNonPowerOfTwo)
            {
                Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Width));
                Assert.IsTrue(MathUtil.IsPow2(textureAtlases[0].Height));
            }

            // Create atlas texture
            var atlasTexture = AtlasTextureFactory.CreateTextureAtlas(textureAtlases[0]);
            atlasTexture.Save(new FileStream(OutputPath, FileMode.Create), ImageFileType.Png);

            Assert.AreEqual(textureAtlases[0].Width, atlasTexture.Description.Width);
            Assert.AreEqual(textureAtlases[0].Height, atlasTexture.Description.Height);

            atlasTexture.Dispose();

            foreach (var texture in textureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
            {
                texture.Texture.Dispose();
            }
        }

        //        [Test]
        public void TestLoadImagesToCreateAtlas()
        {
            // Specify where the images are, and uncomment [Test] above
            var inputDir = @".\";

            var textureElements = new List<AtlasTextureElement>();

            using (var texTool = new TextureTool())
            {
                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_idle_0" + i;
                    textureElements.Add(new AtlasTextureElement
                    {
                        Name = name,
                        BorderSize = 100,
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png")),
                        BorderColor = Color.SteelBlue,
                        BorderModeU = TextureAddressMode.Wrap,
                        BorderModeV = TextureAddressMode.Wrap
                    });
                }

                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_run_0" + i;
                    textureElements.Add(new AtlasTextureElement
                    {
                        Name = name,
                        BorderSize = 100,
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png")),
                        BorderColor = Color.SteelBlue,
                        BorderModeU = TextureAddressMode.Wrap,
                        BorderModeV = TextureAddressMode.Wrap
                    });
                }

                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_shoot_0" + i;
                    textureElements.Add(new AtlasTextureElement
                    {
                        Name = name,
                        BorderSize = 100,
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png")),
                        BorderColor = Color.SteelBlue,
                        BorderModeU = TextureAddressMode.Wrap,
                        BorderModeV = TextureAddressMode.Wrap
                    });
                }

                for (var i = 1; i <= 8; ++i)
                {
                    var name = "ef_0" + i;
                    textureElements.Add(new AtlasTextureElement
                    {
                        Name = name,
                        BorderSize = 100,
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png")),
                        BorderColor = Color.SteelBlue,
                        BorderModeU = TextureAddressMode.Wrap,
                        BorderModeV = TextureAddressMode.Wrap
                    });
                }

                var texturePacker = new TexturePacker
                {
                    AllowMultipack = false,
                    AllowRotation = false,
                    MaxHeight = 2048,
                    MaxWidth = 2048
                };

                var canPackAllTextures = texturePacker.PackTextures(textureElements);

                Assert.IsTrue(canPackAllTextures);

                // Obtain texture atlases
                var textureAtlases = texturePacker.AtlasTextureLayouts;

                // Create atlas texture
                var atlasTexture = AtlasTextureFactory.CreateTextureAtlas(textureAtlases[0]);
                const ImageFileType outputType = ImageFileType.Png;

                atlasTexture.Save(new FileStream(@"C:\Users\Peeranut\Desktop\sprite_output\output2." + GetImageExtension(outputType), FileMode.Create), outputType);
                atlasTexture.Dispose();

                foreach (var texture in textureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
                    texture.Texture.Dispose();
            }
        }

        private string GetImageExtension(ImageFileType fileType)
        {
            return fileType.ToString().ToLower();
        }

        private Image LoadImage(TextureTool texTool, UFile sourcePath)
        {
            using (var texImage = texTool.Load(sourcePath, false))
            {
                // Decompresses the specified texImage
                texTool.Decompress(texImage, false);

                if (texImage.Format == PixelFormat.B8G8R8A8_UNorm)
                    texTool.SwitchChannel(texImage);

                return texTool.ConvertToParadoxImage(texImage);
            }
        }
    }
}
