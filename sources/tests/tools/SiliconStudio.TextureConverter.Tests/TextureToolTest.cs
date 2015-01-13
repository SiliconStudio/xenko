// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using System.Collections.Generic;

using NUnit.Framework;

namespace SiliconStudio.TextureConverter.Tests
{
    [TestFixture]
    class TextureToolTest
    {
        TextureTool texTool;

        [SetUp]
        public void SetUp()
        {
            texTool = new TextureTool();
        }

        [TearDown]
        public void TearDown()
        {
            texTool.Dispose();
        }

        [Test]
        public void LoadTest()
        {
            TexImage img;

            img = texTool.Load(TestTools.InputTestFolder + "stones.png");
            Assert.IsTrue(img.ArraySize == 1);
            Assert.IsTrue(img.Width == 512);
            Assert.IsTrue(img.Height == 512);
            Assert.IsTrue(img.Depth == 1);
            Assert.IsTrue(img.Format == Paradox.Graphics.PixelFormat.B8G8R8A8_UNorm);
            img.Dispose();

            try
            {
                img = texTool.Load(TestTools.InputTestFolder + "elina.pkm");
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }
        }

        [Test]
        public void DecompressTest()
        {
            TexImage img;
            int mipmapCount, arraySize, width, height, depth, subImageArrayLenght;

            // ------------------- Test with BC3 image -------------------
            img = texTool.Load(TestTools.InputTestFolder + "TextureArray_WMipMaps_BC3.dds");
            Assert.IsTrue(img.Format == Paradox.Graphics.PixelFormat.BC3_UNorm);
            mipmapCount = img.MipmapCount;
            arraySize = img.ArraySize;
            width = img.Width;
            height = img.Height;
            depth = img.Depth;
            subImageArrayLenght = img.SubImageArray.Length;

            texTool.Decompress(img, false);
            Assert.IsTrue(img.Format == Paradox.Graphics.PixelFormat.R8G8B8A8_UNorm);
            Assert.IsTrue(mipmapCount == img.MipmapCount);
            Assert.IsTrue(arraySize == img.ArraySize);
            Assert.IsTrue(width == img.Width);
            Assert.IsTrue(height == img.Height);
            Assert.IsTrue(depth == img.Depth);
            Assert.IsTrue(subImageArrayLenght == img.SubImageArray.Length);

            Assert.IsTrue(TestTools.ComputeSHA1(img.Data, img.DataSize).Equals(TestTools.GetInstance().Checksum["DecompressTest_TextureArray_WMipMaps_BC3.dds"]));
            img.Dispose();

            // ------------------- Test with uncompress image -------------------
            img = texTool.Load(TestTools.InputTestFolder + "stones.png");
            texTool.Decompress(img, false);
            Assert.IsTrue(img.Format == Paradox.Graphics.PixelFormat.B8G8R8A8_UNorm); //FITexLibrary loads image in BGRA order...
            img.Dispose();
        }


        [TestCase("stones.png", Orientation.Horizontal)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", Orientation.Horizontal)]
        [TestCase("TextureCube_WMipMaps_PVRTC2_4bpp.pvr", Orientation.Vertical)]
        public void FlipTest(string file, Orientation orientation)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);

            texTool.Flip(image, orientation);
            image.Update();

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Flip_" + orientation + "_" + image.Name]));
            //Console.WriteLine("TextureTool_Flip_" + orientation + "_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }


        [TestCase("Texture3D_WOMipMaps_BC3.dds")]
        public void PreMultiplyAlphaTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);

            texTool.PreMultiplyAlpha(image);

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_PreMultiplyAlpha_" + image.Name]));
            //Console.WriteLine("TextureTool_PreMultiplyAlpha_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }


