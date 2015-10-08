// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Templates;

namespace SiliconStudio.Assets.Tests
{
    /// <summary>
    /// Tests for the <see cref="TemplateManager"/> class.
    /// </summary>
    [TestFixture]
    public class TestTemplateManager: ITemplateGenerator
    {
        [Test, Ignore]
        public void TestTemplateDescriptions()
        {
            // Preload templates defined in Paradox.pdxpkg
            var descriptions = TemplateManager.FindTemplates().ToList();

            // Expect currently 4 templates
            Assert.AreEqual(23, descriptions.Count);
        }

        [Test]
        public void TestTemplateGenerator()
        {
            TemplateManager.Register(this);

            // Preload templates defined in Paradox.pdxpkg
            var descriptions = TemplateManager.FindTemplates().ToList();

            Assert.IsTrue(descriptions.Count > 0);

            var templateGenerator = TemplateManager.FindTemplateGenerator(descriptions[0]);

            Assert.AreEqual(this, templateGenerator);

            TemplateManager.Unregister(this);
        }

        public bool IsSupportingTemplate(TemplateDescription templateDescription)
        {
            return true;
        }

        public Func<bool> PrepareForRun(TemplateGeneratorParameters parameters)
        {
            // Nothing to do in the tests
            return null;
        }

        public bool AfterRun(TemplateGeneratorParameters parameters)
        {
            return true;
        }

        public static void Main()
        {
            var test = new TestTemplateManager();
            test.TestTemplateDescriptions();
        }
    }
}