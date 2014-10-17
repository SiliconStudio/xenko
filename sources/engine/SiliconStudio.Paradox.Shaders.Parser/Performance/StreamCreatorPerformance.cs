// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

namespace SiliconStudio.Paradox.Shaders.Parser.Performance
{
    public static class StreamCreatorPerformance
    {
        private static Stopwatch Global = new Stopwatch();
        private static Stopwatch StreamAnalyzer = new Stopwatch();
        private static Stopwatch FindEntryPoint = new Stopwatch();
        private static Stopwatch StreamAnalysisPerShader = new Stopwatch();
        private static Stopwatch BubbleUpStreamUsages = new Stopwatch();
        private static Stopwatch ComputeShaderStreamAnalysis = new Stopwatch();
        private static Stopwatch TagCleaner = new Stopwatch();
        private static Stopwatch GenerateStreams = new Stopwatch();
        private static Stopwatch RemoveUselessAndSortMethods = new Stopwatch();
        private static Stopwatch PropagateStreamsParameter = new Stopwatch();
        private static Stopwatch TransformStreamsAssignments = new Stopwatch();
        private static Stopwatch AssignSearch = new Stopwatch();
        private static Stopwatch CreateOutputFromStream = new Stopwatch();
        private static Stopwatch CreateStreamFromInput = new Stopwatch();
        private static Stopwatch StreamFieldVisitor = new Stopwatch();
        private static Stopwatch StreamFieldVisitorClone = new Stopwatch();
        
        private static int StreamFieldVisitorCount;

        public static void Start(StreamCreatorStage stage)
        {
            switch (stage)
            {
                case StreamCreatorStage.Global:
                    Global.Start();
                    break;
                case StreamCreatorStage.StreamAnalyzer:
                    StreamAnalyzer.Start();
                    break;
                case StreamCreatorStage.FindEntryPoint:
                    FindEntryPoint.Start();
                    break;
                case StreamCreatorStage.StreamAnalysisPerShader:
                    StreamAnalysisPerShader.Start();
                    break;
                case StreamCreatorStage.BubbleUpStreamUsages:
                    BubbleUpStreamUsages.Start();
                    break;
                case StreamCreatorStage.ComputeShaderStreamAnalysis:
                    ComputeShaderStreamAnalysis.Start();
                    break;
                case StreamCreatorStage.TagCleaner:
                    TagCleaner.Start();
                    break;
                case StreamCreatorStage.GenerateStreams:
                    GenerateStreams.Start();
                    break;
                case StreamCreatorStage.RemoveUselessAndSortMethods:
                    RemoveUselessAndSortMethods.Start();
                    break;
                case StreamCreatorStage.PropagateStreamsParameter:
                    PropagateStreamsParameter.Start();
                    break;
                case StreamCreatorStage.TransformStreamsAssignments:
                    TransformStreamsAssignments.Start();
                    break;
                case StreamCreatorStage.AssignSearch:
                    AssignSearch.Start();
                    break;
                case StreamCreatorStage.CreateOutputFromStream:
                    CreateOutputFromStream.Start();
                    break;
                case StreamCreatorStage.CreateStreamFromInput:
                    CreateStreamFromInput.Start();
                    break;
                case StreamCreatorStage.StreamFieldVisitor:
                    StreamFieldVisitor.Start();
                    ++StreamFieldVisitorCount;
                    break;
                case StreamCreatorStage.StreamFieldVisitorClone:
                    StreamFieldVisitorClone.Start();
                    break;
            }
        }

        public static void Pause(StreamCreatorStage stage)
        {
            switch (stage)
            {
                case StreamCreatorStage.Global:
                    Global.Stop();
                    break;
                case StreamCreatorStage.StreamAnalyzer:
                    StreamAnalyzer.Stop();
                    break;
                case StreamCreatorStage.FindEntryPoint:
                    FindEntryPoint.Stop();
                    break;
                case StreamCreatorStage.StreamAnalysisPerShader:
                    StreamAnalysisPerShader.Stop();
                    break;
                case StreamCreatorStage.BubbleUpStreamUsages:
                    BubbleUpStreamUsages.Stop();
                    break;
                case StreamCreatorStage.ComputeShaderStreamAnalysis:
                    ComputeShaderStreamAnalysis.Stop();
                    break;
                case StreamCreatorStage.TagCleaner:
                    TagCleaner.Stop();
                    break;
                case StreamCreatorStage.GenerateStreams:
                    GenerateStreams.Stop();
                    break;
                case StreamCreatorStage.RemoveUselessAndSortMethods:
                    RemoveUselessAndSortMethods.Stop();
                    break;
                case StreamCreatorStage.PropagateStreamsParameter:
                    PropagateStreamsParameter.Stop();
                    break;
                case StreamCreatorStage.TransformStreamsAssignments:
                    TransformStreamsAssignments.Stop();
                    break;
                case StreamCreatorStage.AssignSearch:
                    AssignSearch.Stop();
                    break;
                case StreamCreatorStage.CreateOutputFromStream:
                    CreateOutputFromStream.Stop();
                    break;
                case StreamCreatorStage.CreateStreamFromInput:
                    CreateStreamFromInput.Stop();
                    break;
                case StreamCreatorStage.StreamFieldVisitor:
                    StreamFieldVisitor.Stop();
                    break;
                case StreamCreatorStage.StreamFieldVisitorClone:
                    StreamFieldVisitorClone.Stop();
                    break;
            }
        }