        [TestCase("stones.png", Paradox.Graphics.PixelFormat.BC3_UNorm)]
        [TestCase("stones.png", Paradox.Graphics.PixelFormat.PVRTC_II_4bpp)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", Paradox.Graphics.PixelFormat.PVRTC_II_4bpp)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", Paradox.Graphics.PixelFormat.BC3_UNorm)]
        public void CompressTest(string filename, Paradox.Graphics.PixelFormat format)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + filename);
            texTool.Compress(image, format);
            Assert.IsTrue(image.Format == format);

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Compress_" + format + "_" + image.Name]));
            //Console.WriteLine("TextureTool_Compress_" + format + "_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }

        [TestCase("stones.png", Filter.MipMapGeneration.Box)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", Filter.MipMapGeneration.Linear)]
        public void GenerateMipMapTest(string file, Filter.MipMapGeneration filter)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            texTool.GenerateMipMaps(image, filter);
            Assert.IsTrue(image.MipmapCount > 1);

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_GenerateMipMap_" + filter + "_" + image.Name]));
            //Console.WriteLine("TextureTool_GenerateMipMap_" + filter + "_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void CorrectGammaTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            texTool.CorrectGamma(image, 1/2.2);
            image.Update();

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_CorrectGamma_" + image.Name]));
            //Console.WriteLine("TextureTool_CorrectGamma_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void ConvertToParadoxImageTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            var pdx = texTool.ConvertToParadoxImage(image);
            Assert.IsTrue(pdx.TotalSizeInBytes == image.DataSize);
            Assert.IsTrue(pdx.Description.MipLevels == image.MipmapCount);
            image.Dispose();
            pdx.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void LoadParadoxImageTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);

            var pdx = texTool.ConvertToParadoxImage(image);

            TexImage pdxImage = texTool.Load(pdx);

            Assert.IsTrue(image.Equals(pdxImage));

            pdx.Dispose();
            image.Dispose();
            pdxImage.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void GenerateNormalMapTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            var normal = texTool.GenerateNormalMap(image, 0.5f);

            Assert.IsTrue(TestTools.ComputeSHA1(normal.Data, normal.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_GenerateNormalMap_" + image.Name]));
            //Console.WriteLine("TextureTool_GenerateNormalMap_" + image.Name + "." + TestTools.ComputeSHA1(normal.Data, normal.DataSize));

            normal.Dispose();
            image.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void RescaleTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            int width = image.Width;
            int height = image.Height;
            Assert.IsTrue(image.MipmapCount > 1);

            texTool.Rescale(image, 0.5f, 0.5f, Filter.Rescaling.Bicubic);
            Assert.IsTrue(image.Width == width / 2);
            Assert.IsTrue(image.Height == height / 2);
            Assert.IsTrue(image.MipmapCount == 1);

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Rescale_" + image.Name]));
            //Console.WriteLine("TextureTool_Rescale_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void ResizeTest(string file)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + file);
            int width = image.Width;
            int height = image.Height;
            Assert.IsTrue(image.MipmapCount > 1);

            texTool.Resize(image, width/2, height/2, Filter.Rescaling.Bicubic);
            Assert.IsTrue(image.Width == width / 2);
            Assert.IsTrue(image.Height == height / 2);
            Assert.IsTrue(image.MipmapCount == 1);

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Rescale_" + image.Name]));
            //Console.WriteLine("TextureTool_Rescale_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }

        [TestCase("TextureArray_WMipMaps_BC3.dds")]
        public void SwitchChannelTest(string file)
        {
            var image = texTool.Load(TestTools.InputTestFolder + file);
            var isInBgraOrder = image.Format.IsInBGRAOrder();

            texTool.SwitchChannel(image);
            image.Update();

            Assert.IsTrue(isInBgraOrder != image.Format.IsInBGRAOrder());

            Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_SwitchChannel_" + image.Name]));
            //Console.WriteLine("TextureTool_SwitchChannel_" + image.Name + "." + TestTools.ComputeSHA1(image.Data, image.DataSize));

            image.Dispose();
        }


        [TestCase("TextureArray_WMipMaps_BGRA8888.dds", ".pvr", Paradox.Graphics.PixelFormat.None, 16)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", ".pvr", Paradox.Graphics.PixelFormat.ETC2_RGBA, 0)]
        [TestCase("TextureArray_WMipMaps_BC3.dds", ".pvr", Paradox.Graphics.PixelFormat.None, 0)]
        public void SaveTest(string input, string extension, Paradox.Graphics.PixelFormat compressionFormat, int minimumMipmapSize)
        {
            TexImage image = texTool.Load(TestTools.InputTestFolder + input);

            string output = Path.GetFileNameWithoutExtension(input) + extension;

            if (compressionFormat == Paradox.Graphics.PixelFormat.None)
            {
                texTool.Save(image, TestTools.TempFolder + output, minimumMipmapSize);
            }
            else
            {
                texTool.Save(image, TestTools.TempFolder + output, compressionFormat, minimumMipmapSize);
            }

            Assert.IsTrue(File.Exists(TestTools.TempFolder + output));
            var loaded = texTool.Load(TestTools.TempFolder + output);

            Assert.IsTrue(TestTools.ComputeSHA1(loaded.Data, loaded.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Save_" + compressionFormat + "_" + minimumMipmapSize + "_" + loaded.Name]));
            //Console.WriteLine("TextureTool_Save_" + compressionFormat + "_" + minimumMipmapSize + "_" + loaded.Name + "." + TestTools.ComputeSHA1(loaded.Data, loaded.DataSize));

            File.Delete(TestTools.TempFolder + output);

            loaded.Dispose();
            image.Dispose();
        }

        [TestCase("TextureCube_WMipMaps_BC3.dds", ".pvr", Filter.Rescaling.CatmullRom, Paradox.Graphics.PixelFormat.ETC2_RGBA)]
        [TestCase("TextureArray_WMipMaps_PVRTC2_4bpp.pvr", ".dds", Filter.Rescaling.Nearest, Paradox.Graphics.PixelFormat.BC3_UNorm)]
        [TestCase("TextureCube_WMipMaps_ATC_RGBA_Explicit.pdx", ".dds", Filter.Rescaling.Lanczos3, Paradox.Graphics.PixelFormat.BC3_UNorm)]
        [TestCase("duck.jpg", ".dds", Filter.Rescaling.Box, Paradox.Graphics.PixelFormat.BC3_UNorm)]
        [TestCase("duck.jpg", ".pvr", Filter.Rescaling.BSpline, Paradox.Graphics.PixelFormat.PVRTC_II_4bpp)]
        public void ProcessingTest(string source, string extension, Filter.Rescaling rescaleFiler, Paradox.Graphics.PixelFormat format)
        {
            var image = texTool.Load(TestTools.InputTestFolder + source);

            texTool.CorrectGamma(image, 2.2);

            texTool.Rescale(image, 0.5f, 0.5f, rescaleFiler);

            texTool.GenerateMipMaps(image, Filter.MipMapGeneration.Box);

            var normalMap = texTool.GenerateNormalMap(image, 4);

            texTool.CorrectGamma(normalMap, 1/2.2);

            string output = "TextureTool_ProcessingTest_NormalMap" + rescaleFiler + "_" + format + "_" + source + extension;
            texTool.Save(normalMap, TestTools.TempFolder + output, format, normalMap.Width / 2);
            normalMap.Dispose();

            Assert.IsTrue(TestTools.ComputeSHA1(TestTools.TempFolder + output).Equals(TestTools.GetInstance().Checksum[output]));
            //Console.WriteLine(output + "." + TestTools.ComputeSHA1(TestTools.TempFolder + output));
            File.Delete(TestTools.TempFolder + output);

            texTool.Flip(image, Orientation.Horizontal);

            texTool.CorrectGamma(image, 1/2.2);

            output = "TextureTool_ProcessingTest_" + rescaleFiler + "_" + format + "_" + source + extension;
            texTool.Save(image, TestTools.TempFolder + output, format, 4);
            image.Dispose();

            Assert.IsTrue(TestTools.ComputeSHA1(TestTools.TempFolder + output).Equals(TestTools.GetInstance().Checksum[output]));
            //Console.WriteLine(output + "." + TestTools.ComputeSHA1(TestTools.TempFolder + output));
            File.Delete(TestTools.TempFolder + output);

            image.Dispose();
        }


        [Test]
        public void CreateAtlasTest()
        {
            string[] fileList = Directory.GetFiles(TestTools.InputTestFolder + "atlas/");
            var list = new List<TexImage>(fileList.Length);

            foreach (string filePath in fileList)
            {
                list.Add(texTool.Load(filePath));
            }

            var atlas = texTool.CreateAtlas(list);

            Assert.IsTrue(TestTools.ComputeSHA1(atlas.Data, atlas.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_CreateAtlas"]));
            //Console.WriteLine("TextureTool_CreateAtlas." + TestTools.ComputeSHA1(atlas.Data, atlas.DataSize));

            atlas.Dispose();

            var another = texTool.Load(fileList[fileList.Length - 1]);
            texTool.Compress(another, Paradox.Graphics.PixelFormat.BC3_UNorm);
            list.Add(another);

            try
            {
                atlas = texTool.CreateAtlas(list);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [TestCase("atlas_WMipMaps.dds", "stones.png")]
        public void ExtractAtlasTest(string atlasFile, string textureName)
        {
            var atlas = texTool.LoadAtlas(TestTools.InputTestFolder + atlasFile);
            var extracted = texTool.Extract(atlas, textureName, 16);

            Assert.IsTrue(TestTools.ComputeSHA1(extracted.Data, extracted.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_ExtractAtlas_" + atlasFile + "_" + textureName]));
            //Console.WriteLine("TextureTool_ExtractAtlas_" + atlasFile + "_" + textureName + "." + TestTools.ComputeSHA1(extracted.Data, extracted.DataSize));

            extracted.Dispose();
            atlas.Dispose();
        }


        [Test]
        public void ExtractAtlasFailTest()
        {
            var atlas = texTool.LoadAtlas(TestTools.InputTestFolder + "atlas_WMipMaps.dds");

            try
            {
                var extracted = texTool.Extract(atlas, "coucoucoucoucoucoucoucoucoucoucoucou", 16);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            atlas.Dispose();
        }


        [TestCase("atlas_WMipMaps.dds")]
        public void ExtractAllAtlasTest(string atlasFile)
        {
            string[] fileList = Directory.GetFiles(TestTools.InputTestFolder + "atlas/");
            var list = new List<TexImage>(fileList.Length);

            foreach (string filePath in fileList)
            {
                list.Add(texTool.Load(filePath));
            }

            var atlas = texTool.CreateAtlas(list);

            var extracted = texTool.ExtractAll(atlas);

            Assert.IsTrue(extracted.Count == list.Count);

            foreach (var image in extracted)
            {
                Assert.IsTrue(TestTools.ComputeSHA1(image.Data, image.DataSize).Equals(TestTools.GetInstance().Checksum["ExtractAll_" + image.Name]));
                image.Dispose();
            }

            atlas.Dispose();

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [TestCase("atlas_WMipMaps.dds", "atlas/square256_2.png")]
        public void UpdateAtlasTest(string atlasFile, string textureName)
        {
            var atlas = texTool.LoadAtlas(TexAtlas.TexLayout.Import(TestTools.InputTestFolder + Path.GetFileNameWithoutExtension(atlasFile) + TexAtlas.TexLayout.Extension), TestTools.InputTestFolder + atlasFile);
            var updateTexture = texTool.Load(TestTools.InputTestFolder + textureName);

            texTool.Update(atlas, updateTexture);

            Assert.IsTrue(TestTools.ComputeSHA1(atlas.Data, atlas.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_UpdateAtlas_" + atlasFile + "_" + Path.GetFileName(textureName)]));
            //Console.WriteLine("TextureTool_UpdateAtlas_" + atlasFile + "_" + Path.GetFileName(textureName) + "." + TestTools.ComputeSHA1(atlas.Data, atlas.DataSize));

            atlas.Dispose();
            updateTexture.Dispose();
        }


        [Test]
        public void UpdateAtlasFailTest()
        {
            var atlas = texTool.LoadAtlas(TexAtlas.TexLayout.Import(TestTools.InputTestFolder + Path.GetFileNameWithoutExtension("atlas_WMipMaps.dds") + TexAtlas.TexLayout.Extension), TestTools.InputTestFolder + "atlas_WMipMaps.dds");
            var updateTexture = texTool.Load(TestTools.InputTestFolder + "atlas/square256_2.png");

            try
            {
                updateTexture.Name = "coucoucoucoucoucoucoucoucoucoucoucou";
                texTool.Update(atlas, updateTexture);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            try
            {
                texTool.Update(atlas, updateTexture, "coucoucoucoucoucoucoucoucoucoucoucou");
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }
        }


        [TestCase("atlas/stones256.png", "atlas/square256.png")]
        public void CreateArrayTest(string file1, string file2)
        {
            var list = new List<TexImage>();
            for (int i = 0; i < 5; ++i)
            {
                list.Add(texTool.Load(TestTools.InputTestFolder + file1));
                list.Add(texTool.Load(TestTools.InputTestFolder + file2));
            }

            var array = texTool.CreateTextureArray(list);

            Assert.IsTrue(TestTools.ComputeSHA1(array.Data, array.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_CreateArray_" + Path.GetFileName(file1) + "_" + Path.GetFileName(file2)]));
            //Console.WriteLine("TextureTool_CreateArray_" + Path.GetFileName(file1) + "_" + Path.GetFileName(file2) + "." + TestTools.ComputeSHA1(array.Data, array.DataSize));

            array.Dispose();

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [Test]
        public void CreateArrayFailTest()
        {
            var list = new List<TexImage>();
            list.Add(texTool.Load(TestTools.InputTestFolder + "atlas/stones256.png"));

            var other = texTool.Load(TestTools.InputTestFolder + "atlas/stones256.png");
            texTool.Rescale(other, 0.5f, 0.5f, Filter.Rescaling.Bilinear);
            list.Add(other);

            try
            {
                var array = texTool.CreateTextureArray(list);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [TestCase("atlas/stones256.png", "atlas/square256.png")]
        public void CreateCubeTest(string file1, string file2)
        {
            var list = new List<TexImage>();
            for (int i = 0; i < 3; ++i)
            {
                list.Add(texTool.Load(TestTools.InputTestFolder + file1));
                list.Add(texTool.Load(TestTools.InputTestFolder + file2));
            }

            var array = texTool.CreateTextureCube(list);

            Assert.IsTrue(TestTools.ComputeSHA1(array.Data, array.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_CreateCube_" + Path.GetFileName(file1) + "_" + Path.GetFileName(file2)]));
            //Console.WriteLine("TextureTool_CreateCube_" + Path.GetFileName(file1) + "_" + Path.GetFileName(file2) + "." + TestTools.ComputeSHA1(array.Data, array.DataSize));

            array.Dispose();

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [Test]
        public void CreateCubeFailTest()
        {
            var list = new List<TexImage>();
            for (int i = 0; i < 7; ++i)
            {
                list.Add(texTool.Load(TestTools.InputTestFolder + "atlas/stones256.png"));
            }

            try
            {
                var array = texTool.CreateTextureCube(list);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            foreach (var image in list)
            {
                image.Dispose();
            }
        }


        [TestCase("array_WMipMaps.dds", 4)]
        public void ExtractTest(string arrayFile, int indice)
        {
            TexImage array = texTool.Load(TestTools.InputTestFolder + arrayFile);

            var extracted = texTool.Extract(array, indice);

            //Console.WriteLine("TextureTool_Extract_" + indice + "_" + arrayFile + "." + TestTools.ComputeSHA1(extracted.Data, extracted.DataSize));
            Assert.IsTrue(TestTools.ComputeSHA1(extracted.Data, extracted.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Extract_" + indice + "_" + arrayFile]));

            extracted.Dispose();
            array.Dispose();
        }


        [Test]
        public void ExtractFailTest()
        {
            TexImage array = texTool.Load(TestTools.InputTestFolder + "array_WMipMaps.dds");

            try
            {
                var extracted = texTool.Extract(array, array.ArraySize);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            array.Dispose();
        }


        [TestCase("atlas/stones256.png", "atlas/square256.png")]
        public void ExtractAllTest(string file1, string file2)
        {
            var list = new List<TexImage>();
            for (int i = 0; i < 5; ++i)
            {
                list.Add(texTool.Load(TestTools.InputTestFolder + file1));
                //Console.WriteLine("ArrayTexLibrary_ExtractAll_" + Path.GetFileName(file1) + "." + TestTools.ComputeSHA1(temp.Data, temp.DataSize));

                list.Add(texTool.Load(TestTools.InputTestFolder + file2));
                //Console.WriteLine("ArrayTexLibrary_ExtractAll_" + Path.GetFileName(file2) + "." + TestTools.ComputeSHA1(temp.Data, temp.DataSize));
            }

            var array = texTool.CreateTextureArray(list);

            var extracted = texTool.ExtractAll(array);

            Assert.IsTrue(list.Count == extracted.Count);

            for (int i = 0; i < array.ArraySize; ++i)
            {
                Assert.IsTrue(TestTools.ComputeSHA1(extracted[i].Data, extracted[i].DataSize).Equals(TestTools.GetInstance().Checksum["ExtractAll_" + list[i].Name]));
                extracted[i].Dispose();
            }

            array.Dispose();

            foreach (var image in list)
            {
                image.Dispose();
            }
        }

        [TestCase("array_WMipMaps.dds", "atlas/square256.png", 3)]
        public void InsertTest(string arrayFile, string newTexture, int indice)
        {
            var array = texTool.Load(TestTools.InputTestFolder + arrayFile);
            var texture = texTool.Load(TestTools.InputTestFolder + newTexture);
            texTool.Compress(texture, Paradox.Graphics.PixelFormat.BC3_UNorm);

            texTool.Insert(array, texture, indice);

            //Console.WriteLine("TextureTool_Insert_" + indice + "_" + Path.GetFileName(newTexture) + "_" + arrayFile + "." + TestTools.ComputeSHA1(array.Data, array.DataSize));
            Assert.IsTrue(TestTools.ComputeSHA1(array.Data, array.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Insert_" + indice + "_" + Path.GetFileName(newTexture) + "_" + arrayFile]));

            try
            {
                texTool.Insert(array, texture, array.ArraySize+1);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            array.Dispose();
            texture.Dispose();
        }

        [TestCase("array_WMipMaps.dds", 3)]
        public void RemoveTest(string arrayFile, int indice)
        {
            var array = texTool.Load(TestTools.InputTestFolder + arrayFile);

            texTool.Remove(array, indice);

            //Console.WriteLine("TextureTool_Remove_" + indice + "_" + arrayFile + "." + TestTools.ComputeSHA1(array.Data, array.DataSize));
            Assert.IsTrue(TestTools.ComputeSHA1(array.Data, array.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_Remove_" + indice + "_" + arrayFile]));

            try
            {
                texTool.Remove(array, array.ArraySize);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }

            array.Dispose();
        }


        [TestCase("array_WMipMaps.dds", "atlas/square256_2.png", 0)]
        public void UpdateArrayTest(string arrayFile, string textureName, int indice)
        {
            var array = texTool.Load(TestTools.InputTestFolder + arrayFile);
            var updateTexture = texTool.Load(TestTools.InputTestFolder + textureName);

            texTool.Update(array, updateTexture, indice);

            Assert.IsTrue(TestTools.ComputeSHA1(array.Data, array.DataSize).Equals(TestTools.GetInstance().Checksum["TextureTool_UpdateArray_" + arrayFile + "_" + indice + "_" + Path.GetFileName(textureName)]));
            //Console.WriteLine("TextureTool_UpdateArray_" + arrayFile + "_" + indice + "_" + Path.GetFileName(textureName) + "." + TestTools.ComputeSHA1(array.Data, array.DataSize));

            array.Dispose();
            updateTexture.Dispose();

            try
            {
                texTool.Update(array, updateTexture, array.ArraySize);
                Assert.IsTrue(false);
            }
            catch (TextureToolsException)
            {
                Assert.IsTrue(true);
            }
        }

    }
}
