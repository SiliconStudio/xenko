// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;
using SiliconStudio.TextureConverter.Requests;
using SiliconStudio.TextureConverter.TexLibraries;

namespace SiliconStudio.TextureConverter.Tests
{
    [TestFixture]
    class ParadoxTexLibraryTest
    {
        ParadoxTexLibrary library;

        [TestFixtureSetUp]
        public void TestSetUp()
        {
            library = new ParadoxTexLibrary();
            Assert.IsTrue(library.SupportBGRAOrder());
        }

        [TestFixtureTearDown]
        public void TestTearDown()
        {
            library.Dispose();
        }


        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureCube_WMipMaps_RGBA8888.pdx")]
        public void StartLibraryTest(string file)
        {
            TexImage image = TestTools.Load(library, file);

            TexLibraryTest.StartLibraryTest(image, library);

            image.Dispose();
        }


        [Test]
        public void CanHandleRequestTest()
        {
            TexImage image = TestTools.Load(library, "Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx");
            Assert.IsFalse(library.CanHandleRequest(image, new DecompressingRequest(false)));
            Assert.IsFalse(library.CanHandleRequest(image, new LoadingRequest(new TexImage())));
            Assert.IsTrue(library.CanHandleRequest(image, new LoadingRequest(Paradox.Graphics.Image.New1D(5, 0, Paradox.Graphics.PixelFormat.ATC_RGBA_Explicit))));
            Assert.IsTrue(library.CanHandleRequest(image, new LoadingRequest("TextureArray_WMipMaps_BC3.dds", false)));
            Assert.IsTrue(library.CanHandleRequest(image, new ExportRequest("TextureArray_WMipMaps_BC3.pdx", 0)));
            Assert.IsTrue(library.CanHandleRequest(image, new ExportToParadoxRequest()));
            image.Dispose();
        }


        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureCube_WMipMaps_RGBA8888.pdx")]
        public void ExportTest(string file)
        {
            TexImage image = TestTools.Load(library, file);

            TexLibraryTest.ExportTest(image, library, file);

            image.Dispose();
        }

        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx", 4)]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx", 512)]
        [TestCase("TextureCube_WMipMaps_RGBA8888.pdx", 16)]
        public void ExportTest(string file, int mipMipMapSize)
        {
            TexImage image = TestTools.Load(library, file);

            TexLibraryTest.ExportMinMipMapTest(image, library, mipMipMapSize);

            image.Dispose();
        }

        [TestCase("Texture3D_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureArray_WMipMaps_ATC_RGBA_Explicit.pdx")]
        [TestCase("TextureCube_WMipMaps_RGBA8888.pdx")]
        public void ExportToParadoxTest(string file)
        {
            TexImage image = TestTools.Load(library, file);

            ExportToParadoxRequest request = new ExportToParadoxRequest();
            library.Execute(image, request);

            var pdx = request.PdxImage;

            Assert.IsTrue(pdx.TotalSizeInBytes == image.DataSize);
            Assert.IsTrue(pdx.Description.MipLevels == image.MipmapCount);
            Assert.IsTrue(pdx.Description.Width == image.Width);
            Assert.IsTrue(pdx.Description.Height == image.Height);

            image.Dispose();
        }
    }
}
