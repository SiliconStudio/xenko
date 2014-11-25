// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.Materials.Processor.Flattener;
using SiliconStudio.Paradox.Assets.Model;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Tests
{
    class TestTreeFlattener
    {
        public static readonly ParameterKey<Vector4> DummyFloat4Key = ParameterKeys.New(Vector4.Zero, "TestTreeFlattener.DummyFloat4Key");

        [TestFixtureSetUp]
        public void Initialize()
        {
            AssetRegistry.RegisterAssembly(typeof(ModelAsset).Assembly);
            AssetRegistry.RegisterAssembly(typeof(MaterialDescription).Assembly);
            AssetRegistry.RegisterAssembly(typeof(Color4).Assembly);

            TestCommon.InitializeAssetDatabase();
        }

        [Test]
        public void RunTest()
        {
            // TODO: extend test with possible/impossible reduction cases
            var materialUrl = "materials/testEffect.pdxmat";
            var materialAsset = AssetSerializer.Load<MaterialAsset>(materialUrl);
            var material = materialAsset.Material;
            var graphicsDevice = GraphicsDevice.New(DeviceCreationFlags.None, GraphicsProfile.Level_11_0);
            var solver = new MaterialTextureLayerFlattener(material, graphicsDevice);
            solver.PrepareForFlattening();
            var compiler = new EffectCompiler();
            compiler.SourceDirectories.Add("shaders");
            solver.Run(compiler);

            // diffuse was reduced
            Assert.AreEqual("diffuse", solver.Material.ColorNodes[MaterialParameters.AlbedoDiffuse]);
            var node = solver.Material.Nodes["diffuse"];
            Assert.NotNull(node);
            var textureNode = solver.Material.Nodes["diffuse"] as MaterialTextureNode;
            Assert.NotNull(textureNode);
            Assert.IsTrue(textureNode.TextureName.StartsWith("__reduced_textures__"));

            // specular was reduced
            Assert.AreEqual("specular", solver.Material.ColorNodes[MaterialParameters.AlbedoSpecular]);
            node = solver.Material.Nodes["specular"];
            Assert.NotNull(node);
            textureNode = solver.Material.Nodes["specular"] as MaterialTextureNode;
            Assert.NotNull(textureNode);
            Assert.IsTrue(textureNode.TextureName.StartsWith("__reduced_textures__"));

            // normalMap wasn't reduced
            Assert.AreEqual("normalMap", solver.Material.ColorNodes[MaterialParameters.NormalMap]);
            node = solver.Material.Nodes["normalMap"];
            Assert.IsFalse(solver.Material.Nodes["normalMap"] is MaterialTextureNode);

            graphicsDevice.Dispose();
        }
    }
}
