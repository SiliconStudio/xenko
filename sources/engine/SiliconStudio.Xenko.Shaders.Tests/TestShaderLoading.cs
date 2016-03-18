// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;

using NUnit.Framework;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Shaders.Parser.Mixins;

using LoggerResult = SiliconStudio.Shaders.Utility.LoggerResult;

namespace SiliconStudio.Xenko.Shaders.Tests
{
    [TestFixture]
    public class TestShaderLoading
    {
        private ShaderSourceManager sourceManager;
        private ShaderLoader shaderLoader;

        [SetUp]
        public void Init()
        {
            // Create and mount database file system
            var objDatabase = ObjectDatabase.CreateDefaultDatabase();
            var databaseFileProvider = new DatabaseFileProvider(objDatabase);
            ContentManager.GetFileProvider = () => databaseFileProvider;

            sourceManager = new ShaderSourceManager();
            sourceManager.LookupDirectoryList.Add(@"shaders");
            shaderLoader = new ShaderLoader(sourceManager);
        }

        [Test]
        public void TestSimple()
        {
            var simple = sourceManager.LoadShaderSource("Simple");

            // Make sure that SourceManager will fail if type is not found
            Assert.Catch<FileNotFoundException>(() => sourceManager.LoadShaderSource("BiduleNotFound"));

            // Reload it and check that it is not loaded twice
            var simple2 = sourceManager.LoadShaderSource("Simple");

            //TODO: cannot compare structure references
            //Assert.That(ReferenceEquals(simple, simple2), Is.True);
            Assert.AreEqual(simple, simple2);
        }

        [Test]
        public void TestLoadAst()
        {
            var log = new LoggerResult();

            var simple = shaderLoader.LoadClassSource(new ShaderClassSource("Simple"), new SiliconStudio.Shaders.Parser.ShaderMacro[0], log, false);

            Assert.That(simple.Members.Count, Is.EqualTo(1));

            var simple2 = shaderLoader.LoadClassSource(new ShaderClassSource("Simple"), new SiliconStudio.Shaders.Parser.ShaderMacro[0], log, false);

            // Make sure that a class is not duplicated in memory
            Assert.That(ReferenceEquals(simple, simple2), Is.True);
        }
    }
}