        public static void Reset()
        {
            Global.Reset();
            StreamAnalyzer.Reset();
            FindEntryPoint.Reset();
            StreamAnalysisPerShader.Reset();
            BubbleUpStreamUsages.Reset();
            ComputeShaderStreamAnalysis.Reset();
            TagCleaner.Reset();
            GenerateStreams.Reset();
            RemoveUselessAndSortMethods.Reset();
            PropagateStreamsParameter.Reset();
            TransformStreamsAssignments.Reset();
            AssignSearch.Reset();
            CreateOutputFromStream.Reset();
            CreateStreamFromInput.Reset();
            StreamFieldVisitor.Reset();
            StreamFieldVisitorClone.Reset();

            StreamFieldVisitorCount = 0;
        }

        public static void PrintResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"----------------------------STREAM CREATOR ANALYZER-----------------------------");
            Console.WriteLine(@"Stream creation took {0} ms", Global.ElapsedMilliseconds);
            Console.WriteLine(@"StreamAnalyzer took {0} ms", StreamAnalyzer.ElapsedMilliseconds);
            Console.WriteLine(@"FindEntryPoint took {0} ms", FindEntryPoint.ElapsedMilliseconds);
            Console.WriteLine(@"StreamAnalysisPerShader took {0} ms", StreamAnalysisPerShader.ElapsedMilliseconds);
            Console.WriteLine(@"BubbleUpStreamUsages took {0} ms", BubbleUpStreamUsages.ElapsedMilliseconds);
            Console.WriteLine(@"ComputeShaderStreamAnalysis took {0} ms", ComputeShaderStreamAnalysis.ElapsedMilliseconds);
            Console.WriteLine(@"TagCleaner took {0} ms", TagCleaner.ElapsedMilliseconds);
            Console.WriteLine(@"GenerateStreams took {0} ms", GenerateStreams.ElapsedMilliseconds);
            Console.WriteLine(@"RemoveUselessAndSortMethods took {0} ms", RemoveUselessAndSortMethods.ElapsedMilliseconds);
            Console.WriteLine(@"PropagateStreamsParameter took {0} ms", PropagateStreamsParameter.ElapsedMilliseconds);
            Console.WriteLine(@"TransformStreamsAssignments took {0} ms", TransformStreamsAssignments.ElapsedMilliseconds);
            Console.WriteLine(@"AssignSearch took {0} ms", AssignSearch.ElapsedMilliseconds);
            Console.WriteLine(@"CreateOutputFromStream took {0} ms", CreateOutputFromStream.ElapsedMilliseconds);
            Console.WriteLine(@"CreateStreamFromInput took {0} ms", CreateStreamFromInput.ElapsedMilliseconds);
            Console.WriteLine(@"StreamFieldVisitor took {0} ms for {1} calls", StreamFieldVisitor.ElapsedMilliseconds, StreamFieldVisitorCount);
            Console.WriteLine(@"StreamFieldVisitorClone took {0} ms", StreamFieldVisitorClone.ElapsedMilliseconds);
            Console.WriteLine(@"-------------------------------------------------------------------------------");
            Console.WriteLine();
        }
    }

    public enum StreamCreatorStage
    {
        Global,
        StreamAnalyzer,
        FindEntryPoint,
        StreamAnalysisPerShader,
        BubbleUpStreamUsages,
        ComputeShaderStreamAnalysis,
        TagCleaner,
        GenerateStreams,
        RemoveUselessAndSortMethods,
        PropagateStreamsParameter,
        TransformStreamsAssignments,
        AssignSearch,
        CreateOutputFromStream,
        CreateStreamFromInput,
        StreamFieldVisitor,
        StreamFieldVisitorClone
    }
}
