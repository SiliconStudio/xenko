// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;
using System.Linq;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders.Compiler;
using SiliconStudio.Xenko.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Xenko.Shaders.Tests
{
    [TestFixture]
    public class TestGenericClass
    {
        private ShaderSourceManager manager;
        private SiliconStudio.Shaders.Utility.LoggerResult logger;
        private ShaderLoader loader;

        [SetUp]
        public void Init()
        {
            // Create and mount database file system
            var objDatabase = ObjectDatabase.CreateDefaultDatabase();
            var databaseFileProvider = new DatabaseFileProvider(objDatabase);
            ContentManager.GetFileProvider = () => databaseFileProvider;

            manager = new ShaderSourceManager();
            manager.LookupDirectoryList.Add("shaders");
            logger = new SiliconStudio.Shaders.Utility.LoggerResult();
            loader = new ShaderLoader(manager);
        }

        [Test]
        public void TestParsing()
        {
            var generics = new object[9];
            generics[0] = "Texturing.Texture1";
            generics[1] = "Texturing.Sampler1";
            generics[2] = "TEXCOORD0";
            generics[3] = "CustomLink";
            generics[4] = "1.2f";
            generics[5] = "int2(1,2)";
            generics[6] = "uint3(0,1,2)";
            generics[7] = "float4(5,4,3,2)";
            generics[8] = "StaticClass.staticFloat";
            var shaderClass = loader.LoadClassSource(new ShaderClassSource("GenericClass", generics), null, logger, false);

            Assert.IsNotNull(shaderClass);

            Assert.AreEqual(10, shaderClass.Members.Count);
            Assert.AreEqual(4, shaderClass.Members.OfType<Variable>().Count(x => x.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Static)));
            Assert.AreEqual(0, shaderClass.ShaderGenerics.Count);
            Assert.AreEqual(0, shaderClass.GenericArguments.Count);
            Assert.AreEqual(0, shaderClass.GenericParameters.Count);
            Assert.AreEqual(1, shaderClass.BaseClasses.Count);

            var linkVar = shaderClass.Members[0] as Variable;
            Assert.IsNotNull(linkVar);
            var linkName = linkVar.Attributes.OfType<AttributeDeclaration>().Where(x => x.Name.Text == "Link").Select(x => x.Parameters[0].Text).FirstOrDefault();
            Assert.AreEqual("GenericLink.CustomLink", linkName);

            var baseClass = shaderClass.BaseClasses[0].Name as IdentifierGeneric;
            Assert.IsNotNull(baseClass);
            Assert.AreEqual(3, baseClass.Identifiers.Count);
            Assert.AreEqual("TEXCOORD0", baseClass.Identifiers[0].Text);
            Assert.AreEqual("CustomLink", baseClass.Identifiers[1].Text);
            Assert.AreEqual("float4(5,4,3,2)", baseClass.Identifiers[2].Text);
        }

        [Test]
        public void TestShaderCompilation()
        {
            var generics = new string[3];
            generics[0] = "Texturing.Texture1";
            generics[1] = "TEXCOORD0";
            generics[2] = "float4(2.0,1,1,1)";

            var compilerParameters = new CompilerParameters();
            compilerParameters.Set(EffectSourceCodeKeys.Enable, true);
            compilerParameters.EffectParameters.Profile = GraphicsProfile.Level_11_0;

            var mixinSource = new ShaderMixinSource { Name = "TestShaderCompilationGenericClass" };
            mixinSource.Mixins.Add(new ShaderClassSource("GenericClass2", generics));

            var log = new CompilerResults();

            var compiler = new EffectCompiler();
            compiler.SourceDirectories.Add("shaders");

            var effectByteCode = compiler.Compile(mixinSource, compilerParameters.EffectParameters, compilerParameters);
        }


        public void Run()
        {
            Init();
            TestParsing();
            //TestShaderCompilation();
        }

        public static void Main5()
        {
            // Create and mount database file system
            var objDatabase = ObjectDatabase.CreateDefaultDatabase();
            var assetIndexMap = AssetIndexMap.Load(VirtualFileSystem.ApplicationDatabaseIndexPath);
            var databaseFileProvider = new DatabaseFileProvider(assetIndexMap, objDatabase);
            ContentManager.GetFileProvider = () => databaseFileProvider;

            var test = new TestGenericClass();
            test.Run();
        }
    }
}
