// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Tests
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
        /// <returns>ShaderMixinSource.</returns>
        private static ShaderMixinSource GenerateMixin(string mixinName, ParameterCollection properties)
        {
            var mixin = ShaderMixinManager.Generate(mixinName, properties);

            // Verify that output used properties are a subset of input properties
            //Assert.That(usedProperties.IsSubsetOf(properties), Is.True);

            //foreach(var usedProps in allUsedProperties)
            //    Assert.That(usedProps.IsSubsetOf(properties), Is.True);

            return mixin;
        }
    }
}