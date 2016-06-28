// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.TypeConverters;

namespace SiliconStudio.Core.Design.Tests
{
    /// <summary>
    /// Tests for the <see cref="SiliconStudio.Core.TypeConverters"/> classes.
    /// </summary>
    [TestFixture]
    public class TestTypeConverter
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
            RuntimeHelpers.RunModuleConstructor(typeof(BaseConverter).Assembly.ManifestModule.ModuleHandle);
        }
        
        [Test]
        public void TestColor()
        {
            TestConversionMultipleCultures(new Color(10, 20, 30, 40));
        }

        [Test]
        public void TestColor4()
        {
            TestConversionMultipleCultures(new Color4(0.25f, 50.0f, -4.9f, 1));
        }

        [Test]
        [Ignore("fix Half.ToString() and update converter accordingly (to match other converters)")]
        public void TestHalf()
        {
            TestConversionMultipleCultures(new Half(5.6f));
        }

        [Test]
        [Ignore("fix Half2.ToString() and update converter accordingly (to match other converters)")]
        public void TestHalf2()
        {
            TestConversionMultipleCultures(new Half2(new Half(5.12f), new Half(2)));
        }

        [Test]
        [Ignore("fix Half3.ToString() and update converter accordingly (to match other converters)")]
        public void TestHalf3()
        {
            TestConversionMultipleCultures(new Half3(new Half(5.12f), new Half(2), new Half(-17.54f)));
        }

        [Test]
        [Ignore("fix Half4.ToString() and update converter accordingly (to match other converters)")]
        public void TestHalf4()
        {
            TestConversionMultipleCultures(new Half4(new Half(5.12f), new Half(2), new Half(-17.54f), new Half(-5)));
        }

        [Test]
        public void TestMatrix()
        {
            TestConversionMultipleCultures(new Matrix(0.25f, 50.0f, -4.9f, 1, 5.12f, 2, -17.54f, -5, 10.25f, 150.0f, -14.9f, 11, 15.12f, 12, -117.54f, -15));
        }

        [Test]
        public void TestQuaternion()
        {
            TestConversionMultipleCultures(new Quaternion(5.12f, 2, -17.54f, -5));
        }

        [Test]
        public void TestVector2()
        {
            TestConversionMultipleCultures(new Vector2(5.12f, -17.54f));
        }

        [Test]
        public void TestVector3()
        {
            TestConversionMultipleCultures(new Vector3(5.12f, 2, -17.54f));
        }

        [Test]
        public void TestVector4()
        {
            TestConversionMultipleCultures(new Vector4(5.12f, 2, -17.54f, -5));
        }

        private static void TestConversionMultipleCultures<T>(T testValue)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            TestConversion(testValue);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            TestConversion(testValue);
        }

        private static void TestConversion<T>(T testValue)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            Assert.NotNull(converter);
            Assert.True(converter.CanConvertTo(typeof(string)));
            var value = converter.ConvertTo(testValue, typeof(string));
            Assert.AreEqual(testValue.ToString(), value);
            Assert.True(converter.CanConvertFrom(typeof(string)));
            var result = converter.ConvertFrom(value);
            Assert.AreEqual(testValue, result);
        }
    }
}