// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    [Description("Check Texture")]
    public class TestTexture
    {
        [Test]
        public void TestTexture1D()
        {
            var device = GraphicsDevice.New();

            // Check texture creation with an array of data, with usage default to later allow SetData
            var data = new byte[256];
            data[0] = 255;
            data[31] = 1;
            var texture = Texture.New1D(device, 256, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

            // Perform texture op
            CheckTexture(texture, data);

            // Release the texture
            texture.Dispose();

            device.Dispose();
        }

        [Test]
        public void TestTexture1DMipMap()
        {
            var device = GraphicsDevice.New();

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

            device.Dispose();
        }

        [Test]
        public void TestTexture2D()
        {
            var device = GraphicsDevice.New();

            // Check texture creation with an array of data, with usage default to later allow SetData
            var data = new byte[256 * 256];
            data[0] = 255;
            data[31] = 1;
            var texture = Texture.New2D(device, 256, 256, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

            // Perform texture op
            CheckTexture(texture, data);

            // Release the texture
            texture.Dispose();

            device.Dispose();
        }

        [Test]
        public void TestTexture2DArray()
        {
            var device = GraphicsDevice.New();

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
            var data1 = texture.GetData<byte>(1, 0);
            Assert.That(data1.Length, Is.EqualTo(data.Length));
            Assert.That(data1[0], Is.EqualTo(1));
            renderTarget1.Dispose();

            // Get a render target on the array 2 (128) with value 2 and get back the data
            var renderTarget2 = texture.ToTextureView(ViewType.Single, 2, 0);
            device.Clear(renderTarget2, new Color4(0xFF000002));
            var data2 = texture.GetData<byte>(2, 0);
            Assert.That(data2.Length, Is.EqualTo(data.Length));
            Assert.That(data2[0], Is.EqualTo(2));
            renderTarget2.Dispose();

            // Get a render target on the array 3 (128) with value 3 and get back the data
            var renderTarget3 = texture.ToTextureView(ViewType.Single, 3, 0);
            device.Clear(renderTarget3, new Color4(0xFF000003));
            var data3 = texture.GetData<byte>(3, 0);
            Assert.That(data3.Length, Is.EqualTo(data.Length));
            Assert.That(data3[0], Is.EqualTo(3));
            renderTarget3.Dispose();

            // Release the texture
            texture.Dispose();

            device.Dispose();
        }

        [Test]
        public void TestTexture2DUnorderedAccess()
        {
            // Force to use Level11 in order to use UnorderedAccessViews
            var device = GraphicsDevice.New(DeviceCreationFlags.None, GraphicsProfile.Level_11_0);

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

            device.Dispose();
        }

        [Test]
        public void TestTexture3D()
        {
            var device = GraphicsDevice.New();

            // Check texture creation with an array of data, with usage default to later allow SetData
            var data = new byte[32 * 32 * 32];
            data[0] = 255;
            data[31] = 1;
            var texture = Texture.New3D(device, 32, 32, 32, PixelFormat.R8_UNorm, data, usage: GraphicsResourceUsage.Default);

            // Perform generate texture checking
            CheckTexture(texture, data);

            // Release the texture
            texture.Dispose();

            device.Dispose();
        }

        [Test]
        public void TestTexture3DRenderTarget()
        {
            var device = GraphicsDevice.New(DeviceCreationFlags.Debug);

            // Check texture creation with an array of data, with usage default to later allow SetData
            var texture = Texture.New3D(device, 32, 32, 32, true, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

            // Get a render target on the 1st mipmap of this texture 3D
            var renderTarget0 = texture.ToTextureView(ViewType.Single, 0, 0);
            device.Clear(renderTarget0, new Color4(0xFF000001));
            var data1 = texture.GetData<byte>(0, 0);
            Assert.That(data1.Length, Is.EqualTo(32*32*32));
            Assert.That(data1[0], Is.EqualTo(1));
            renderTarget0.Dispose();

            // Get a render target on the 2nd mipmap of this texture 3D
            var renderTarget1 = texture.ToTextureView(ViewType.Single, 0, 1);

            // Check that width/height is correctly calculated 
            Assert.That(renderTarget1.ViewWidth, Is.EqualTo(32 >> 1));
            Assert.That(renderTarget1.ViewHeight, Is.EqualTo(32 >> 1));

            device.Clear(renderTarget1, new Color4(0xFF000001));
            var data2 = texture.GetData<byte>(0, 1);
            Assert.That(data2.Length, Is.EqualTo(16*16*16));
            Assert.That(data2[0], Is.EqualTo(1));
            renderTarget1.Dispose();

            // Release the texture
            texture.Dispose();

            device.Dispose();
        }

        [Test]
        public void TestDepthStencilBuffer()
        {
            // Force to create a device
            var device = GraphicsDevice.New(DeviceCreationFlags.Debug, GraphicsProfile.Level_10_0);

            // Check that reaonly is not supported for depth stencil buffer
            Assert.That(Texture.IsDepthStencilReadOnlySupported(device), Is.False);

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

            device.Dispose();
        }

        [Test]
        public void TestDepthStencilBufferWithNativeReadonly()
        {
            //// Without shaders, it is difficult to check this method without accessing internals
            //// Force to create a device
            var device = GraphicsDevice.New(DeviceCreationFlags.None, GraphicsProfile.Level_11_0);

            // Check that reaonly is not supported for depth stencil buffer
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

            device.Dispose();
        }

        /// <summary>
        /// Tests the load save.
        /// </summary>
        /// <remarks>
        /// This test loads several images using <see cref="Texture.Load"/> (on the GPU) and save them to the disk using <see cref="Texture.Save"/>.
        /// The saved image is then compared with the original image to check that the whole chain (CPU -> GPU, GPU -> CPU) is passing correctly
        /// the textures.
        /// </remarks>
        [Test]
        public void TestLoadSave()
        {
            // Change this to stress-test GPU textures
            const int countTest = 1;

            var files = new List<string>();
            var device = GraphicsDevice.New();
            
            var dxsdkDir = Environment.GetEnvironmentVariable("DXSDK_DIR");
            if (string.IsNullOrEmpty(dxsdkDir))
                throw new NotSupportedException("Install DirectX SDK June 2010 to run this test (DXSDK_DIR env variable is missing).");

            files.AddRange(Directory.EnumerateFiles(Path.Combine(dxsdkDir, @"Samples\Media"), "*.dds", SearchOption.AllDirectories));
            files.AddRange(Directory.EnumerateFiles(Path.Combine(dxsdkDir, @"Samples\Media"), "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.EnumerateFiles(Path.Combine(dxsdkDir, @"Samples\Media"), "*.bmp", SearchOption.AllDirectories));

            var excludeList = new List<string>()
                                  {
                                      "RifleStock1Bump.dds"  // This file is in BC1 format but size is not a multiple of 4, so It can't be loaded as a texture, so we skip it.
                                  };

            var testMemoryBefore = GC.GetTotalMemory(true);
            for (int i = 0; i < countTest; i++)
            {
                var clock = Stopwatch.StartNew();
                foreach (var file in files)
                {
                    if (excludeList.Contains(Path.GetFileName(file), StringComparer.InvariantCultureIgnoreCase))
                        continue;

                    // Load an image from a file and dispose it.

                    Texture texture;

                    using (var fileStream = File.OpenRead(file))
                        texture = Texture.Load(device, fileStream);

                    // Copy GPU to GPU
                    var texture2 = texture.Clone();
                    device.Copy(texture, texture2);

                    // dispose original
                    texture.Dispose();
                    texture = texture2;

                    var localPath = Path.GetFileName(file);
                    using (var outStream = File.OpenWrite(localPath))
                        texture.Save(outStream, ImageFileType.Dds);
                    texture.Dispose();

                    using (var fileStream = File.OpenRead(file))
                    {
                        using (var originalImage = Image.Load(fileStream))
                        {
                            using (var fileStream1 = File.OpenRead(localPath))
                            {
                                using (var textureImage = Image.Load(fileStream1))
                                {
                                    TestImage.CompareImage(originalImage, textureImage, file);
                                }
                            }
                        }
                    }
                }
                var time = clock.ElapsedMilliseconds;
                clock.Stop();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                var testMemoryAfter = GC.GetTotalMemory(true);
                Console.WriteLine("[{0}] Test loading {1} GPU texture / saving to dds and compare with original Memory {2} delta bytes, in {3}ms", i, files.Count, testMemoryAfter - testMemoryBefore, time);

                ////if ((i % 10) == 0 )
                //{
                //    Console.WriteLine("------------------------------------------------------");
                //    Console.WriteLine("Lived Ojbects");
                //    Console.WriteLine("------------------------------------------------------");
                //    deviceDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
                //}
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

#endif