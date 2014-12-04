// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using SiliconStudio.Paradox.Shaders.Parser.Mixins;

namespace SiliconStudio.Paradox.Shaders.Tests
{
    /// <summary>
    /// Code used to regenerate all cs files from pdxsl/pdxfx in the project
    /// </summary>
    [TestFixture]
    public class TestCodeGen
    {
        //[Test]
        public void Test()
        {
            var filePath = @"D:\Code\Paradox\sources\engine\SiliconStudio.Paradox.Shaders.Tests\GameAssets\Mixins\A.pdxsl";
            var source = File.ReadAllText(filePath);
            var content = ShaderMixinCodeGen.GenerateCsharp(source, filePath.Replace("C:", "D:"));
        }

        //[Test] // Decomment this line to regenerate all files (sources and samples)
        public void RebuildAllPdxfxPdxsl()
        {
            RegenerateDirectory(Path.Combine(Environment.CurrentDirectory, @"..\..\sources"));
            RegenerateDirectory(Path.Combine(Environment.CurrentDirectory, @"..\..\samples"));
        }

        private static void RegenerateDirectory(string directory)
        {
            //foreach (var pdxsl in Directory.EnumerateFiles(directory, "*.pdxsl", SearchOption.AllDirectories))
            //{
            //    RebuildFile(pdxsl);
            //}
            foreach (var pdxfx in Directory.EnumerateFiles(directory, "*.pdxfx", SearchOption.AllDirectories))
            {
                RebuildFile(pdxfx);
            }
        }

        private static void RebuildFile(string filePath)
        {
            try
            {
                var source = File.ReadAllText(filePath);
                var content = ShaderMixinCodeGen.GenerateCsharp(source, filePath.Replace("C:", "D:"));
                var destPath = Path.ChangeExtension(filePath, ".cs");
                File.WriteAllText(destPath, content, Encoding.UTF8);
                Console.WriteLine("File generated {0}", filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error {0}: {1}", filePath, ex);
            }
        }
    }
}