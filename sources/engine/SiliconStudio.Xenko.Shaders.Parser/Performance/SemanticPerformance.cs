// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

namespace SiliconStudio.Paradox.Shaders.Parser.Performance
{
    public static class SemanticPerformance
    {
        private static Stopwatch TotalTime = new Stopwatch();

        private static Stopwatch VisitVariable = new Stopwatch();
        private static Stopwatch CommonVisit = new Stopwatch();
        private static Stopwatch FindDeclarationScope = new Stopwatch();
        private static Stopwatch FindDeclarationsFromObject = new Stopwatch();
        private static Stopwatch FindDeclarations = new Stopwatch();
        private static Stopwatch ProcessMethodInvocation = new Stopwatch();
        private static Stopwatch CheckNameConflict = new Stopwatch();
        private static Stopwatch HasExternQualifier = new Stopwatch();

        private static int VisitVariableCount = 0;
        private static int CommonVisitCount = 0;
        private static int FindDeclarationScopeCount = 0;
        private static int FindDeclarationsFromObjectCount = 0;
        private static int FindDeclarationsCount = 0;
        private static int ProcessMethodInvocationCount = 0;
        private static int CheckNameConflictCount = 0;
        private static int HasExternQualifierCount = 0;

        private static int nbShaders = 0;

        public static void Start(SemanticStage stage)
        {
            switch (stage)
            {
                case SemanticStage.Global:
                    TotalTime.Start();
                    break;
                case SemanticStage.VisitVariable:
                    VisitVariable.Start();
                    ++VisitVariableCount;
                    break;
                case SemanticStage.CommonVisit:
                    CommonVisit.Start();
                    ++CommonVisitCount;
                    break;
                case SemanticStage.FindDeclarationScope:
                    FindDeclarationScope.Start();
                    ++FindDeclarationScopeCount;
                    break;
                case SemanticStage.FindDeclarationsFromObject:
                    FindDeclarationsFromObject.Start();
                    ++FindDeclarationsFromObjectCount;
                    break;
                case SemanticStage.FindDeclarations:
                    FindDeclarations.Start();
                    ++FindDeclarationsCount;
                    break;
                case SemanticStage.ProcessMethodInvocation:
                    ProcessMethodInvocation.Start();
                    ++ProcessMethodInvocationCount;
                    break;
                case SemanticStage.CheckNameConflict:
                    CheckNameConflict.Start();
                    ++CheckNameConflictCount;
                    break;
                case SemanticStage.HasExternQualifier:
                    HasExternQualifier.Start();
                    ++HasExternQualifierCount;
                    break;
            }
        }

        public static void Pause(SemanticStage stage)
        {
            switch (stage)
            {
                case SemanticStage.Global:
                    TotalTime.Stop();
                    break;
                case SemanticStage.VisitVariable:
                    VisitVariable.Stop();
                    break;
                case SemanticStage.CommonVisit:
                    CommonVisit.Stop();
                    break;
                case SemanticStage.FindDeclarationScope:
                    FindDeclarationScope.Stop();
                    break;
                case SemanticStage.FindDeclarationsFromObject:
                    FindDeclarationsFromObject.Stop();
                    break;
                case SemanticStage.FindDeclarations:
                    FindDeclarations.Stop();
                    break;
                case SemanticStage.ProcessMethodInvocation:
                    ProcessMethodInvocation.Stop();
                    break;
                case SemanticStage.CheckNameConflict:
                    CheckNameConflict.Stop();
                    break;
                case SemanticStage.HasExternQualifier:
                    HasExternQualifier.Stop();
                    break;
            }
        }

        public static void IncrShader()
        {
            ++nbShaders;
        }

        public static void Reset()
        {
            nbShaders = 0;
            
            TotalTime.Reset();
            VisitVariable.Reset();
            CommonVisit.Reset();
            FindDeclarationScope.Reset();
            FindDeclarationsFromObject.Reset();
            FindDeclarations.Reset();
            ProcessMethodInvocation.Reset();
            CheckNameConflict.Reset();
            HasExternQualifier.Reset();

            VisitVariableCount = 0;
            CommonVisitCount = 0;
            FindDeclarationScopeCount = 0;
            FindDeclarationsFromObjectCount = 0;
            FindDeclarationsCount = 0;
            ProcessMethodInvocationCount = 0;
            CheckNameConflictCount = 0;
            HasExternQualifierCount = 0;
        }

        public static void PrintResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"--------------------------TOTAL SEMANTIC ANALYZER---------------------------");
            Console.WriteLine(@"{0} shader(s) analyzed in {1} ms, {2} ms per shader", nbShaders, TotalTime.ElapsedMilliseconds, nbShaders == 0 ? 0 : TotalTime.ElapsedMilliseconds / nbShaders);
            Console.WriteLine(@"VisitVariable {0} ms for {1} calls", VisitVariable.ElapsedMilliseconds, VisitVariableCount);
            Console.WriteLine(@"CommonVisit took {0} ms for {1} calls", CommonVisit.ElapsedMilliseconds, CommonVisitCount);
            Console.WriteLine(@"FindDeclarationScope took {0} ms for {1} calls", FindDeclarationScope.ElapsedMilliseconds, FindDeclarationScopeCount);
            Console.WriteLine(@"FindDeclarationsFromObject took {0} ms for {1} calls", FindDeclarationsFromObject.ElapsedMilliseconds, FindDeclarationsFromObjectCount);
            Console.WriteLine(@"FindDeclarations took {0} ms for {1} calls", FindDeclarations.ElapsedMilliseconds, FindDeclarationsCount);
            Console.WriteLine(@"ProcessMethodInvocation took {0} ms for {1} calls", ProcessMethodInvocation.ElapsedMilliseconds, ProcessMethodInvocationCount);
            Console.WriteLine(@"CheckNameConflict took {0} ms for {1} calls", CheckNameConflict.ElapsedMilliseconds, CheckNameConflictCount);
            Console.WriteLine(@"HasExternQualifier took {0} ms for {1} calls", HasExternQualifier.ElapsedMilliseconds, HasExternQualifierCount);
            Console.WriteLine(@"-------------------------------------------------------------------------------");
            Console.WriteLine();
        }
    }

    public enum SemanticStage
    {
        Global,
        VisitVariable,
        CommonVisit,
        FindDeclarationScope,
        FindDeclarationsFromObject,
        FindDeclarations,
        ProcessMethodInvocation,
        CheckNameConflict,
        HasExternQualifier
    }
}