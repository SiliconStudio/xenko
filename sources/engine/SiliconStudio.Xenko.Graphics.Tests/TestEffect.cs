// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
/*
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System.IO;

using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;
using EffectCompiler = SiliconStudio.Xenko.Shaders.Compiler.EffectCompiler;

namespace SiliconStudio.Xenko.Graphics
{
    [TestFixture]
    public class TestEffect : Game
    {
        private bool isTestGlsl = false;
        private bool isTestGlslES = false;
        
        public TestEffect()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 480,
                //PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 }
                PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 }
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (isTestGlsl)
            {
                base.Update(gameTime);
                isTestGlsl = false;
                RuntimeToGlslEffect();
            }
            else if (isTestGlslES)
            {
                base.Update(gameTime);
                isTestGlslES = false;
                RuntimeToGlslESEffect();
            }
            else
            {
                Exit();
            }
        }


        [Test]
        public void TestSimpleEffect()
        {
            EffectBytecode effectBytecode;

            // Create and mount database file system
            var objDatabase = new ObjectDatabase(VirtualFileSystem.ApplicationDatabasePath);
            using (var contentIndexMap = new contentIndexMap("/assets"))
            {
                contentIndexMap.LoadNewValues();
                var database = new DatabaseFileProvider(contentIndexMap, objDatabase);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\shaders", "*.xksl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"Compiler", "*.xksl"))
                    CopyStream(database, shaderName);

                var compiler = new EffectCompiler();
                compiler.SourceDirectories.Add("assets/shaders");
                var compilerCache = new EffectCompilerCache(compiler);

                var compilerParmeters = new CompilerParameters { Platform = GraphicsPlatform.Direct3D };

                var compilerResults = compilerCache.Compile(new ShaderMixinSource("SimpleEffect"), compilerParmeters);
                Assert.That(compilerResults.HasErrors, Is.False);

                effectBytecode = compilerResults.Bytecodes[0];
            }

            var graphicsDevice = GraphicsDevice.New();

            var effect = new Effect(graphicsDevice, effectBytecode);
            effect.Apply();
        }

        [Test]
        public void TestToGlslEffect()
        {
            isTestGlsl = true;
            this.Run();
        }

        private void RuntimeToGlslEffect()
        {
            EffectBytecode effectBytecode;

            // Create and mount database file system
            var objDatabase = new ObjectDatabase(VirtualFileSystem.ApplicationDatabasePath);
            using (var contentIndexMap = new contentIndexMap("/assets"))
            {
                contentIndexMap.LoadNewValues();
                var database = new DatabaseFileProvider(contentIndexMap, objDatabase);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\shaders", "*.xksl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"Compiler", "*.xksl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\engine\SiliconStudio.Xenko.Graphics\Shaders", "*.xksl"))
                    CopyStream(database, shaderName);

                var compiler = new EffectCompiler();
                compiler.SourceDirectories.Add("assets/shaders");
                var compilerCache = new EffectCompilerCache(compiler);

                var compilerParameters = new CompilerParameters { Platform = GraphicsPlatform.OpenGLCore };

                var compilerResults = compilerCache.Compile(new ShaderMixinSource("ToGlslEffect"), compilerParameters);
                Assert.That(compilerResults.HasErrors, Is.False);

                effectBytecode = compilerResults.Bytecodes[0];
            }

            this.GraphicsDevice.Begin();

            var effect = new Effect(this.GraphicsDevice, effectBytecode);
            effect.Apply();
        }

        [Test]
        public void TestToGlslESEffect()
        {
            isTestGlslES = true;
            this.Run();
        }

        private void RuntimeToGlslESEffect()
        {
            EffectBytecode effectBytecode;

            // Create and mount database file system
            var objDatabase = new ObjectDatabase(VirtualFileSystem.ApplicationDatabasePath);
            using (var contentIndexMap = new contentIndexMap("/assets"))
            {
                contentIndexMap.LoadNewValues();
                var database = new DatabaseFileProvider(contentIndexMap, objDatabase);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\shaders", "*.xksl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"Compiler", "*.xksl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\engine\SiliconStudio.Xenko.Graphics\Shaders", "*.xksl"))
                    CopyStream(database, shaderName);

                var compiler = new EffectCompiler();
                compiler.SourceDirectories.Add("assets/shaders");
                var compilerCache = new EffectCompilerCache(compiler);

                var compilerParameters = new CompilerParameters { Platform = GraphicsPlatform.OpenGLES };

                var compilerResults = compilerCache.Compile(new ShaderMixinSource("ToGlslEffect"), compilerParameters);
                Assert.That(compilerResults.HasErrors, Is.False);

                effectBytecode = compilerResults.Bytecodes[0];
            }

            this.GraphicsDevice.Begin();

            var effect = new Effect(this.GraphicsDevice, effectBytecode);
            effect.Apply();
        }

        private void CopyStream(DatabaseFileProvider database, string fromFilePath)
        {
            var shaderFilename = string.Format("shaders/{0}", Path.GetFileName(fromFilePath));
            if (!database.FileExists(shaderFilename))
            {
                using (var outStream = database.OpenStream(shaderFilename, VirtualFileMode.Create, VirtualFileAccess.Write, VirtualFileShare.Write))
                {
                    using (var inStream = new FileStream(fromFilePath, FileMode.Open, FileAccess.Read))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }
        }
    }
}
#endif
*/
