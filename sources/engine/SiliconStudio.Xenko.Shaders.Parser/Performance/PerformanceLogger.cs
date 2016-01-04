// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SiliconStudio.Xenko.Shaders.Parser.Performance
{
    public static class PerformanceLogger
    {
        private static int globalCount;
        private static int loadingCount;
        private static int typeAnalysisCount;
        private static int semanticAnalysisCount;
        private static int mixCount;
        private static int deepCloneCount;
        private static int astParsingCount;

        private static readonly List<long> GlobalTimes = new List<long>();
        private static readonly List<long> LoadingTimes = new List<long>();
        private static readonly List<long> TypeAnalysisTimes = new List<long>();
        private static readonly List<long> SemanticAnalysisTimes = new List<long>();
        private static readonly List<long> MixTimes = new List<long>();
        private static readonly List<long> DeepcloneTimes = new List<long>();
        private static readonly List<long> AstParsingTimes = new List<long>();
        
        private static Stopwatch globalWatch = new Stopwatch();
        private static Stopwatch loadingWatch = new Stopwatch();
        private static Stopwatch typeAnalysisWatch = new Stopwatch();
        private static Stopwatch semanticAnalysisWatch = new Stopwatch();
        private static Stopwatch mixWatch = new Stopwatch();
        private static Stopwatch deepCloneWatch = new Stopwatch();
        private static Stopwatch astParsingWatch = new Stopwatch();

        public static void Start(PerformanceStage stage)
        {
            switch (stage)
            {
                case PerformanceStage.Global:
                    globalWatch.Start();
                    break;
                case PerformanceStage.Loading:
                    loadingWatch.Start();
                    break;
                case PerformanceStage.TypeAnalysis:
                    typeAnalysisWatch.Start();
                    break;
                case PerformanceStage.SemanticAnalysis:
                    semanticAnalysisWatch.Start();
                    break;
                case PerformanceStage.Mix:
                    mixWatch.Start();
                    break;
                case PerformanceStage.DeepClone:
                    deepCloneWatch.Start();
                    break;
                case PerformanceStage.AstParsing:
                    astParsingWatch.Start();
                    break;
            }
        }

        public static void Pause(PerformanceStage stage)
        {
            switch (stage)
            {
                case PerformanceStage.Global:
                    globalWatch.Stop();
                    break;
                case PerformanceStage.Loading:
                    loadingWatch.Stop();
                    break;
                case PerformanceStage.TypeAnalysis:
                    typeAnalysisWatch.Stop();
                    break;
                case PerformanceStage.SemanticAnalysis:
                    semanticAnalysisWatch.Stop();
                    break;
                case PerformanceStage.Mix:
                    mixWatch.Stop();
                    break;
                case PerformanceStage.DeepClone:
                    deepCloneWatch.Stop();
                    break;
                case PerformanceStage.AstParsing:
                    astParsingWatch.Stop();
                    break;
            }
        }

        public static void Stop(PerformanceStage stage)
        {
            switch (stage)
            {
                case PerformanceStage.Global:
                    globalWatch.Stop();
                    GlobalTimes.Add(globalWatch.ElapsedMilliseconds);
                    ++globalCount;
                    break;
                case PerformanceStage.Loading:
                    loadingWatch.Stop();
                    LoadingTimes.Add(loadingWatch.ElapsedMilliseconds);
                    ++loadingCount;
                    break;
                case PerformanceStage.TypeAnalysis:
                    typeAnalysisWatch.Stop();
                    TypeAnalysisTimes.Add(typeAnalysisWatch.ElapsedMilliseconds);
                    ++typeAnalysisCount;
                    break;
                case PerformanceStage.SemanticAnalysis:
                    semanticAnalysisWatch.Stop();
                    SemanticAnalysisTimes.Add(semanticAnalysisWatch.ElapsedMilliseconds);
                    ++semanticAnalysisCount;
                    break;
                case PerformanceStage.Mix:
                    mixWatch.Stop();
                    MixTimes.Add(mixWatch.ElapsedMilliseconds);
                    ++mixCount;
                    break;
                case PerformanceStage.DeepClone:
                    deepCloneWatch.Stop();
                    DeepcloneTimes.Add(deepCloneWatch.ElapsedMilliseconds);
                    ++deepCloneCount;
                    break;
                case PerformanceStage.AstParsing:
                    astParsingWatch.Stop();
                    AstParsingTimes.Add(astParsingWatch.ElapsedMilliseconds);
                    ++astParsingCount;
                    break;
            }
        }

        public static void Reset()
        {
            globalWatch.Reset();
            loadingWatch.Reset();
            typeAnalysisWatch.Reset();
            semanticAnalysisWatch.Reset();
            mixWatch.Reset();
            deepCloneWatch.Reset();
            astParsingWatch.Reset();
        }

        public static void PrintResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"--------------------------TOTAL PERFORMANCE ANALYZER---------------------------");
            Console.WriteLine(@"Loading took {0} ms for {1} shader(s)", LoadingTimes.Aggregate((long)0, (agg, next) => agg + next), loadingCount);
            Console.WriteLine(@"Type analysis took {0} ms for {1} shader(s)", TypeAnalysisTimes.Aggregate((long)0, (agg, next) => agg + next), typeAnalysisCount);
            Console.WriteLine(@"Semantic analysis took {0} ms for {1} shader(s)", SemanticAnalysisTimes.Aggregate((long)0, (agg, next) => agg + next), semanticAnalysisCount);
            Console.WriteLine(@"Mix took {0} ms for {1} shader(s)", MixTimes.Aggregate((long)0, (agg, next) => agg + next), mixCount);
            Console.WriteLine(@"DeepClone took {0} ms for {1} shader(s)", DeepcloneTimes.Aggregate((long)0, (agg, next) => agg + next), deepCloneCount);
            Console.WriteLine(@"Ast parsing took {0} ms for {1} shader(s)", AstParsingTimes.Aggregate((long)0, (agg, next) => agg + next), astParsingCount);
            Console.WriteLine(@"-------------------------------------------------------------------------------");
            Console.WriteLine();
        }
        public static void PrintLastResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"--------------------------LAST PERFORMANCE ANALYZER---------------------------");
            Console.WriteLine(@"Process took {0} ms", globalWatch.ElapsedMilliseconds);
            Console.WriteLine(@"Loading took {0} ms", loadingWatch.ElapsedMilliseconds);
            Console.WriteLine(@"Type analysis took {0} ms", typeAnalysisWatch.ElapsedMilliseconds);
            Console.WriteLine(@"Semantic analysis took {0} ms", semanticAnalysisWatch.ElapsedMilliseconds);
            Console.WriteLine(@"Mix took {0} ms", mixWatch.ElapsedMilliseconds);
            Console.WriteLine(@"DeepClone took {0} ms", deepCloneWatch.ElapsedMilliseconds);
            Console.WriteLine(@"Ast parsing took {0} ms", astParsingWatch.ElapsedMilliseconds);
            Console.WriteLine(@"------------------------------------------------------------------------------");
            Console.WriteLine();
        }


        public static void WriteOut(int limit)
        {
            if (globalCount == limit)
            {
                PrintResult();
                TextWriter tw = new StreamWriter(File.Open("performance.csv", FileMode.Append));
                tw.WriteLine("loading,type,semantic,mix,deepclone,global");

                for (var i = 0; i < limit; ++i)
                {
                    tw.WriteLine("{0},{1},{2},{3},{4},{5}", LoadingTimes[i], TypeAnalysisTimes[i], SemanticAnalysisTimes[i], MixTimes[i], DeepcloneTimes[i], GlobalTimes[i]);
                }
                tw.Dispose();
            }
        }
    }

    public enum PerformanceStage
    {
        Global,
        Loading,
        TypeAnalysis,
        SemanticAnalysis,
        Mix,
        DeepClone,
        AstParsing
    }
}
