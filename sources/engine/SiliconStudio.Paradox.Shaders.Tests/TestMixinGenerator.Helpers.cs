// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Tests
{
    /// <summary>
    /// Helper methods for TestMixinGenerator
    /// </summary>
    public partial class TestMixinGenerator
    {
        /// <summary>
        /// Generates the mixin.
        /// </summary>
        /// <param name="mixinName">Name of the mixin.</param>
        /// <param name="properties">The properties that the mixin will use.</param>
        /// <param name="usedProperties">The properties effectively used by the mixin.</param>
        /// <returns>ShaderMixinSource.</returns>
        private static ShaderMixinSourceTree GenerateMixin(string mixinName, ParameterCollection properties, out ShaderMixinParameters usedProperties)
        {
            var allUsedProperties = new List<ShaderMixinParameters>();
            var mixin = ShaderMixinManager.Generate(mixinName, properties);

            usedProperties = mixin.UsedParameters;

            // Verify that output used properties are a subset of input properties
            Assert.That(usedProperties.IsSubsetOf(properties), Is.True);

            foreach(var usedProps in allUsedProperties)
                Assert.That(usedProps.IsSubsetOf(properties), Is.True);

            return mixin;
        }
    }
}