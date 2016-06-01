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
            Assert.DoesNotThrow(() => { var s = "E:/a/b"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/d.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.DoesNotThrow(() => { var s = "E:/a/b/c/.txt"; new UFile(s); new UFile(s.Replace('/', '\\')); });
            Assert.Throws<ArgumentException>(() => new UFile("a\""));
            Assert.Throws<ArgumentException>(() => new UFile("*.txt"));
            Assert.Throws<ArgumentException>(() => new UFile("/a/"));
            Assert.Throws<ArgumentException>(() => new UFile("/"));
        }

        [Test]
        public void TestUDirectoryConstructor()
        {
            // TODO
        }

        [Test]
        public void TestUPathFullPath()
        {
            // TODO
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
            Assert.True(new UFile("E:/a/b.txt").HasDirectory);
            Assert.True(new UFile("E:/a/b/c.txt").HasDirectory);
            Assert.True(new UFile("E:/a/b/c").HasDirectory);
            Assert.True(new UDirectory("/a/b/c").HasDirectory);
            Assert.True(new UDirectory("E:/a/b/c").HasDirectory);
            Assert.True(new UDirectory("/a").HasDirectory);
            Assert.True(new UDirectory("E:/a").HasDirectory);
            Assert.True(new UDirectory("/").HasDirectory);
            // TODO: "E:/" should return true if "/" does. But shouldn't we always return true when it's an UDirectory anyway?
            //Assert.True(new UDirectory("E:/").HasDirectory);
            Assert.False(new UFile("a.txt").HasDirectory);
            Assert.False(new UDirectory("E:").HasDirectory);
            Assert.False(new UFile("E:").HasDirectory);
        }

        [Test]
        public void TestUPathIsDirectoryOnly()
        {
            // TODO
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
            // TODO
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
            // TODO
        }

        [Test]
        public void TestUPathMakeRelative()
        {
            // TODO
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
