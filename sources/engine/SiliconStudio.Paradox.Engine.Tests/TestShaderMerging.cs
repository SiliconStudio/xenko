// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;

using SiliconStudio.Paradox.Shaders.Parser;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;

using NUnit.Framework;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class TestShaderMerging
    {
        [Test]
        public void TestSimpleMerge()
        {
/*            var shader1 = ParadoxShaderParser.Parse("..\\..\\..\\..\\..\\..\\sources\\shaders\\testmix2.hotei");

            var defaultShader = new Shader();
            defaultShader.Mix(shader1);

            var shaderMixer = new ShaderMixer();
            var shader2 = shaderMixer.Generate(defaultShader);

            var shaderCleaner = new ParadoxShaderCleaner();
            shaderCleaner.Run(shader2);

            var writer = new Framework.Shaders.Writer.Hlsl.HlslWriter();
            writer.Visit(shader2);
            var source = writer.Text;
            File.WriteAllText("..\\..\\..\\..\\..\\..\\sources\\shaders\\testmixresult.hotei", source);
 */
        }

        [Test, Ignore]
        public void TestComposition()
        {
            var writer = new SiliconStudio.Shaders.Writer.Hlsl.HlslWriter();

            //ShaderMixer.DefaultSourcePath = "..\\..\\..\\..\\..\\..\\sources\\shaders\\";

            var shader = new Shader();

            var mainShader = shader.GetMainShaderClass();

            var mix1 = ShaderExtensions.StartMix();
            mix1.Mix(new TypeName("ComputeColorStream"));

            var mix2 = ShaderExtensions.StartMix();
            mix2.Mix(new TypeName("ComputeColor"));
            mainShader.Compose("colors", mix1, mix2);

            throw new NotImplementedException();

            //var shaderCompose = ShaderMixer.LoadShaderClass("testmix");
            //shader.Mix(shaderCompose);

            //var shaderMixer = new ShaderMixer();
            //var finalShader = shaderMixer.Generate(shader);

            //var shaderCleaner = new ParadoxShaderCleaner();
            //shaderCleaner.Run(finalShader);

            //writer = new Framework.Shaders.Writer.Hlsl.HlslWriter();
            //writer.Visit(finalShader);
            //Console.WriteLine(writer.Text);
        }

        [Test, Ignore]
        public void TestCompositionSub()
        {
            var writer = new SiliconStudio.Shaders.Writer.Hlsl.HlslWriter();

            //ShaderMixer.DefaultSourcePath = "..\\..\\..\\..\\..\\..\\sources\\shaders\\";

            var shader = new Shader();

            var mainShader = shader.GetMainShaderClass();

            var mix111 = ShaderExtensions.StartMix()
                .Mix(new TypeName("ComputeColorMultiply"))
                .Compose("color1", ShaderExtensions.StartMix()
                    .Mix(new TypeName(new IdentifierGeneric("ComputeColorTexture", "Texture0.DiffuseTexture", "Texture0.TexCoord"))))
                .Compose("color2", ShaderExtensions.StartMix()
                    .Mix(new TypeName(new IdentifierGeneric("ComputeColorTextureRepeat", "Texture0.DiffuseTexture2", "Texture0.TexCoord", "10"))));

            var mix11 = ShaderExtensions.StartMix()
                .Mix(new TypeName("ComputeColorLerpAlpha"))
                .Compose("color1", mix111)
                .Compose("color2", ShaderExtensions.StartMix()
                    .Mix(new TypeName(new IdentifierGeneric("ComputeColorTextureRepeat", "Texture0.DiffuseTexture3", "Texture0.TexCoord", "10"))));

            var mix12 = ShaderExtensions.StartMix()
                .Mix(new TypeName(new IdentifierGeneric("ComputeColorTexture", "Texture0.DiffuseTexture3", "Texture0.TexCoord2")));

            var mix1 = ShaderExtensions.StartMix()
                .Mix(new TypeName("ComputeColorMultiply"))
                .Compose("color1", mix11)
                .Compose("color2", mix12);

            throw new NotImplementedException();

            //mainShader
            //    .Mix(new TypeName("ShaderBase"))
            //    .Mix(new TypeName("TransformationWVP"))
            //    .Mix(new TypeName("AlbedoSpecularBase"))
            //    .Mix(new TypeName("AlbedoDiffuseBase"))
            //    .Mix(new TypeName("NormalVSGBuffer"))
            //    .Mix(new TypeName("SpecularPowerGBuffer"))
            //    .Mix(new TypeName("PositionVSGBuffer"))
            //    .Mix(new TypeName("BRDFDiffuseLambert"))
            //    .Mix(new TypeName("BRDFSpecularBlinnPhong"))
            //    .Mix(new TypeName("AlbedoFlatShading"))
            //    .Compose("albedoDiffuse", mix1)
            //    .Compose("albedoSpecular", ShaderMixer.LoadShaderClass("ComputeColorStream"));

            //var shaderMixer = new ShaderMixer();
            //var finalShader = shaderMixer.Generate(shader);

            //var shaderCleaner = new ParadoxShaderCleaner();
            //shaderCleaner.Run(finalShader);

            //writer = new Framework.Shaders.Writer.Hlsl.HlslWriter();
            //writer.Visit(finalShader);
            ////Console.WriteLine(writer.Text);
            //File.WriteAllText(ShaderMixer.DefaultSourcePath + "testsubmix.hotei", writer.Text);
        }
    }
}
#endif