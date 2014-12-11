// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Tests
{
    /// <summary>
    /// Tests for the mixins code generation and runtime API.
    /// </summary>
    [TestFixture]
    public partial class TestMixinGenerator
    {
        /// <summary>
        /// Tests a simple mixin.
        /// </summary>
        [Test]
        public void TestSimple()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimple", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C");
        }

        /// <summary>
        /// Tests with a child mixin.
        /// </summary>
        [Test]
        public void TestSimpleChild()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimpleChild", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C", "C1", "C2");
        }

        /// <summary>
        /// Tests a simple composition
        /// </summary>
        [Test]
        public void TestSimpleCompose()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimpleCompose", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C");
            mixin.Mixin.CheckComposition("x", "X");
        }

        /// <summary>
        /// Tests simgple parameters usage
        /// </summary>
        [Test]
        public void TestSimpleParams()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimpleParams", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "D");
            mixin.Mixin.CheckComposition("y", "Y");
            mixin.Mixin.CheckMacro("Test", "ok");

            // Set a key to modify the mixin
            properties.Set(Test7.TestParameters.param1, true);

            mixin = GenerateMixin("DefaultSimpleParams", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C");
            mixin.Mixin.CheckComposition("x", "X");
            mixin.Mixin.CheckMacro("param2", 1);
        }

        /// <summary>
        /// Tests clone.
        /// </summary>
        [Test]
        public void TestSimpleClone()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimpleClone", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C");
            Assert.That(mixin.Children.Count, Is.EqualTo(1), "Expecting one children mixin");

            mixin.Children.Values.First().Mixin.CheckMixin("A", "B", "C", "C1", "C2");
        }


        /// <summary>
        /// Test parameters
        /// </summary>
        [Test]
        public void TestMixinAndComposeKeys()
        {
            var properties = new ShaderMixinParameters();

            var subCompute1Key = TestABC.TestParameters.UseComputeColor2.ComposeWith("SubCompute1");
            var subCompute2Key = TestABC.TestParameters.UseComputeColor2.ComposeWith("SubCompute2");
            var subComputesKey = TestABC.TestParameters.UseComputeColorRedirect.ComposeWith("SubComputes[0]");

            properties.Set(subCompute1Key, true);
            properties.Set(subComputesKey, true);
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("test_mixin_compose_keys", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A");

            Assert.AreEqual(3, mixin.Mixin.Compositions.Count);

            Assert.IsTrue(mixin.Mixin.Compositions.ContainsKey("SubCompute1"));
            Assert.IsTrue(mixin.Mixin.Compositions.ContainsKey("SubCompute2"));
            Assert.IsTrue(mixin.Mixin.Compositions.ContainsKey("SubComputes"));

            Assert.AreEqual("mixin ComputeColor2", mixin.Mixin.Compositions["SubCompute1"].ToString());
            Assert.AreEqual("mixin ComputeColor", mixin.Mixin.Compositions["SubCompute2"].ToString());
            Assert.AreEqual("[mixin ComputeColorRedirect [{ColorRedirect = mixin ComputeColor2}]]", mixin.Mixin.Compositions["SubComputes"].ToString());

            Assert.IsTrue(mixin.UsedParameters.ContainsKey(subCompute1Key));
            Assert.IsTrue(mixin.UsedParameters.ContainsKey(subCompute2Key));
            Assert.IsTrue(mixin.UsedParameters.ContainsKey(subComputesKey));

            Assert.IsTrue(mixin.UsedParameters.Get(subCompute1Key));
            Assert.IsFalse(mixin.UsedParameters.Get(subCompute2Key));
            Assert.IsTrue(mixin.UsedParameters.Get(subComputesKey));
        }

        /// <summary>
        /// Test parameters
        /// </summary>
        [Test]
        public void TestSimpleChildParams()
        {
            var properties = new ShaderMixinParameters();
            properties.Set(Test4.TestParameters.TestCount, 0);
            ShaderMixinParameters usedProperties;

            var mixin = GenerateMixin("DefaultSimpleChildParams", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C");
            Assert.That(mixin.Children.Count, Is.EqualTo(1), "Expecting one children mixin");
            var childMixin = mixin.Children.Values.First();
            childMixin.Mixin.CheckMixin("A", "B", "C1");
            Assert.IsTrue(childMixin.UsedParameters.ContainsKey(Test4.TestParameters.TestCount));
            Assert.AreEqual(0, childMixin.UsedParameters.Get(Test4.TestParameters.TestCount));
        }

        /// <summary>
        /// Tests the complex parameters (array and nested using)
        /// </summary>
        [Test]
        public void TestComplexParams()
        {
            var properties = new ShaderMixinParameters();
            ShaderMixinParameters usedProperties;

            // Populate the the properties used by the mixin
            var subParam1 = new Test1.SubParameters();
            var subParameters = new Test1.SubParameters[4];
            for (int i = 0; i < subParameters.Length; i++)
            {
                subParameters[i] = new Test1.SubParameters();
            }

            properties.Set(Test1.TestParameters.subParam1, subParam1);
            properties.Set(Test1.TestParameters.subParameters, subParameters);

            // Generate the mixin with default properties
            var mixin = GenerateMixin("DefaultComplexParams", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C", "D");

            // Modify properties in order to modify mixin
            for (int i = 0; i < subParameters.Length; i++)
            {
                subParameters[i].Set(Test1.SubParameters.param1, (i & 1) == 0);
            }
            subParam1.Set(Test1.SubParameters.param2, 2);

            mixin = GenerateMixin("DefaultComplexParams", properties, out usedProperties);
            mixin.Mixin.CheckMixin("A", "B", "C", "C1", "C3");
        }

        public static ParameterKey<int> PropertyInt = ParameterKeys.New<int>();
        public static ParameterKey<ShaderMixinParameters> PropertySub = ParameterKeys.New<ShaderMixinParameters>();
        public static ParameterKey<ShaderMixinParameters[]> PropertySubs = ParameterKeys.New<ShaderMixinParameters[]>();

        [Test]
        public void TestShaderParametersSerialization()
        {
            // Test serialization
            var shaderParameters = new ShaderMixinParameters("Test");
            shaderParameters.Set(PropertyInt, 5);
            var subShaderParameters = new ShaderMixinParameters("Sub");
            subShaderParameters.Set(PropertyInt, 6);
            shaderParameters.Set(PropertySub, subShaderParameters);

            var subShaderParametersArray = new ShaderMixinParameters[1];
            var subShaderParametersArray1 = new ShaderMixinParameters("InArray1");
            subShaderParametersArray[0] = subShaderParametersArray1;
            subShaderParametersArray1.Set(PropertyInt, 7);
            shaderParameters.Set(PropertySubs, subShaderParametersArray);

            var memoryStream = new MemoryStream();
            var writer = new BinarySerializationWriter(memoryStream);
            writer.Write(shaderParameters);
            writer.Flush();
            memoryStream.Position = 0;

            var reader = new BinarySerializationReader(memoryStream);
            var shaderParametersReloaded = reader.Read<ShaderMixinParameters>();

            // They should be strictly equal
            Assert.That(shaderParametersReloaded.IsSubsetOf(shaderParameters), Is.True);
            Assert.That(shaderParameters.IsSubsetOf(shaderParametersReloaded), Is.True);

            // Test subset
            // Check that by removing one key from the original parameters, the reloaded version is
            // no longer a subset
            subShaderParametersArray1.Remove(PropertyInt);
            Assert.That(shaderParametersReloaded.IsSubsetOf(shaderParameters), Is.False);
        }

    }
}