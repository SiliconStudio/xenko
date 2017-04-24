// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.IO;
using NUnit.Framework;

namespace SiliconStudio.ProjectTemplating.Tests
{
    [TestFixture]
    class Program
    {
        [Test]
        public void TestProjectTemplate()
        {
            var projectTemplate = ProjectTemplate.Load(@"..\..\Test\TestProjectTemplate.ttproj");
            var outputDir = Environment.CurrentDirectory + "\\OutputTemp";
            try
            {
                Directory.Delete(outputDir, true);
            }
            catch (Exception)
            {
            }

            var result = projectTemplate.Generate(outputDir, "TestProject", Guid.NewGuid());
            Assert.IsFalse(result.HasErrors);

            Assert.IsTrue(File.Exists(Path.Combine(outputDir, "TestProject.cs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDir, @"SubFolder\TextRaw.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDir, @"SubFolder\TextTemplate1.cs")));

            Assert.AreEqual("This is a test with the file name using the property $ProjectName$ = \"TestProject\"", File.ReadAllText(Path.Combine(outputDir, @"TestProject.cs")).Trim());
            Assert.AreEqual(File.ReadAllText(@"..\..\Test\SubFolder\TextRaw.txt"), File.ReadAllText(Path.Combine(outputDir, @"SubFolder\TextRaw.txt")));
            Assert.AreEqual("This is a test of template with the project name TestProject and 5", File.ReadAllText(Path.Combine(outputDir, @"SubFolder\TextTemplate1.cs")).Trim());
        }

        static void Main(string[] args)
        {
            var program = new Program();
            program.TestProjectTemplate();
        }
    }
}
