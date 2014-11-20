// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Assets.Model;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestMaterialTree
    {
        // ParameterKeys for test purpose
        private static readonly ParameterKey<Vector4> DummyVector4Key = ParameterKeys.New(Vector4.Zero, "TestMaterialTree.DummyVector4Key");
        private static readonly ParameterKey<float> DummyFloatKey = ParameterKeys.New(0.0f, "TestMaterialTree.DummyFloatKey");


        [TestFixtureSetUp]
        public void Initialize()
        {
            AssetRegistry.RegisterAssembly(typeof(ModelAsset).Assembly);
            AssetRegistry.RegisterAssembly(typeof(MaterialDescription).Assembly);
            AssetRegistry.RegisterAssembly(typeof(Color4).Assembly);
            AssetRegistry.RegisterAssembly(typeof(MaterialParameters).Assembly);
        }

        [Test]
        public void TestLoad()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");
            var material = materialAsset.Material;

            Assert.AreEqual(MaterialShadingModel.Phong, material.GetParameter(MaterialParameters.ShadingModel));
            Assert.AreEqual(MaterialDiffuseModel.Lambert, material.GetParameter(MaterialParameters.DiffuseModel));
            Assert.AreEqual(MaterialSpecularModel.BlinnPhong, material.GetParameter(MaterialParameters.SpecularModel));

            Assert.AreEqual(new Color4(1, 1, 0.5f, 0.5f), material.GetParameter(MaterialKeys.DiffuseColorValue));
            Assert.AreEqual(new Color4(0.4f, 0.1f, 1, 1), material.GetParameter(MaterialKeys.SpecularColorValue));
            Assert.AreEqual(0, (float)material.GetParameter(MaterialKeys.SpecularIntensity));
            Assert.AreEqual(0, (float)material.GetParameter(MaterialKeys.SpecularPower));
        }

        [Test]
        public void TestMakeShader()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");

            var materialShaderCreator = new MaterialTreeShaderCreator(materialAsset.Material);
            var allShaders = materialShaderCreator.GenerateModelShaders();

            Assert.AreEqual(6, materialShaderCreator.ModelShaderSources.Count);
            Assert.AreEqual(4, allShaders.Count);

            // TODO: more tests
        }

        [Test]
        public void TestReduction()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testReduction.pdxmat");

            var materialReducer = new MaterialTreeReducer(materialAsset.Material);
            materialReducer.ReduceTrees();

            var reducedTrees = materialReducer.ReducedTrees;

            Assert.IsTrue(reducedTrees["diffuse"] is MaterialFloat4Node);
        }

        [Test]
        public void TestReplacement()
        {
            //throw new NotImplementedException();
            /*
            var parameters = new AssetStoreParameters();
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat", parameters);

            var tree = materialAsset.Material.GetMaterialNode("Default", "displacement");
            
            tree.BuildSlotList();
            Assert.AreEqual(4, tree.MaterialSlots.Count);

            var nodeToReplace = ((tree.RootNode as MaterialBinaryNode).RightChild as MaterialUnaryNode).Node;

            (nodeToReplace as MaterialFloatNode).Value = 0.666f;
            var rightNode = new MaterialFloatNode(5.0f);
            var newNode = new MaterialBinaryNode(nodeToReplace, rightNode, MaterialBinaryOperand.Add);

            var newNode2 = new MaterialFloat4Node(new Vector4(0.1f, 0.2f, 0.3f, 0.4f));

            tree.ReplaceNode(nodeToReplace, newNode2);

            tree.BuildSlotList();
            Assert.AreEqual(4, tree.MaterialSlots.Count);*/
        }

        [Test]
        public void TestCopy()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");
            var material = materialAsset.Material;
            var matCopy = materialAsset.Material.Clone();

            Assert.AreEqual(material.Nodes.Count, matCopy.Nodes.Count);
            foreach (var matRef in material.Nodes)
            {
                Assert.True(matCopy.Nodes.ContainsKey(matRef.Key));
                //Assert.True(matRef.Value.Equals(matCopy.Nodes[matRef.Key]));
            }
        }

        [Test]
        public void TestSave()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");

            var materialReducer = new MaterialTreeReducer(materialAsset.Material);
            materialReducer.ReduceTrees();

            var reducedTrees = materialReducer.ReducedTrees;
            foreach (var reducedTree in reducedTrees)
            {
                materialAsset.Material.AddNode(reducedTree.Key + "_reduced", reducedTree.Value);
            }

            var model = new MaterialDescription();
            model.SetParameter(MaterialParameters.UseTransparent, true);
            model.SetParameter(MaterialParameters.ShadingModel, MaterialShadingModel.Phong);
            model.SetParameter(MaterialParameters.DiffuseModel, MaterialDiffuseModel.Lambert);
            model.SetParameter(MaterialParameters.SpecularModel, MaterialSpecularModel.BlinnPhong);
            model.SetParameter(MaterialKeys.DiffuseColorValue, new Color4(1.0f, 1.0f, 0.5f, 0.5f));
            model.SetParameter(MaterialKeys.SpecularColorValue, new Color4(0.4f, 0.1f, 1.0f, 1.0f));
            model.SetParameter(MaterialKeys.SpecularIntensity, 1.1f);
            model.SetParameter(MaterialKeys.SpecularPower, 10.1f);

            var defaultModel = materialAsset.Material;
            foreach (var treeName in defaultModel.ColorNodes)
            {
                model.ColorNodes.Add(treeName.Key, treeName.Value + "_reduced");
            }

            var matAsset2 = new MaterialAsset { Material = model };

            AssetSerializer.Save("testMaterial2.pdxmat", matAsset2);

            var savedAsset = AssetSerializer.Load<MaterialAsset>("testMaterial2.pdxmat");
            var loadedMaterial = savedAsset.Material;

            Assert.AreEqual(true, loadedMaterial.GetParameter(MaterialParameters.UseTransparent));
            Assert.AreEqual(MaterialShadingModel.Phong, loadedMaterial.GetParameter(MaterialParameters.ShadingModel));
            Assert.AreEqual(MaterialDiffuseModel.Lambert, loadedMaterial.GetParameter(MaterialParameters.DiffuseModel));
            Assert.AreEqual(MaterialSpecularModel.BlinnPhong, loadedMaterial.GetParameter(MaterialParameters.SpecularModel));
            Assert.AreEqual(new Color4(1.0f, 1.0f, 0.5f, 0.5f), loadedMaterial.GetParameter(MaterialKeys.DiffuseColorValue));
            Assert.AreEqual(new Color4(0.4f, 0.1f, 1.0f, 1.0f), loadedMaterial.GetParameter(MaterialKeys.SpecularColorValue));
            Assert.AreEqual(1.1f, loadedMaterial.GetParameter(MaterialKeys.SpecularIntensity));
            Assert.AreEqual(10.1f, loadedMaterial.GetParameter(MaterialKeys.SpecularPower));
        }

        [Test]
        public void TestShaderLoad()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");

            Assert.AreEqual(7, materialAsset.Material.Nodes.Count);
        }

        [Test]
        public void TestParameters()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testMaterial.pdxmat");
            var material = materialAsset.Material;

            var parameterCreator = new MaterialParametersCreator(material, "testMaterial.pdxmat");
            parameterCreator.CreateParameterCollectionData();
            var parameters = parameterCreator.Parameters;

            Assert.IsTrue(parameters.ContainsKey((MaterialParameters.AlbedoDiffuse)));
            Assert.IsTrue(parameters.ContainsKey((MaterialParameters.AlbedoSpecular)));
            Assert.IsTrue(parameters.ContainsKey((MaterialParameters.NormalMap)));
            Assert.IsTrue(parameters.ContainsKey((MaterialParameters.DisplacementMap)));
            Assert.AreEqual(MaterialDiffuseModel.Lambert, parameters[MaterialParameters.DiffuseModel]);
            Assert.AreEqual(MaterialSpecularModel.BlinnPhong, parameters[MaterialParameters.SpecularModel]);
            Assert.AreEqual(MaterialShadingModel.Phong, parameters[MaterialParameters.ShadingModel]);
            Assert.AreEqual(new Color4(0.4f, 0.1f, 1, 1), parameters[MaterialKeys.SpecularColorValue]);
            Assert.AreEqual(new Color4(1, 1, 0.5f, 0.5f), parameters[MaterialKeys.DiffuseColorValue]);
            Assert.AreEqual(0, parameters[MaterialKeys.SpecularIntensity]);
            Assert.AreEqual(0, parameters[MaterialKeys.SpecularPower]);
            Assert.IsTrue(parameters.ContainsKey((TexturingKeys.Sampler0)));
            Assert.IsTrue(parameters.ContainsKey((TexturingKeys.Texture0)));
            Assert.IsTrue(parameters.ContainsKey((TexturingKeys.Texture1)));
            Assert.IsTrue(parameters.ContainsKey((TexturingKeys.Texture2)));
            Assert.IsTrue(parameters.ContainsKey((TexturingKeys.Texture3)));
            Assert.AreEqual(16, parameters.Count);
        }
        
        [Test]
        public void TestConstantKeyCreation()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testConstantValueKey.pdxmat");

            var materialShaderCreator = new MaterialTreeShaderCreator(materialAsset.Material);
            var allParameters = materialShaderCreator.GenerateModelShaders();
            Assert.IsTrue(allParameters.Keys.Any(x => x == DummyFloatKey));
            Assert.IsTrue(allParameters.Keys.Any(x => x == DummyVector4Key));
        }

        /*[Test]
        public void TestTextureGathering()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testTextureReference.pdxmat");

            var textureVisitor = new MaterialTextureVisitor(materialAsset.Material);
            var modelTextures = textureVisitor.GetAllModelTextureValues();

            Assert.AreEqual(2, modelTextures.Count);

            var allTextures = textureVisitor.GetAllTextureValues();
            Assert.AreEqual(3, allTextures.Count);
        }*/

        [Test]
        public void TestTextureGeneric()
        {
            var materialAsset = AssetSerializer.Load<MaterialAsset>("materials/testTextureGeneric.pdxmat");

            var parameterCreator = new MaterialParametersCreator(materialAsset.Material, "testTextureGeneric.pdxmat");
            parameterCreator.CreateParameterCollectionData();
            var allParameters = parameterCreator.Parameters;

            // TODO: remove Sampler0 (extra sampler)
            Assert.IsTrue(allParameters.ContainsKey(TexturingKeys.Sampler0));
            Assert.IsTrue(allParameters.ContainsKey(TexturingKeys.Sampler1));
            Assert.IsTrue(allParameters.ContainsKey(TexturingKeys.Texture0));
            Assert.IsFalse(allParameters.ContainsKey(TexturingKeys.Texture1));

            var sampler = allParameters[TexturingKeys.Sampler1] as ContentReference<SamplerState>;
            Assert.NotNull(sampler);
            Assert.AreEqual(TextureAddressMode.Mirror, sampler.Value.Description.AddressU);
            Assert.AreEqual(TextureAddressMode.Clamp, sampler.Value.Description.AddressV);
            Assert.AreEqual(TextureFilter.Anisotropic, sampler.Value.Description.Filter);
        }

        [Test]
        public void TestGenericDictionary()
        {
            var orderedNames = new List<string>();
            orderedNames.Add("firstValue");
            orderedNames.Add("secondValue");
            orderedNames.Add("thirdValue");
            orderedNames.Add("fourthValue");

            var orderedGen = new List<INodeParameter>();
            orderedGen.Add(new NodeParameter());
            orderedGen.Add(new NodeParameterFloat());
            orderedGen.Add(new NodeParameterSampler());
            orderedGen.Add(new NodeParameterFloat4());

            var testDict = new GenericDictionary();
            testDict.Add(orderedNames[0], orderedGen[0]);
            testDict.Add(orderedNames[1], orderedGen[1]);
            testDict.Add(orderedNames[2], orderedGen[2]);
            testDict.Add(orderedNames[3], orderedGen[3]);

            //test enumeration order
            var counter = 0;
            foreach (var dictElem in testDict)
            {
                Assert.AreEqual(dictElem.Key, orderedNames[counter]);
                Assert.AreEqual(dictElem.Value, orderedGen[counter]);
                counter++;
            }

            //test serialization
            var clonedObject = AssetCloner.Clone(testDict);
            Assert.IsTrue(clonedObject is GenericDictionary);
            counter = 0;
            foreach (var dictElem in (GenericDictionary)clonedObject)
            {
                Assert.AreEqual(dictElem.Key, orderedNames[counter]);
                counter++;
            }
        }

        public static void Main(string[] args)
        {
        }
    }
}
