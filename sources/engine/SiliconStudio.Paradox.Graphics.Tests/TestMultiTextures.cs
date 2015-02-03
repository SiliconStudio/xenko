// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
/*
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    class TestMultiTextures : TestGameBase
    {
        private Texture2D UV2Texture;
        private Effect MultiTexturesEffect;
        private GeometricPrimitive geometry;

        public TestMultiTextures()
        {
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var objDatabase = new ObjectDatabase("/data/db");
            using (var assetIndexMap = new AssetIndexMap("/assets"))
            {
                assetIndexMap.LoadNewValues();
                DatabaseFileProvider database = null;
                foreach (var provider in VirtualFileSystem.Providers)
                {
                    if (provider.RootPath == "/assets/")
                    {
                       database = provider as DatabaseFileProvider;
                    }
                }
                if (database == null)
                    database = new DatabaseFileProvider(assetIndexMap, objDatabase);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\shaders", "*.pdxsl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"Compiler", "*.pdxsl"))
                    CopyStream(database, shaderName);

                foreach (var shaderName in Directory.EnumerateFiles(@"..\..\..\..\engine\SiliconStudio.Paradox.Graphics\Shaders", "*.pdxsl"))
                    CopyStream(database, shaderName);

                var compiler = new EffectCompiler();
                compiler.SourceDirectories.Add("assets/shaders");
                var compilerCache = new EffectCompilerCache(compiler);
                var compilerParameters = new CompilerParameters {Platform = GraphicsPlatform.OpenGLCore};
                var compilerResults = compilerCache.Compile(new ShaderMixinSource("MultiTexturesSpriteEffect"), compilerParameters);
                
                Assert.That(compilerResults.HasErrors, Is.False);

                var effectBytecode = compilerResults.Bytecodes[0];
                MultiTexturesEffect = new Effect(GraphicsDevice, effectBytecode);

                using (var stream = new FileStream("uvInvert.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                    UV2Texture = Texture2D.Load(GraphicsDevice, stream);
            }

            // load geometry
            geometry = GeometricPrimitive.Plane.New(GraphicsDevice);
            
            var view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            var projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            MultiTexturesEffect.SharedParameters.Set(TransformationKeys.WorldViewProjection, Matrix.Multiply(view, projection));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.CornflowerBlue);

            base.Draw(gameTime);

            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.BackBuffer);
            MultiTexturesEffect.SharedParameters.Set(TexturingKeys.Texture0, UVTexture);
            MultiTexturesEffect.SharedParameters.Set(TexturingKeys.Texture1, UV2Texture);
            MultiTexturesEffect.Apply();
            geometry.Draw();
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

        public static void Main()
        {
            using (var game = new TestMultiTextures())
                game.Run();
        }
    }
}

#endif
*/