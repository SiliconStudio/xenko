// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace SiliconStudio.Assets.Tests
{
    public class TestBase
    {
        public readonly string DirectoryTestBase = Path.Combine(AssemblyDirectory, @"data\SiliconStudio.Assets.Tests\");

        public static void GenerateAndCompare(string title, string outputFilePath, string referenceFilePath, Asset asset)
        {
            Console.WriteLine(title + @"- from file " + outputFilePath);
            Console.WriteLine(@"---------------------------------------");
            AssetFileSerializer.Save(outputFilePath, asset);
            var left = File.ReadAllText(outputFilePath).Trim();
            Console.WriteLine(left);
            var right = File.ReadAllText(referenceFilePath).Trim();
            Assert.That(left, Is.EqualTo(right));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

    }
}
