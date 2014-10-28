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
using SiliconStudio.Paradox.Assets.Texture;
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
            Game.InitializeAssetDatabase();
        }

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

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(1, maxRectPacker.PackedRectangles.Count);
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

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(0, packRectangles.Count);
            Assert.AreEqual(2, maxRectPacker.PackedRectangles.Count);
            Assert.IsTrue(maxRectPacker.PackedRectangles.Find(rectangle => rectangle.Key == "B").IsRotated);
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

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(6, maxRectPacker.PackedRectangles.Count);
        }

        [Test]
        public void TestTexturePackerFitAllElements()
        {
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Config
            {
                BorderSize = 0,
                UseMultipack = false,
                UseRotation = true,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Dispose image
            foreach (var texture in texturePacker.TextureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        public Dictionary<string, IntermediateTexture> CreateFakeTextureElements()
        {
            var textureElements = new Dictionary<string, IntermediateTexture>();

            textureElements.Add("A", new IntermediateTexture
            {
                Texture = Image.New2D(100, 200, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            textureElements.Add("B", new IntermediateTexture
            {
                Texture = Image.New2D(400, 300, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            return textureElements;
        }

        [Test]
        public void TestTexturePackerWithMultiPack()
        {
            var textureAtlases = new List<TextureAtlas>();
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Config
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 300,
                MaxWidth = 300
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.AreEqual(1, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsFalse(canPackAllTextures);

            // The current bin cant fit all of textures, resize the bin
            var newPackConfiguration = new Config
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 1500,
                MaxWidth = 800
            };

            texturePacker.ResetPacker(newPackConfiguration);

            canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(2, textureAtlases.Count);

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[1].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[1].Height));

            // Dispose image
            foreach (var texture in textureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        [Test]
        public void TestTexturePackerWithBorder()
        {
            var textureAtlases = new List<TextureAtlas>();

            var textureElements = new Dictionary<string, IntermediateTexture>();

            textureElements.Add("A", new IntermediateTexture
            {
                Texture = Image.New2D(100, 200, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            textureElements.Add("B", new IntermediateTexture
            {
                Texture = Image.New2D(57, 22, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            var packConfiguration = new Config
            {
                BorderSize = 2,
                UseMultipack = true,
                UseRotation = true,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 512,
                MaxWidth = 512
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            // Test if border is applied in width and height
            var textureA = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "A");
            var textureB = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "B");

            Assert.AreEqual(textureA.Texture.Description.Width + 2 * packConfiguration.BorderSize, textureA.Region.Value.Width);
            Assert.AreEqual(textureA.Texture.Description.Height + 2 * packConfiguration.BorderSize, textureA.Region.Value.Height);

            Assert.AreEqual(textureB.Texture.Description.Width + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Width : textureB.Region.Value.Height);
            Assert.AreEqual(textureB.Texture.Description.Height + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Height : textureB.Region.Value.Width);
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
            var textureElements = new Dictionary<string, IntermediateTexture>();

            var mockTexture = CreateMockTexture(100, 200, Color.MediumPurple);

            // Load a test texture asset
            textureElements.Add("A", new IntermediateTexture
            {
                Texture = mockTexture
            });

            var packConfiguration = new Config
            {
                BorderSize = 0,
                SizeContraint = SizeConstraints.PowerOfTwo,
                UseMultipack = false,
                UseRotation = true,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.TextureAtlases;

            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            // Create atlas texture
            var atlasTexture = TextureAtlasFactory.CreateTextureAtlas(textureAtlases[0]);
//            atlasTexture.Save(new FileStream(@"C:/Users/Peeranut/Desktop/super_output/img.png", FileMode.CreateNew), ImageFileType.Png);

            Assert.AreEqual(textureAtlases[0].Width, atlasTexture.Description.Width);
            Assert.AreEqual(textureAtlases[0].Height, atlasTexture.Description.Height);

            mockTexture.Dispose();
            atlasTexture.Dispose();
        }

        [Test]
        public void TestWrapBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(15, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(25, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(29, 10, TextureAddressMode.Wrap));

            // Negative sets
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-4, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-14, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-24, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-29, 10, TextureAddressMode.Wrap));
        }

        [Test]
        public void TestClampBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(15, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(25, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(29, 10, TextureAddressMode.Clamp));

            // Negative sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-4, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-14, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-24, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-29, 10, TextureAddressMode.Clamp));
        }

        [Test]
        public void TestMirrorBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(11, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(7, TextureAtlasFactory.GetSourceTextureIndex(12, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(21, 10, TextureAddressMode.Mirror));

            // Negative Sets
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-1, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(2, TextureAtlasFactory.GetSourceTextureIndex(-2, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(3, TextureAtlasFactory.GetSourceTextureIndex(-3, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-11, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-21, 10, TextureAddressMode.Mirror));
        }

        [Test]
        public void TestMirrorOnceBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(11, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(12, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(21, 10, TextureAddressMode.MirrorOnce));

            // Negative Sets
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-1, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(2, TextureAtlasFactory.GetSourceTextureIndex(-2, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(3, TextureAtlasFactory.GetSourceTextureIndex(-3, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-11, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-21, 10, TextureAddressMode.MirrorOnce));
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
            var textureElements = new Dictionary<string, IntermediateTexture>();

            // Load a test texture asset
            textureElements.Add("MediumPurple", new IntermediateTexture
            {
                Texture = CreateMockTexture(130, 158, Color.MediumPurple)
            });

            textureElements.Add("Red", new IntermediateTexture
            {
                Texture = CreateMockTexture(127, 248, Color.Red)
            });

            textureElements.Add("Blue", new IntermediateTexture
            {
                Texture = CreateMockTexture(212, 153, Color.Blue)
            });

            textureElements.Add("Gold", new IntermediateTexture
            {
                Texture = CreateMockTexture(78, 100, Color.Gold)
            });

            textureElements.Add("RosyBrown", new IntermediateTexture
            {
                Texture = CreateMockTexture(78, 100, Color.RosyBrown)
            });

            textureElements.Add("SaddleBrown", new IntermediateTexture
            {
                Texture = CreateMockTexture(400, 100, Color.SaddleBrown)
            });

            textureElements.Add("Salmon", new IntermediateTexture
            {
                Texture = CreateMockTexture(400, 200, Color.Salmon)
            });

            textureElements.Add("PowderBlue", new IntermediateTexture
            {
                Texture = CreateMockTexture(190, 200, Color.PowderBlue)
            });

            textureElements.Add("Orange", new IntermediateTexture
            {
                Texture = CreateMockTexture(200, 230, Color.Orange)
            });

            textureElements.Add("Silver", new IntermediateTexture
            {
                Texture = CreateMockTexture(100, 170, Color.Silver)
            });

            textureElements.Add("SlateGray", new IntermediateTexture
            {
                Texture = CreateMockTexture(100, 170, Color.SlateGray)
            });

            textureElements.Add("Tan", new IntermediateTexture
            {
                Texture = CreateMockTexture(140, 110, Color.Tan)
            });

            var packConfiguration = new Config
            {
                BorderSize = 10,
                SizeContraint = SizeConstraints.PowerOfTwo,
                BorderAddressMode = TextureAddressMode.Border,
                BorderColor = Color.SteelBlue,
                UseMultipack = false,
                UseRotation = true,
                MaxHeight = 1024,
                MaxWidth = 1024
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.TextureAtlases;

            Assert.AreEqual(1, textureAtlases.Count);

            if (packConfiguration.SizeContraint == SizeConstraints.PowerOfTwo)
            {
                Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
                Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));
            }

            // Create atlas texture
            var atlasTexture = TextureAtlasFactory.CreateTextureAtlas(textureAtlases[0]);
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

            var textureElements = new Dictionary<string, IntermediateTexture>();

            using (var texTool = new TextureTool())
            {
                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_idle_0" + i;
                    textureElements.Add(name, new IntermediateTexture
                    {
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png"))
                    });
                }

                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_run_0" + i;
                    textureElements.Add(name, new IntermediateTexture
                    {
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png"))
                    });
                }

                for (var i = 1; i <= 5; ++i)
                {
                    var name = "character_shoot_0" + i;
                    textureElements.Add(name, new IntermediateTexture
                    {
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png"))
                    });
                }

                for (var i = 1; i <= 8; ++i)
                {
                    var name = "ef_0" + i;
                    textureElements.Add(name, new IntermediateTexture
                    {
                        Texture = LoadImage(texTool, new UFile(inputDir + "/" + name + ".png"))
                    });
                }

                var packConfiguration = new Config
                {
                    BorderSize = 100,
                    SizeContraint = SizeConstraints.PowerOfTwo,
                    BorderAddressMode = TextureAddressMode.Wrap,
                    OutputAtlasImageType = ImageFileType.Dds,
                    BorderColor = Color.SteelBlue,
                    UseMultipack = false,
                    UseRotation = false,
                    MaxHeight = 2048,
                    MaxWidth = 2048
                };

                var texturePacker = new TexturePacker(packConfiguration);

                var canPackAllTextures = texturePacker.PackTextures(textureElements);

                Assert.IsTrue(canPackAllTextures);

                // Obtain texture atlases
                var textureAtlases = texturePacker.TextureAtlases;

                // Create atlas texture
                var atlasTexture = TextureAtlasFactory.CreateTextureAtlas(textureAtlases[0]);
                var outputType = packConfiguration.OutputAtlasImageType ?? ImageFileType.Png;

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
            using (var texImage = texTool.Load(sourcePath))
            {
                // Decompresses the specified texImage
                texTool.Decompress(texImage);

                if (texImage.Format == PixelFormat.B8G8R8A8_UNorm)
                    texTool.SwitchChannel(texImage);

                return texTool.ConvertToParadoxImage(texImage);
            }
        }
    }
}
