// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                block.GenerateSlots(block.Slots, new SlotGeneratorContext());

            // Build links
            visualScript.Links.Add(new Link(functionStart, writeTrue));

            // Test
            TestAndCompareOutput(visualScript, "True", testInstance => testInstance.Test());
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
                block.GenerateSlots(block.Slots, new SlotGeneratorContext());

            // Build links
            visualScript.Links.Add(new Link(functionStart, conditionalBranch));
            visualScript.Links.Add(new Link(conditionalBranch.TrueSlot, writeTrue));
            visualScript.Links.Add(new Link(conditionalBranch.FalseSlot, writeFalse));

            // Test
            conditionalBranch.ConditionSlot.Value = true;
            TestAndCompareOutput(visualScript, "True", testInstance => testInstance.Test());

            conditionalBranch.ConditionSlot.Value = false;
            TestAndCompareOutput(visualScript, "False", testInstance => testInstance.Test());
        }

        [Test]
        public void TestVariableGet()
        {
            var visualScript = new VisualScriptAsset();

            var condition = new Variable("bool", "Condition");
            visualScript.Variables.Add(condition);

            // Build blocks
            // TODO: Switch to a simple Write(variable) later, so that we don't depend on ConditionalBranchBlock for this test?
            var functionStart = new FunctionStartBlock { FunctionName = "Test" };
            var conditionGet = new VariableGet { Variable = condition };
            var conditionalBranch = new ConditionalBranchBlock();
            var writeTrue = new CustomCodeBlock { Code = "System.Console.Write(true);" };
            var writeFalse = new CustomCodeBlock { Code = "System.Console.Write(false);" };
            visualScript.Blocks.Add(functionStart);
            visualScript.Blocks.Add(conditionGet);
            visualScript.Blocks.Add(conditionalBranch);
            visualScript.Blocks.Add(writeTrue);
            visualScript.Blocks.Add(writeFalse);

            // Generate slots
            foreach (var block in visualScript.Blocks)
                block.GenerateSlots(block.Slots, new SlotGeneratorContext());

            // Build links
            visualScript.Links.Add(new Link(functionStart, conditionalBranch));
            visualScript.Links.Add(new Link(conditionGet.ValueSlot, conditionalBranch.ConditionSlot));
            visualScript.Links.Add(new Link(conditionalBranch.TrueSlot, writeTrue));
            visualScript.Links.Add(new Link(conditionalBranch.FalseSlot, writeFalse));

            // Test
            TestAndCompareOutput(visualScript, "True", testInstance =>
            {
                testInstance.Condition = true;
                testInstance.Test();
            });

            TestAndCompareOutput(visualScript, "False", testInstance =>
            {
                testInstance.Condition = false;
                testInstance.Test();
            });
        }

        [Test]
        public void TestVariableSet()
        {
            var visualScript = new VisualScriptAsset();

            var condition = new Variable("bool", "Condition");
            visualScript.Variables.Add(condition);

            // Build blocks
            // TODO: Switch to a simple Write(variable) later, so that we don't depend on ConditionalBranchBlock for this test?
            var functionStart = new FunctionStartBlock { FunctionName = "Test" };
            var conditionGet = new VariableGet { Variable = condition };
            var conditionSet = new VariableSet { Variable = condition };
            var conditionalBranch = new ConditionalBranchBlock();
            var writeTrue = new CustomCodeBlock { Code = "System.Console.Write(true);" };
            var writeFalse = new CustomCodeBlock { Code = "System.Console.Write(false);" };
            visualScript.Blocks.Add(functionStart);
            visualScript.Blocks.Add(conditionGet);
            visualScript.Blocks.Add(conditionSet);
            visualScript.Blocks.Add(conditionalBranch);
            visualScript.Blocks.Add(writeTrue);
            visualScript.Blocks.Add(writeFalse);

            // Generate slots
            foreach (var block in visualScript.Blocks)
                block.GenerateSlots(block.Slots, new SlotGeneratorContext());

            // Build links
            visualScript.Links.Add(new Link(functionStart, conditionSet));
            visualScript.Links.Add(new Link(conditionSet, conditionalBranch));
            visualScript.Links.Add(new Link(conditionGet.ValueSlot, conditionalBranch.ConditionSlot));
            visualScript.Links.Add(new Link(conditionalBranch.TrueSlot, writeTrue));
            visualScript.Links.Add(new Link(conditionalBranch.FalseSlot, writeFalse));

            // Test
            conditionSet.InputSlot.Value = true;
            TestAndCompareOutput(visualScript, "True", testInstance =>
            {
                testInstance.Test();
            });

            conditionSet.InputSlot.Value = false;
            TestAndCompareOutput(visualScript, "False", testInstance =>
            {
                testInstance.Test();
            });
        }

        private static void TestAndCompareOutput(VisualScriptAsset visualScriptAsset, string expectedOutput, Action<dynamic> testCode)
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
                testCode(testInstance);

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

                if (!emitResult.Success)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Compilation errors:");
                    foreach (var diagnostic in emitResult.Diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error))
                    {
                        sb.AppendLine(diagnostic.ToString());
                    }

                    throw new InvalidOperationException(sb.ToString());
                }

                peStream.Position = 0;
                pdbStream.Position = 0;

                var assembly = Assembly.Load(peStream.ToArray(), pdbStream.ToArray());
                var @class = assembly.GetType("TestClass");
                return Activator.CreateInstance(@class);
            }
        }
    }
}