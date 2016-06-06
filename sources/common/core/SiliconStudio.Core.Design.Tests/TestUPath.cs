// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NUnit.Framework;
using SiliconStudio.Core.IO;
// ReSharper disable ObjectCreationAsStatement

namespace SiliconStudio.Core.Design.Tests
{
    [TestFixture]
    public class TestUPath
    {
        [Test]
        public void TestUFileConstructor()
        {
            Assert.DoesNotThrow(() => new UFile(null));
            Assert.DoesNotThrow(() => new UFile(""));
            Assert.DoesNotThrow(() => { var s = "a"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = ".txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b/c/d.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b/c/.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b/c/d.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b/c/.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/d.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.Throws<ArgumentException>(() => new UFile("a\""));
            Assert.Throws<ArgumentException>(() => new UFile("*.txt"));
            Assert.Throws<ArgumentException>(() => new UFile("/a/"));
            Assert.Throws<ArgumentException>(() => new UFile("/"));
            Assert.Throws<ArgumentException>(() => new UFile("E:/"));
            Assert.Throws<ArgumentException>(() => new UFile("E:"));
            Assert.Throws<ArgumentException>(() => new UFile("E:e"));
        }

        [Test]
        public void TestUDirectoryConstructor()
        {
            Assert.DoesNotThrow(() => new UDirectory(null));
            Assert.DoesNotThrow(() => new UDirectory(""));
            Assert.DoesNotThrow(() => { var s = "a"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a.txt/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = ".txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b/c/d.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "a/b/c/.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b/c/d.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/b/c/.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a.txt/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/d.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/.txt"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "/a/"; new UDirectory(s); new UDirectory(s.Replace('/', '\\')); });
            Assert.Throws<ArgumentException>(() => new UDirectory("*.txt"));
            Assert.Throws<ArgumentException>(() => new UDirectory("E:e"));
        }

        [Test]
        public void TestUPathFullPath()
        {
            // TODO (include tests with parent and self paths .. and .)
        }

        [Test]
        public void TestUPathHasDrive()
        {
            // TODO
        }

        [Test]
        public void TestUPathHasDirectory()
        {
            Assert.True(new UFile("/a/b.txt").HasDirectory);
            Assert.True(new UFile("/a/b/c.txt").HasDirectory);
            Assert.True(new UFile("/a/b/c").HasDirectory);
            Assert.True(new UFile("/a.txt").HasDirectory);
            Assert.True(new UFile("E:/a.txt").HasDirectory);
            Assert.True(new UFile("E:/a/b.txt").HasDirectory);
            Assert.True(new UFile("E:/a/b/c.txt").HasDirectory);
            Assert.True(new UFile("E:/a/b/c").HasDirectory);
            Assert.True(new UDirectory("/a/b/c").HasDirectory);
            Assert.True(new UDirectory("E:/a/b/c").HasDirectory);
            Assert.True(new UDirectory("/a").HasDirectory);
            Assert.True(new UDirectory("E:/a").HasDirectory);
            Assert.True(new UDirectory("/").HasDirectory);
            Assert.True(new UDirectory("E:/").HasDirectory);
            Assert.True(new UDirectory("E:").HasDirectory);
            Assert.False(new UFile("a.txt").HasDirectory);
            Assert.False(new UFile("a").HasDirectory);
        }

        [Test]
        public void TestUPathIsRelativeAndIsAbsolute()
        {
            var assert = new Action<UPath, bool>((x, isAbsolute) =>
            {
                Assert.AreEqual(isAbsolute, x.IsAbsolute);
                Assert.AreEqual(!isAbsolute, x.IsRelative);
            });
            assert(new UFile("/a/b/c.txt"), true);
            assert(new UFile("E:/a/b/c.txt"), true);
            assert(new UDirectory("/c.txt"), true);
            assert(new UDirectory("/"), true);
            assert(new UFile("a/b/c.txt"), false);
            assert(new UFile("../c.txt"), false);
        }

        [Test]
        public void TestUPathIsFile()
        {
            // TODO
        }

        [Test]
        public void TestUPathPathType()
        {
            // TODO
        }

        [Test]
        public void TestUPathIsNullOrEmpty()
        {
            Assert.True(UPath.IsNullOrEmpty(new UFile(null)));
            Assert.True(UPath.IsNullOrEmpty(new UFile("")));
            Assert.True(UPath.IsNullOrEmpty(new UFile(" ")));
            Assert.True(UPath.IsNullOrEmpty(new UDirectory(null)));
            Assert.True(UPath.IsNullOrEmpty(new UDirectory("")));
            Assert.True(UPath.IsNullOrEmpty(new UDirectory(" ")));
            Assert.True(UPath.IsNullOrEmpty(null));
            Assert.False(UPath.IsNullOrEmpty(new UFile("a")));
            Assert.False(UPath.IsNullOrEmpty(new UDirectory("a")));
            Assert.False(UPath.IsNullOrEmpty(new UDirectory("C:/")));
            Assert.False(UPath.IsNullOrEmpty(new UDirectory("/")));
        }

        [Test]
        public void TestUPathGetDrive()
        {
            // TODO
        }

        [Test]
        public void TestUPathGetDirectory()
        {
            // TODO
        }

        [Test]
        public void TestUPathGetParent()
        {
            // TODO
        }

        [Test]
        public void TestUPathGetFullDirectory()
        {
            Assert.AreEqual(new UDirectory("/a"), new UFile("/a/b.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/a/b"), new UFile("/a/b/c.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/a/b"), new UFile("/a/b/c").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/"), new UFile("/a.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/"), new UFile("E:/a.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/a"), new UFile("E:/a/b.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/a/b"), new UFile("E:/a/b/c.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/a/b"), new UFile("E:/a/b/c").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/a/b/c"), new UDirectory("/a/b/c").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/a/b/c"), new UDirectory("E:/a/b/c").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/a"), new UDirectory("/a").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/a"), new UDirectory("E:/a").GetFullDirectory());
            Assert.AreEqual(new UDirectory("/"), new UDirectory("/").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/"), new UDirectory("E:/").GetFullDirectory());
            Assert.AreEqual(new UDirectory(null), new UFile("a.txt").GetFullDirectory());
            Assert.AreEqual(new UDirectory("E:/"), new UDirectory("E:").GetFullDirectory());
        }

        [Test]
        public void TestUPathEquals()
        {
            // TODO
        }

        [Test]
        public void TestUPathGetHashCode()
        {
            // TODO
        }

        [Test]
        public void TestUPathCompare()
        {
            // TODO
        }

        [Test]
        public void TestUPathToString()
        {
            // TODO
        }

        [Test]
        public void TestUPathToWindowsPath()
        {
            // TODO
        }

        [Test]
        public void TestUPathCombine()
        {
            // TODO: not enough test!
            Assert.AreEqual(new UFile("/a/b/d/e.txt"), UPath.Combine("/a/b/c", new UFile("../d/e.txt")));
            Assert.AreEqual(new UFile("/d/e.txt"), UPath.Combine("/a/b/c", new UFile("../../../d/e.txt")));
            Assert.AreEqual(new UFile("/d/e.txt"), UPath.Combine("/a/b/c", new UFile("../../../../../../d/e.txt")));
            Assert.AreEqual(new UFile("C:/a/d/e.txt"), UPath.Combine("C:/a/b/c", new UFile("../../d/e.txt")));
            Assert.AreEqual(new UFile("C:/d/e.txt"), UPath.Combine("C:/a/b/c", new UFile("../../../d/e.txt")));
            Assert.AreEqual(new UFile("C:/d/e.txt"), UPath.Combine("C:/a/b/c", new UFile("../../../../../../d/e.txt")));
        }

        [Test]
        public void TestUPathMakeRelative()
        {
            // TODO
            //Assert.AreEqual(new UDirectory("../.."), new UDirectory("C:/a").MakeRelative("/a/b/c"));
        }

        [Test]
        public void TestUPathHasDirectoryChars()
        {
            // TODO
        }

        [Test]
        public void TestUPathIsValid()
        {
            // TODO
        }

        [Test]
        public void TestUPathNormalize()
        {
            // TODO - maybe we should turn this method private? Or keep a single overload public?
        }

        [Test]
        public void TestUFileGetDirectoryAndFileName()
        {
            // TODO
        }

        [Test]
        public void TestUFileGetFileName()
        {
            // TODO
        }

        [Test]
        public void TestUFileGetFileExtension()
        {
            // TODO
        }

        [Test]
        public void TestUFileGetFileNameWithExtension()
        {
            // TODO
        }

        [Test]
        public void TestUFileGetFullPathWithoutExtension()
        {
            // TODO
        }

        [Test]
        public void TestUFileIsValid()
        {
            // TODO
        }

        [Test]
        public void TestUDirectoryContains()
        {
            // TODO
        }

        [Test]
        public void TestUDirectoryGetDirectoryName()
        {
            // TODO
        }
    }
}
