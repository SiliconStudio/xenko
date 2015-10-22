// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    [Description("Check Texture")]
    public class TestTexture : GraphicsTestBase
    {
        public TestTexture()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
        }

        [Test]
        public void TestTexture1D()
        {
            RunDrawTest(
                game =>
                {
                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[256];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New1D(game.GraphicsDevice, 256, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

                    // Perform texture op
                    CheckTexture(texture, data);

                    // Release the texture
                    texture.Dispose();
                });
        }

        [Test]
        public void TestTexture1DMipMap()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[256];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New1D(device, 256, true, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

                    // Verify the number of mipmap levels
                    Assert.That(texture.MipLevels, Is.EqualTo(Math.Log(data.Length, 2) + 1));

                    // Get a render target on the mipmap 1 (128) with value 1 and get back the data
                    var renderTarget1 = texture.ToTextureView(ViewType.Single, 0, 1);
                    device.Clear(renderTarget1, new Color4(0xFF000001));
                    var data1 = texture.GetData<byte>(0, 1);
                    Assert.That(data1.Length, Is.EqualTo(128));
                    Assert.That(data1[0], Is.EqualTo(1));
                    renderTarget1.Dispose();

                    // Get a render target on the mipmap 2 (128) with value 2 and get back the data
                    var renderTarget2 = texture.ToTextureView(ViewType.Single, 0, 2);
                    device.Clear(renderTarget2, new Color4(0xFF000002));
                    var data2 = texture.GetData<byte>(0, 2);
                    Assert.That(data2.Length, Is.EqualTo(64));
                    Assert.That(data2[0], Is.EqualTo(2));
                    renderTarget2.Dispose();

                    // Get a render target on the mipmap 3 (128) with value 3 and get back the data
                    var renderTarget3 = texture.ToTextureView(ViewType.Single, 0, 3);
                    device.Clear(renderTarget3, new Color4(0xFF000003));
                    var data3 = texture.GetData<byte>(0, 3);
                    Assert.That(data3.Length, Is.EqualTo(32));
                    Assert.That(data3[0], Is.EqualTo(3));
                    renderTarget3.Dispose();

                    // Release the texture
                    texture.Dispose();
                });
        }

        [Test]
        public void TestTexture2D()
        {
            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[256 * 256];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New2D(device, 256, 256, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

                    // Perform texture op
                    CheckTexture(texture, data);

                    // Release the texture
                    texture.Dispose();
                },
                GraphicsProfile.Level_9_1);
        }

        [Test]
        public void TestTexture2DArray()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[256 * 256];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New2D(device, 256, 256, 1, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget, 4);

                    // Verify the number of mipmap levels
                    Assert.That(texture.MipLevels, Is.EqualTo(1));

                    // Get a render target on the array 1 (128) with value 1 and get back the data
                    var renderTarget1 = texture.ToTextureView(ViewType.Single, 1, 0);
                    Assert.That(renderTarget1.ViewWidth, Is.EqualTo(256));
                    Assert.That(renderTarget1.ViewHeight, Is.EqualTo(256));

                    device.Clear(renderTarget1, new Color4(0xFF000001));
                    var data1 = texture.GetData<byte>(1);
                    Assert.That(data1.Length, Is.EqualTo(data.Length));
                    Assert.That(data1[0], Is.EqualTo(1));
                    renderTarget1.Dispose();

                    // Get a render target on the array 2 (128) with value 2 and get back the data
                    var renderTarget2 = texture.ToTextureView(ViewType.Single, 2, 0);
                    device.Clear(renderTarget2, new Color4(0xFF000002));
                    var data2 = texture.GetData<byte>(2);
                    Assert.That(data2.Length, Is.EqualTo(data.Length));
                    Assert.That(data2[0], Is.EqualTo(2));
                    renderTarget2.Dispose();

                    // Get a render target on the array 3 (128) with value 3 and get back the data
                    var renderTarget3 = texture.ToTextureView(ViewType.Single, 3, 0);
                    device.Clear(renderTarget3, new Color4(0xFF000003));
                    var data3 = texture.GetData<byte>(3);
                    Assert.That(data3.Length, Is.EqualTo(data.Length));
                    Assert.That(data3[0], Is.EqualTo(3));
                    renderTarget3.Dispose();

                    // Release the texture
                    texture.Dispose();
                });
        }

        [Test]
        public void TestTexture2DUnorderedAccess()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[256 * 256];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New2D(device, 256, 256, 1, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 4);

                    // Clear slice array[1] with value 1, read back data from texture and check validity
                    var texture1 = texture.ToTextureView(ViewType.Single, 1, 0);
                    Assert.That(texture1.ViewWidth, Is.EqualTo(256));
                    Assert.That(texture1.ViewHeight, Is.EqualTo(256));
                    Assert.That(texture1.ViewDepth, Is.EqualTo(1));

                    device.ClearReadWrite(texture1, new Int4(1));
                    var data1 = texture.GetData<byte>(1);
                    Assert.That(data1.Length, Is.EqualTo(data.Length));
                    Assert.That(data1[0], Is.EqualTo(1));
                    texture1.Dispose();

                    // Clear slice array[2] with value 2, read back data from texture and check validity
                    var texture2 = texture.ToTextureView(ViewType.Single, 2, 0);
                    device.ClearReadWrite(texture2, new Int4(2));
                    var data2 = texture.GetData<byte>(2);
                    Assert.That(data2.Length, Is.EqualTo(data.Length));
                    Assert.That(data2[0], Is.EqualTo(2));
                    texture2.Dispose();

                    texture.Dispose();
                },
                GraphicsProfile.Level_11_0); // Force to use Level11 in order to use UnorderedAccessViews
        }

        [Test]
        public void TestTexture3D()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;
                    
                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var data = new byte[32 * 32 * 32];
                    data[0] = 255;
                    data[31] = 1;
                    var texture = Texture.New3D(device, 32, 32, 32, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

                    // Perform generate texture checking
                    CheckTexture(texture, data);

                    // Release the texture
                    texture.Dispose();
                });
        }

        [Test]
        public void TestTexture3DRenderTarget()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var texture = Texture.New3D(device, 32, 32, 32, true, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

                    // Get a render target on the 1st mipmap of this texture 3D
                    var renderTarget0 = texture.ToTextureView(ViewType.Single, 0, 0);
                    device.Clear(renderTarget0, new Color4(0xFF000001));
                    var data1 = texture.GetData<byte>();
                    Assert.That(data1.Length, Is.EqualTo(32 * 32 * 32));
                    Assert.That(data1[0], Is.EqualTo(1));
                    renderTarget0.Dispose();

                    // Get a render target on the 2nd mipmap of this texture 3D
                    var renderTarget1 = texture.ToTextureView(ViewType.Single, 0, 1);

                    // Check that width/height is correctly calculated 
                    Assert.That(renderTarget1.ViewWidth, Is.EqualTo(32 >> 1));
                    Assert.That(renderTarget1.ViewHeight, Is.EqualTo(32 >> 1));

                    device.Clear(renderTarget1, new Color4(0xFF000001));
                    var data2 = texture.GetData<byte>(0, 1);
                    Assert.That(data2.Length, Is.EqualTo(16 * 16 * 16));
                    Assert.That(data2[0], Is.EqualTo(1));
                    renderTarget1.Dispose();

                    // Release the texture
                    texture.Dispose();
                });
        }

        [Test]
        public void TestDepthStencilBuffer()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    // Check that read-only is not supported for depth stencil buffer
                    var supported = GraphicsDevice.Platform != GraphicsPlatform.Direct3D11;
                    Assert.AreEqual(supported, Texture.IsDepthStencilReadOnlySupported(device));

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var texture = Texture.New2D(device, 256, 256, PixelFormat.D32_Float, TextureFlags.DepthStencil);

                    // Clear the depth stencil buffer with a value of 0.5f
                    device.Clear(texture, DepthStencilClearOptions.DepthBuffer, 0.5f);

                    var values = texture.GetData<float>();
                    Assert.That(values.Length, Is.EqualTo(256*256));
                    Assert.That(values[0], Is.EqualTo(0.5f));

                    // Create a new copy of the depth stencil buffer
                    var textureCopy = texture.CreateDepthTextureCompatible();

                    device.Copy(texture, textureCopy);

                    values = textureCopy.GetData<float>();
                    Assert.That(values.Length, Is.EqualTo(256 * 256));
                    Assert.That(values[0], Is.EqualTo(0.5f));

                    // Dispose the depth stencil buffer
                    textureCopy.Dispose();
                }, 
                GraphicsProfile.Level_10_0);
        }

        [Test]
        public void TestDepthStencilBufferWithNativeReadonly()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunDrawTest(
                game =>
                {
                    var device = game.GraphicsDevice;

                    //// Without shaders, it is difficult to check this method without accessing internals

                    // Check that read-only is not supported for depth stencil buffer
                    Assert.That(Texture.IsDepthStencilReadOnlySupported(device), Is.True);

                    // Check texture creation with an array of data, with usage default to later allow SetData
                    var texture = Texture.New2D(device, 256, 256, PixelFormat.D32_Float, TextureFlags.ShaderResource | TextureFlags.DepthStencilReadOnly);

                    // Clear the depth stencil buffer with a value of 0.5f, but the depth buffer is readonly
                    device.Clear(texture, DepthStencilClearOptions.DepthBuffer, 0.5f);

                    var values = texture.GetData<float>();
                    Assert.That(values.Length, Is.EqualTo(256 * 256));
                    Assert.That(values[0], Is.EqualTo(0.0f));

                    // Dispose the depth stencil buffer
                    texture.Dispose();
                },
                GraphicsProfile.Level_11_0);
        }

        /// <summary>
        /// Tests the load save.
        /// </summary>
        /// <remarks>
        /// This test loads several images using <see cref="Texture.Load"/> (on the GPU) and save them to the disk using <see cref="Texture.Save(Stream,ImageFileType)"/>.
        /// The saved image is then compared with the original image to check that the whole chain (CPU -> GPU, GPU -> CPU) is passing correctly
        /// the textures.
        /// </remarks>
        [Test]
        public void TestLoadSave()
        {
            foreach (ImageFileType sourceFormat in Enum.GetValues(typeof(ImageFileType)))
            {
                RunDrawTest(
                    game =>
                    {
                        var intermediateFormat = ImageFileType.Paradox;

                        if (sourceFormat == ImageFileType.Wmp) // no input image of this format.
                            return;

                        if (sourceFormat == ImageFileType.Wmp || sourceFormat == ImageFileType.Tga) // TODO remove this when Load/Save methods are implemented for those types.
                            return;

                        var device = game.GraphicsDevice;
                        var fileName = sourceFormat.ToFileExtension().Substring(1) + "Image";
                        var filePath = "ImageTypes/" + fileName;

                        var testMemoryBefore = GC.GetTotalMemory(true);
                        var clock = Stopwatch.StartNew();

                        // Load an image from a file and dispose it.
                        Texture texture;
                        using (var inStream = game.Asset.OpenAsStream(filePath, StreamFlags.None))
                            texture = Texture.Load(device, inStream);

                        // Copy GPU to GPU
                        var texture2 = texture.Clone();
                        device.Copy(texture, texture2);

                        // dispose original
                        texture.Dispose();
                        texture = texture2;
                            
                        var tempStream = new MemoryStream();
                        texture.Save(tempStream, intermediateFormat);
                        tempStream.Position = 0;
                        texture.Dispose();

                        using (var inStream = game.Asset.OpenAsStream(filePath, StreamFlags.None))
                        using (var originalImage = Image.Load(inStream))
                        {
                            using (var textureImage = Image.Load(tempStream))
                            {
                                TestImage.CompareImage(originalImage, textureImage, false, false, fileName);
                            }
                        }
                        tempStream.Dispose();
                        var time = clock.ElapsedMilliseconds;
                        clock.Stop();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        var testMemoryAfter = GC.GetTotalMemory(true);
                        Log.Info(@"Test loading {0} GPU texture / saving to {1} and compare with original Memory {2} delta bytes, in {3}ms", fileName, intermediateFormat, testMemoryAfter - testMemoryBefore, time);
                    }, 
                    GraphicsProfile.Level_9_1);
            }
        }

        [Test]
        public void TestLoadDraw()
        {
            foreach (ImageFileType sourceFormat in Enum.GetValues(typeof(ImageFileType)))
            {
                if (sourceFormat == ImageFileType.Wmp) // no input image of this format.
                    return;

                if (sourceFormat == ImageFileType.Wmp || sourceFormat == ImageFileType.Tga) // TODO remove this when Load/Save methods are implemented for those types.
                    return;

                if (Platform.Type == PlatformType.Android && sourceFormat == ImageFileType.Tiff)// TODO remove this when Load/Save methods are implemented for this type.
                    return;

                RunDrawTest(
                    (game, context, frame) =>
                    {
                        var device = game.GraphicsDevice;
                        var fileName = sourceFormat.ToFileExtension().Substring(1) + "Image";
                        var filePath = "ImageTypes/" + fileName;
                        
                        // Load an image from a file and dispose it.
                        Texture texture;
                        using (var inStream = game.Asset.OpenAsStream(filePath, StreamFlags.None))
                            texture = Texture.Load(device, inStream, loadAsSRGB: true);
                        
                        game.GraphicsDevice.SetBlendState(game.GraphicsDevice.BlendStates.AlphaBlend);
                        game.GraphicsDevice.DrawTexture(texture);
                    },
                    GraphicsProfile.Level_9_1,
                    sourceFormat.ToString());
            }
        }

        private void CheckTexture(Texture texture, byte[] data)
        {
            // Get back the data from the gpu
            var data2 = texture.GetData<byte>();

            // Assert that data are the same
            Assert.That(Utilities.Compare(data, data2), Is.True);

            // Sets new data on the gpu
            data[0] = 1;
            data[31] = 255;
            texture.SetData(data);

            // Get back the data from the gpu
            data2 = texture.GetData<byte>();

            // Assert that data are the same
            Assert.That(Utilities.Compare(data, data2), Is.True);
        }
    }
}