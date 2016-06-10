// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Shaders.Parser.Performance
{
    public static class MixPerformance
    {
        private static Stopwatch Global = new Stopwatch();
        private static Stopwatch AddDefaultCompositions = new Stopwatch();
        private static Stopwatch CreateReferencesStructures = new Stopwatch();
        private static Stopwatch RegenKeys = new Stopwatch();
        private static Stopwatch BuildMixinInheritance = new Stopwatch();
        private static Stopwatch ComputeMixinOccurrence = new Stopwatch();
        private static Stopwatch BuildStageInheritance = new Stopwatch();
        private static Stopwatch LinkVariables = new Stopwatch();
        private static Stopwatch ProcessExterns = new Stopwatch();
        private static Stopwatch PatchAllMethodInferences = new Stopwatch();
        private static Stopwatch MergeReferences = new Stopwatch();
        private static Stopwatch RenameAllVariables = new Stopwatch();
        private static Stopwatch RenameAllMethods = new Stopwatch();
        private static Stopwatch GenerateShader = new Stopwatch();
        
        public static void Start(MixStage stage)
        {
            switch (stage)
            {
                case MixStage.Global:
                    Global.Start();
                    break;
                case MixStage.AddDefaultCompositions:
                    AddDefaultCompositions.Start();
                    break;
                case MixStage.CreateReferencesStructures:
                    CreateReferencesStructures.Start();
                    break;
                case MixStage.RegenKeys:
                    RegenKeys.Start();
                    break;
                case MixStage.BuildMixinInheritance:
                    BuildMixinInheritance.Start();
                    break;
                case MixStage.ComputeMixinOccurrence:
                    ComputeMixinOccurrence.Start();
                    break;
                case MixStage.BuildStageInheritance:
                    BuildStageInheritance.Start();
                    break;
                case MixStage.LinkVariables:
                    LinkVariables.Start();
                    break;
                case MixStage.ProcessExterns:
                    ProcessExterns.Start();
                    break;
                case MixStage.PatchAllMethodInferences:
                    PatchAllMethodInferences.Start();
                    break;
                case MixStage.MergeReferences:
                    MergeReferences.Start();
                    break;
                case MixStage.RenameAllVariables:
                    RenameAllVariables.Start();
                    break;
                case MixStage.RenameAllMethods:
                    RenameAllMethods.Start();
                    break;
                case MixStage.GenerateShader:
                    GenerateShader.Start();
                    break;
            }
        }

        public static void Pause(MixStage stage)
        {
            switch (stage)
            {
                case MixStage.Global:
                    Global.Stop();
                    break;
                case MixStage.AddDefaultCompositions:
                    AddDefaultCompositions.Stop();
                    break;
                case MixStage.CreateReferencesStructures:
                    CreateReferencesStructures.Stop();
                    break;
                case MixStage.RegenKeys:
                    RegenKeys.Stop();
                    break;
                case MixStage.BuildMixinInheritance:
                    BuildMixinInheritance.Stop();
                    break;
                case MixStage.ComputeMixinOccurrence:
                    ComputeMixinOccurrence.Stop();
                    break;
                case MixStage.BuildStageInheritance:
                    BuildStageInheritance.Stop();
                    break;
                case MixStage.LinkVariables:
                    LinkVariables.Stop();
                    break;
                case MixStage.ProcessExterns:
                    ProcessExterns.Stop();
                    break;
                case MixStage.PatchAllMethodInferences:
                    PatchAllMethodInferences.Stop();
                    break;
                case MixStage.MergeReferences:
                    MergeReferences.Stop();
                    break;
                case MixStage.RenameAllVariables:
                    RenameAllVariables.Stop();
                    break;
                case MixStage.RenameAllMethods:
                    RenameAllMethods.Stop();
                    break;
                case MixStage.GenerateShader:
                    GenerateShader.Stop();
                    break;
            }
        }

        public static void Reset()
        {
            Global.Reset();
            AddDefaultCompositions.Reset();
            CreateReferencesStructures.Reset();
            RegenKeys.Reset();
            BuildMixinInheritance.Reset();
            ComputeMixinOccurrence.Reset();
            BuildStageInheritance.Reset();
            LinkVariables.Reset();
            ProcessExterns.Reset();
            PatchAllMethodInferences.Reset();
            MergeReferences.Reset();
            RenameAllVariables.Reset();
            RenameAllMethods.Reset();
            GenerateShader.Reset();
        }

        public static void PrintResult()
        {
            Console.WriteLine();
            Console.WriteLine(@"---------------------------------MIX ANALYZER-----------------------------------");
            Console.WriteLine(@"Whole mix took {0} ms", Global.ElapsedMilliseconds);
            Console.WriteLine(@"AddDefaultCompositions took {0} ms", AddDefaultCompositions.ElapsedMilliseconds);
            Console.WriteLine(@"CreateReferencesStructures took {0} ms", CreateReferencesStructures.ElapsedMilliseconds);
            Console.WriteLine(@"RegenKeys took {0} ms", RegenKeys.ElapsedMilliseconds);
            Console.WriteLine(@"BuildMixinInheritance took {0} ms", BuildMixinInheritance.ElapsedMilliseconds);
            Console.WriteLine(@"ComputeMixinOccurrence took {0} ms", ComputeMixinOccurrence.ElapsedMilliseconds);
            Console.WriteLine(@"BuildStageInheritance took {0} ms", BuildStageInheritance.ElapsedMilliseconds);
            Console.WriteLine(@"LinkVariables took {0} ms", LinkVariables.ElapsedMilliseconds);
            Console.WriteLine(@"ProcessExterns took {0} ms", ProcessExterns.ElapsedMilliseconds);
            Console.WriteLine(@"PatchAllMethodInferences took {0} ms", PatchAllMethodInferences.ElapsedMilliseconds);
            Console.WriteLine(@"MergeReferences took {0} ms", MergeReferences.ElapsedMilliseconds);
            Console.WriteLine(@"RenameAllVariables took {0} ms", RenameAllVariables.ElapsedMilliseconds);
            Console.WriteLine(@"RenameAllMethods took {0} ms", RenameAllMethods.ElapsedMilliseconds);
            Console.WriteLine(@"GenerateShader took {0} ms", GenerateShader.ElapsedMilliseconds);
            Console.WriteLine(@"-------------------------------------------------------------------------------");
            Console.WriteLine();
        }
    }

    public enum MixStage
    {
        Global,
        AddDefaultCompositions,
        CreateReferencesStructures,
        RegenKeys,
        BuildMixinInheritance,
        ComputeMixinOccurrence,
        BuildStageInheritance,
        LinkVariables,
        ProcessExterns,
        PatchAllMethodInferences,
        MergeReferences,
        RenameAllVariables,
        RenameAllMethods,
        GenerateShader
    }
}
