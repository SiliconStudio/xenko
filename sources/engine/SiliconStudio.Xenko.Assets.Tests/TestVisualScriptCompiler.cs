// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Assets.Scripts;

namespace SiliconStudio.Xenko.Assets.Tests
{
    [TestFixture]
    public class TestVisualScriptCompiler
    {
        [Test]
        public void TestCustomCode()
        {
            var visualScript = new VisualScriptAsset();

            // Build blocks
            var functionStart = new FunctionStartBlock { FunctionName = "Test" };
            var writeTrue = new CustomCodeBlock { Code = "System.Console.Write(true);" };
            visualScript.Blocks.Add(functionStart);
            visualScript.Blocks.Add(writeTrue);

            // Generate slots
            foreach (var block in visualScript.Blocks)
                block.RegenerateSlots();

            // Build links
            visualScript.Links.Add(new Link(functionStart.StartSlot, writeTrue.ExecutionInput));

            // Test
            TestAndCompareOutput(visualScript, "True");
        }

        [Test]
        public void TestConditionalExpression()
        {
            var visualScript = new VisualScriptAsset();

            // Build blocks
            var functionStart = new FunctionStartBlock { FunctionName = "Test" };
            var conditionalBranch = new ConditionalBranchBlock();
            var writeTrue = new CustomCodeBlock { Code = "System.Console.Write(true);" };
            var writeFalse = new CustomCodeBlock { Code = "System.Console.Write(false);" };
            visualScript.Blocks.Add(functionStart);
            visualScript.Blocks.Add(conditionalBranch);
            visualScript.Blocks.Add(writeTrue);
            visualScript.Blocks.Add(writeFalse);

            // Generate slots
            foreach (var block in visualScript.Blocks)
                block.RegenerateSlots();

            // Build links
            visualScript.Links.Add(new Link(functionStart.StartSlot, conditionalBranch.ExecutionInput));
            visualScript.Links.Add(new Link(conditionalBranch.TrueSlot, writeTrue.ExecutionInput));
            visualScript.Links.Add(new Link(conditionalBranch.FalseSlot, writeFalse.ExecutionInput));

            // Test
            TestAndCompareOutput(visualScript, "True");
        }

        private static void TestAndCompareOutput(VisualScriptAsset visualScriptAsset, string expectedOutput)
        {
            // Compile
            var compilerResult = VisualScriptCompiler.Generate(visualScriptAsset, new VisualScriptCompilerOptions
            {
                Class = "TestClass",
            });

            using (var textWriter = new StringWriter())
            {
                Console.SetOut(textWriter);

                // Create class
                var testInstance = CreateInstance(new[] { compilerResult.SyntaxTree });
                // Execute method
                testInstance.Test();

                // Check output
                textWriter.Flush();
                Assert.That(textWriter.ToString(), Is.EqualTo(expectedOutput));

                // Restore Console.Out
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }
        }

        private static dynamic CreateInstance(SyntaxTree[] syntaxTrees)
        {
            var compilation = CSharpCompilation.Create("Test.dll",
                syntaxTrees,
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var peStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(peStream, pdbStream);

                Assert.That(emitResult.Success);

                peStream.Position = 0;
                pdbStream.Position = 0;

                var assembly = Assembly.Load(peStream.ToArray(), pdbStream.ToArray());
                var @class = assembly.GetType("TestClass");
                return Activator.CreateInstance(@class);
            }
        }
    }
}