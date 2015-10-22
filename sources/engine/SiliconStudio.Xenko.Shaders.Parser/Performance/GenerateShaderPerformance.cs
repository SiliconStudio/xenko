// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Shaders.Parser.Performance
{
    public static class GenerateShaderPerformance
    {
        private static Stopwatch Global = new Stopwatch();
        private static Stopwatch GroupByConstantBuffer = new Stopwatch();
        private static Stopwatch StreamCreator = new Stopwatch();
        private static Stopwatch ExpandForEachStatements = new Stopwatch();
        private static Stopwatch RemoveUselessVariables = new Stopwatch();

        public static void Start(GenerateShaderStage stage)
        {
            switch (stage)
            {
                case GenerateShaderStage.Global:
                    Global.Start();
                    break;
                case GenerateShaderStage.GroupByConstantBuffer:
                    GroupByConstantBuffer.Start();
                    break;
                case GenerateShaderStage.StreamCreator:
                    StreamCreator.Start();
                    break;
                case GenerateShaderStage.ExpandForEachStatements:
                    ExpandForEachStatements.Start();
                    break;
                case GenerateShaderStage.RemoveUselessVariables:
                    RemoveUselessVariables.Start();
                    break;
            }
        }

        public static void Pause(GenerateShaderStage stage)
        {
            switch (stage)
            {
                case GenerateShaderStage.Global:
                    Global.Stop();
                    break;
                case GenerateShaderStage.GroupByConstantBuffer:
                    GroupByConstantBuffer.Stop();
                    break;
                case GenerateShaderStage.StreamCreator:
                    StreamCreator.Stop();
                    break;
                case GenerateShaderStage.ExpandForEachStatements:
                    ExpandForEachStatements.Stop();
                    break;
                case GenerateShaderStage.RemoveUselessVariables:
                    RemoveUselessVariables.Start();
                    break;
            }
        }

        public static void Reset()
        {
            Global.Reset();
            GroupByConstantBuffer.Reset();
            StreamCreator.Reset();
            ExpandForEachStatements.Reset();
            RemoveUselessVariables.Reset();
        }

        public static void PrintResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"----------------------------GENERATE SHADER ANALYZER-----------------------------");
            Console.WriteLine(@"Whole generation took {0} ms", Global.ElapsedMilliseconds);
            Console.WriteLine(@"GroupByConstantBuffer took {0} ms", GroupByConstantBuffer.ElapsedMilliseconds);
            Console.WriteLine(@"StreamCreator took {0} ms", StreamCreator.ElapsedMilliseconds);
            Console.WriteLine(@"ExpandForEachStatements took {0} ms", ExpandForEachStatements.ElapsedMilliseconds);
            Console.WriteLine(@"RemoveUselessVariables took {0} ms", RemoveUselessVariables.ElapsedMilliseconds);
            Console.WriteLine(@"-------------------------------------------------------------------------------");
            Console.WriteLine();
        }
    }

    public enum GenerateShaderStage
    {
        Global,
        GroupByConstantBuffer,
        StreamCreator,
        ExpandForEachStatements,
        RemoveUselessVariables
    }
}
