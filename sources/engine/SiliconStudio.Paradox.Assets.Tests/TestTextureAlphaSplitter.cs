// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using NUnit.Framework;

using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.SpriteFont;
using SiliconStudio.Paradox.Effects;
using System.Linq;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestTextureAlphaSplitter
    {
        public static void LoadParadoxAssemblies()
        {
            RuntimeHelpers.RunModuleConstructor(typeof(Asset).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SpriteFontAsset).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(MaterialKeys).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(Effects.Model).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(MaterialTextureNode).Module.ModuleHandle);
        }

        [TestFixtureSetUp]
        public void InitializeTest()
        {
            LoadParadoxAssemblies();
            TestCommon.InitializeAssetDatabase();
        }

        [Test]
        public void TestSplitTexture()
        {
            var sessionResult = PackageSession.Load("../../sources/engine/SiliconStudio.Paradox.Assets.Tests/SiliconStudio.Paradox.Assets.Tests.pdxpkg");
            var session = sessionResult.Session;

            var materialItem = session.FindAsset("Cube/TestMaterial");
            var material = (MaterialAsset)materialItem.Asset;
            var solver = new TextureAlphaComponentSplitter(session);

            var modifiedMaterial = solver.Run(material.Material, materialItem.Location.GetDirectory());
                
            Assert.AreEqual(3, modifiedMaterial.Nodes.Count);
            Assert.AreEqual(1, modifiedMaterial.ColorNodes.Count);

            // test that the original structure of the material hasn't changed

            var originalRootNode = modifiedMaterial.Nodes[modifiedMaterial.ColorNodes.First().Value];
            Assert.IsTrue(originalRootNode is MaterialBinaryNode);

            var originalBinaryRootNode = (MaterialBinaryNode)originalRootNode;
            Assert.AreEqual((int)MaterialBinaryOperand.Overlay, (int)originalBinaryRootNode.Operand);
            Assert.IsTrue(originalBinaryRootNode.LeftChild is MaterialBinaryNode);
            Assert.IsTrue(originalBinaryRootNode.RightChild is MaterialBinaryNode);

            var originalRootLeftChildNode = (MaterialBinaryNode)originalBinaryRootNode.LeftChild;
            var originalRootRightChildNode = (MaterialBinaryNode)originalBinaryRootNode.RightChild;

            Assert.AreEqual((int)MaterialBinaryOperand.Screen, (int)originalRootLeftChildNode.Operand);
            Assert.AreEqual((int)MaterialBinaryOperand.Saturate, (int)originalRootRightChildNode.Operand);
            Assert.IsTrue(originalRootLeftChildNode.LeftChild is MaterialReferenceNode);
            Assert.IsTrue(originalRootLeftChildNode.RightChild is MaterialReferenceNode);
            Assert.IsTrue(originalRootRightChildNode.RightChild is MaterialFloat4Node);

            var originalRootLeftLeftChildNode = (MaterialReferenceNode)originalRootLeftChildNode.LeftChild;
            var originalRootLeftRightChildNode = (MaterialReferenceNode)originalRootLeftChildNode.RightChild;
            var originalRootRightLeftChildNode = originalRootRightChildNode.LeftChild;
            var originalRootRightRightChildNode = (MaterialFloat4Node)originalRootRightChildNode.RightChild;

            Assert.AreEqual("diffuseFactor", originalRootLeftLeftChildNode.Name);
            Assert.AreEqual("diffuseTexture", originalRootLeftRightChildNode.Name);
                
            var rawUnreferencedRootLeftLeftChildNode = modifiedMaterial.Nodes["diffuseFactor"];
            var rawUnreferencedRootLeftRightChildNode = modifiedMaterial.Nodes["diffuseTexture"];
                
            Assert.IsTrue(rawUnreferencedRootLeftLeftChildNode is MaterialFloat4Node);

            var unreferencedRootLeftLeftChildNode = (MaterialFloat4Node)rawUnreferencedRootLeftLeftChildNode;
            var unreferencedRootLeftRightChildNode = rawUnreferencedRootLeftRightChildNode;

            Assert.AreEqual(new Vector4(0.1f, 0.2f, 0.3f, 0.4f), unreferencedRootLeftLeftChildNode.Value);
            Assert.AreEqual(new Vector4(1f, 2f, 3f, 4f), originalRootRightRightChildNode.Value);

            var originalTextureNodes = new List<MaterialTextureNode>
                {
                    (MaterialTextureNode)((MaterialBinaryNode)((MaterialBinaryNode)material.Material.Nodes["diffuse"]).RightChild).LeftChild, 
                    (MaterialTextureNode)material.Material.Nodes["diffuseTexture"]
                };
            var modifiedTextureNodes = new List<IMaterialNode> { originalRootRightLeftChildNode, unreferencedRootLeftRightChildNode };

            // test that the old MaterialTextureReferences has been substituted

            for (int i = 0; i < originalTextureNodes.Count; i++)
            {
                var originalTextureNode = originalTextureNodes[i];
                var modifiedTextureNode = modifiedTextureNodes[i];

                Assert.IsTrue(modifiedTextureNode is MaterialShaderClassNode);

                var newShaderNode = (MaterialShaderClassNode)modifiedTextureNode;

                Assert.AreEqual("ComputeColorSubstituteAlphaWithColor", Path.GetFileNameWithoutExtension(newShaderNode.MixinReference.Location));
                Assert.IsTrue(newShaderNode.CompositionNodes.ContainsKey("color1"));
                Assert.IsTrue(newShaderNode.CompositionNodes.ContainsKey("color2"));
                Assert.IsTrue(newShaderNode.CompositionNodes["color1"] is MaterialTextureNode);
                Assert.IsTrue(newShaderNode.CompositionNodes["color2"] is MaterialTextureNode);

                var leftNode = (MaterialTextureNode)newShaderNode.CompositionNodes["color1"];
                var rightNode = (MaterialTextureNode)newShaderNode.CompositionNodes["color2"];

                var textureNodes = new List<MaterialTextureNode> { leftNode, rightNode };
                foreach (var textureNode in textureNodes)
                {
                    Assert.AreEqual(originalTextureNode.Sampler.AddressModeU, textureNode.Sampler.AddressModeU);
                    Assert.AreEqual(originalTextureNode.Sampler.AddressModeV, textureNode.Sampler.AddressModeV);
                    Assert.AreEqual(originalTextureNode.Sampler.Filtering, textureNode.Sampler.Filtering);
                    Assert.AreEqual(originalTextureNode.Offset, textureNode.Offset);
                    Assert.AreEqual(originalTextureNode.Sampler.SamplerParameterKey, textureNode.Sampler.SamplerParameterKey);
                    Assert.AreEqual(originalTextureNode.Scale, textureNode.Scale);
                    Assert.AreEqual(originalTextureNode.TexcoordIndex, textureNode.TexcoordIndex);
                }

                Assert.AreEqual(originalTextureNode.Key, leftNode.Key);
                Assert.AreEqual(null, rightNode.Key);

                const string textureName = "cube_Untitled";
                const string leftNodeSupposedLocation = "Cube/" + TextureAlphaComponentSplitter.SplittedTextureNamePrefix + textureName + TextureAlphaComponentSplitter.SplittedColorTextureNameSuffix;
                const string rightNodeSupposedLocation = "Cube/" + TextureAlphaComponentSplitter.SplittedTextureNamePrefix + textureName + TextureAlphaComponentSplitter.SplittedAlphaTextureNameSuffix;
                Assert.AreEqual(leftNodeSupposedLocation, leftNode.TextureName);
                Assert.AreEqual(rightNodeSupposedLocation, rightNode.TextureName);
                Assert.IsTrue(AssetManager.FileProvider.FileExists(leftNodeSupposedLocation));
                Assert.IsTrue(AssetManager.FileProvider.FileExists(rightNodeSupposedLocation));
            }
        }

        public static void Main()
        {
            var test = new TestTextureAlphaSplitter();
            test.InitializeTest();
            test.TestSplitTexture();
        }
    }
}