// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;
using SiliconStudio.TextureConverter.Requests;
using SiliconStudio.TextureConverter.TexLibraries;

namespace SiliconStudio.TextureConverter.Tests
{
    [TestFixture]
    class AtitcTexLibraryTest
    {
        AtitcTexLibrary library;
        ParadoxTexLibrary paraLib;

        [TestFixtureSetUp]
        public void TestSetUp()
        {
            library = new AtitcTexLibrary();
            paraLib = new ParadoxTexLibrary();
            Assert.IsFalse(library.SupportBGRAOrder());
        }

        [TestFixtureTearDown]
        public void TestTearDown()
        {
            library.Dispose();
            paraLib.Dispose();
        }

        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        public void StartLibraryTest(string file)
        {
            TexImage image = LoadInput(file);

            TexLibraryTest.StartLibraryTest(image, library);

            AtitcTextureLibraryData libraryData = (AtitcTextureLibraryData)image.LibraryData[library];
            Assert.IsTrue(libraryData.Textures.Length == image.SubImageArray.Length);

            image.Dispose();
        }

        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        public void EndLibraryTest(string file)
        {
            TexImage image = LoadInput(file);

            IntPtr buffer;

            buffer = image.SubImageArray[0].Data;
            library.Execute(image, new DecompressingRequest(false));

            Assert.IsTrue(image.Format == Paradox.Graphics.PixelFormat.R8G8B8A8_UNorm); // The images features are updated with the call to Execute
            Assert.IsTrue(image.SubImageArray[0].Data == buffer); // The sub images are only updated on the call to EndLibrary

            library.EndLibrary(image);

            Assert.IsTrue(image.SubImageArray[0].Data != buffer);

            image.Dispose();
        }

        [Test]
        public void CanHandleRequestTest()
        {
            TexImage image = LoadInput("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx");

            Assert.IsTrue(library.CanHandleRequest(image, new DecompressingRequest(false)));
            Assert.IsTrue(library.CanHandleRequest(image, new CompressingRequest(Paradox.Graphics.PixelFormat.ATC_RGBA_Explicit)));
            Assert.IsFalse(library.CanHandleRequest(image, new CompressingRequest(Paradox.Graphics.PixelFormat.BC3_UNorm)));

            image.Dispose();
        }


        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureCube_WMipMaps_ATC_RGBA_Explicit.pdx")]
        public void DecompressTest(string file)
        {
            TexImage image = LoadInput(file);

            TexLibraryTest.DecompressTest(image, library);

            image.Dispose();
        }

        [TestCase("Texture3D_WMipMap_RGBA8888.pdx", Paradox.Graphics.PixelFormat.ATC_RGBA_Explicit)]
        [TestCase("TextureArray_WMipMaps_RGBA8888.pdx", Paradox.Graphics.PixelFormat.ATC_RGBA_Interpolated)]
        [TestCase("TextureCube_WMipMaps_RGBA8888.pdx", Paradox.Graphics.PixelFormat.ATC_RGBA_Explicit)]
        public void CompressTest(string file, Paradox.Graphics.PixelFormat format)
        {
            TexImage image = LoadInput(file);

            TexLibraryTest.CompressTest(image, library, format);

            image.Dispose();
        }

        private TexImage LoadInput(string file)
        {
            var image = TestTools.Load(paraLib, file);
            library.StartLibrary(image);
            return image;
        }
    }
}
