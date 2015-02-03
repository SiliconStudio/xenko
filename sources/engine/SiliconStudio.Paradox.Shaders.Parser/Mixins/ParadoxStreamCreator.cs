// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Utility;

using ParameterQualifier = SiliconStudio.Shaders.Ast.ParameterQualifier;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal  class ParadoxStreamCreator
    {
        #region private static members

        private readonly static string[] GeometryShaderUnOptimizedSemantics = { "SV_RenderTargetArrayIndex" };

        #endregion

        #region Private members

        /// <summary>
        /// The shader
        /// </summary>
        private ShaderClassType shader;

        /// <summary>
        /// the main ModuleMixin corresonding to the shader
        /// </summary>
        private ModuleMixin mainModuleMixin;

        /// <summary>
        /// Ordered list of all the mixin in their appearance order
        /// </summary>
        private List<ModuleMixin> mixinInheritance = new List<ModuleMixin>();

        /// <summary>
        /// the entry points of the shader
        /// </summary>
        private HashSet<MethodDefinition> entryPointMethods = new HashSet<MethodDefinition>();

        /// <summary>
        /// All the streams usages
        /// </summary>
        private Dictionary<MethodDeclaration, List<StreamUsageInfo>> streamsUsages = new Dictionary<MethodDeclaration, List<StreamUsageInfo>>();

        /// <summary>
        /// List of methods that need streams structure.
        /// </summary>
        private Dictionary<PdxShaderStage, List<MethodDeclaration>> methodsPerShaderStage = new Dictionary<PdxShaderStage, List<MethodDeclaration>>();

        /// <summary>
        /// Stream analyzer
        /// </summary>
        private ParadoxStreamAnalyzer streamAnalyzer;

        /// <summary>
        /// the error logger
        /// </summary>
        private LoggerResult errorWarningLog;

        #endregion

        #region Constructor

        private ParadoxStreamCreator(ShaderClassType shaderClassType, ModuleMixin mixin, List<ModuleMixin> mixins, LoggerResult errorLog)
        {
            shader = shaderClassType;
            mainModuleMixin = mixin;
            mixinInheritance = mixins;
            errorWarningLog = errorLog ?? new LoggerResult();
        }

        public static void Run(ShaderClassType shaderClassType, ModuleMixin mixin, List<ModuleMixin> mixins, LoggerResult errorLog)
        {
            var streamCreator = new ParadoxStreamCreator(shaderClassType, mixin, mixins, errorLog);
            streamCreator.Run();
        }

        #endregion

        #region Public method

        public void Run()
        {
            streamAnalyzer = new ParadoxStreamAnalyzer(errorWarningLog);
            streamAnalyzer.Run(shader);

            if (errorWarningLog.HasErrors)
                return;

            streamsUsages = streamAnalyzer.StreamsUsageByMethodDefinition;
            
            // Find entry points
            var vertexShaderMethod = FindEntryPoint("VSMain");
            var hullShaderMethod = FindEntryPoint("HSMain");
            var hullConstantShaderMethod = FindEntryPoint("HSConstantMain");
            var domainShaderMethod = FindEntryPoint("DSMain");
            var geometryShaderMethod = FindEntryPoint("GSMain");
            var pixelShaderMethod = FindEntryPoint("PSMain");
            var computeShaderMethod = FindEntryPoint("CSMain");
            
            if (vertexShaderMethod != null)
                vertexShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Vertex") } });
            if (pixelShaderMethod != null)
                pixelShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Pixel") } });
            if (geometryShaderMethod != null)
                geometryShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Geometry") } });
            if (hullShaderMethod != null)
                hullShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Hull") } });
            if (domainShaderMethod != null)
                domainShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Domain") } });
            if (computeShaderMethod != null)
                computeShaderMethod.Attributes.Add(new AttributeDeclaration { Name = new Identifier("EntryPoint"), Parameters = new List<Literal> { new Literal("Compute") } });

            if (!(hullShaderMethod == null && hullConstantShaderMethod == null && domainShaderMethod == null) && (hullShaderMethod == null || hullConstantShaderMethod == null || domainShaderMethod == null))
            {
                errorWarningLog.Error(ParadoxMessageCode.ErrorIncompleteTesselationShader, new SourceSpan());
                return;
            }
            
            StreamStageUsage streamStageUsageVS = vertexShaderMethod == null ? null : StreamAnalysisPerShader(vertexShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, vertexShaderMethod, PdxShaderStage.Vertex);
            StreamStageUsage streamStageUsageHS = hullShaderMethod == null ? null : StreamAnalysisPerShader(hullShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, hullShaderMethod, PdxShaderStage.Hull);
            StreamStageUsage streamStageUsageHSCS = hullConstantShaderMethod == null ? null : StreamAnalysisPerShader(hullConstantShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, hullConstantShaderMethod, PdxShaderStage.Constant);
            StreamStageUsage streamStageUsageDS = domainShaderMethod == null ? null : StreamAnalysisPerShader(domainShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, domainShaderMethod, PdxShaderStage.Domain);
            StreamStageUsage streamStageUsageGS = geometryShaderMethod == null ? null : StreamAnalysisPerShader(geometryShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, geometryShaderMethod, PdxShaderStage.Geometry);
            StreamStageUsage streamStageUsagePS = pixelShaderMethod == null ? null : StreamAnalysisPerShader(pixelShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, pixelShaderMethod, PdxShaderStage.Pixel);
            StreamStageUsage streamStageUsageCS = computeShaderMethod == null ? null : StreamAnalysisPerShader(computeShaderMethod.GetTag(ParadoxTags.ShaderScope) as ModuleMixin, computeShaderMethod, PdxShaderStage.Compute);
            
            // pathc some usage so that variables are correctly passed even if they are not explicitely used.
            if (streamStageUsageGS != null && streamStageUsageVS != null)
            {
                var needToAdd = true;
                foreach (var variable in streamStageUsageGS.OutStreamList.OfType<Variable>())
                {
                    var sem = variable.Qualifiers.OfType<Semantic>().FirstOrDefault();
                    if (sem != null && sem.Name.Text == "SV_Position")
                    {
                        needToAdd = false;
                        break;
                    }
                }

                if (needToAdd)
                {
                    // get the ShadingPosition variable
                    foreach (var variable in streamStageUsageVS.OutStreamList.OfType<Variable>())
                    {
                        var sem = variable.Qualifiers.OfType<Semantic>().FirstOrDefault();
                        if (sem != null && sem.Name.Text == "SV_Position")
                        {
                            streamStageUsageGS.OutStreamList.Add(variable);
                            break;
                        }

                    }
                }
                // TODO: it may have more variables like this one.
            }


            var shaderStreamsUsage = new List<StreamStageUsage>();
            
            // store these methods to prevent their renaming
            if (vertexShaderMethod != null)
            {
                entryPointMethods.Add(vertexShaderMethod);
                shaderStreamsUsage.Add(streamStageUsageVS);
            }
            if (hullShaderMethod != null)
            {
                entryPointMethods.Add(hullShaderMethod);
                shaderStreamsUsage.Add(streamStageUsageHS);
            }
            if (hullConstantShaderMethod != null)
            {
                entryPointMethods.Add(hullConstantShaderMethod);
                shaderStreamsUsage.Add(streamStageUsageHSCS);
            }
            if (domainShaderMethod != null)
            {
                entryPointMethods.Add(domainShaderMethod);
                shaderStreamsUsage.Add(streamStageUsageDS);
            }
            if (geometryShaderMethod != null)
            {
                entryPointMethods.Add(geometryShaderMethod);
                shaderStreamsUsage.Add(streamStageUsageGS);
            }
            if (pixelShaderMethod != null)
            {
                entryPointMethods.Add(pixelShaderMethod);
                shaderStreamsUsage.Add(streamStageUsagePS);
            }
            if (computeShaderMethod != null)
            {
                entryPointMethods.Add(computeShaderMethod);
            }

            BubbleUpStreamUsages(shaderStreamsUsage);
            
            if (computeShaderMethod != null)
                ComputeShaderStreamAnalysis(streamStageUsageCS);
            
            StructType outputStructure = null;

            // remove the now useless tags and typeinferences to accelerate cloning
            var tagCleaner = new ParadoxTagCleaner();
            tagCleaner.Run(shader);
            
            outputStructure = GenerateStreams(vertexShaderMethod, streamStageUsageVS, "VS", outputStructure);
            outputStructure = GenerateStreamsForHullShader(hullShaderMethod, hullConstantShaderMethod, streamStageUsageHS, "HS", outputStructure);
            outputStructure = GenerateStreamsForDomainShader(domainShaderMethod, streamStageUsageDS, "DS", outputStructure);
            outputStructure = GenerateStreamsWithSpecialDataInput(geometryShaderMethod, streamStageUsageGS, "GS", outputStructure);
            outputStructure = GenerateStreams(pixelShaderMethod, streamStageUsagePS, "PS", outputStructure, false);

            outputStructure = GenerateStreams(computeShaderMethod, streamStageUsageCS, "CS", null);
            
            RemoveUselessAndSortMethods();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sort the methods based on their calls
        /// </summary>
        private void RemoveUselessAndSortMethods()
        {
            var methods = shader.Members.OfType<MethodDeclaration>().ToList();
            shader.Members.RemoveAll(methods.Contains);
            
            var zeroCalledMethods = new HashSet<MethodDeclaration>();
            var methodReferenceCounter = methods.ToDictionary(x => x, x => 0);

            foreach (var reference in streamsUsages)
            {
                foreach (var usage in reference.Value.Where(x => x.CallType == StreamCallType.Method))
                {
                    int value;
                    if (methodReferenceCounter.TryGetValue(usage.MethodDeclaration, out value))
                        methodReferenceCounter[usage.MethodDeclaration] = value + 1;
                    else if (!entryPointMethods.Contains(usage.MethodDeclaration))
                        zeroCalledMethods.Add(usage.MethodDeclaration);
                }
            }
            
            
            zeroCalledMethods.UnionWith(methodReferenceCounter.Where(x => x.Value == 0 && !entryPointMethods.Contains(x.Key)).Select(x => x.Key));
            BuildOrderedMethodUsageList(zeroCalledMethods, methodReferenceCounter);

            var finalMethodsList = BuildOrderedMethodUsageList(new HashSet<MethodDeclaration>(entryPointMethods), methodReferenceCounter);
            finalMethodsList.Reverse();
            shader.Members.AddRange(finalMethodsList);
        }

        /// <summary>
        /// Recursively create a list of all the methods that are exclusively used from the start ones
        /// </summary>
        /// <param name="startList">the list of starting methods</param>
        /// <param name="methodReferenceCounter">all the methods</param>
        /// <returns></returns>
        private List<MethodDeclaration> BuildOrderedMethodUsageList(HashSet<MethodDeclaration> startList, Dictionary<MethodDeclaration, int> methodReferenceCounter)
        {
            var finalMethodsList = new List<MethodDeclaration>();
            while (startList.Count > 0)
            {
                var newZeroCalledMethods = new List<MethodDeclaration>();

                foreach (var method in startList)
                {
                    finalMethodsList.Add(method);

                    List<StreamUsageInfo> reference;
                    if (streamsUsages.TryGetValue(method, out reference))
                    {
                        foreach (var usage in reference.Where(x => x.CallType == StreamCallType.Method))
                        {
                            int value;
                            if (methodReferenceCounter.TryGetValue(usage.MethodDeclaration, out value))
                            {
                                methodReferenceCounter[usage.MethodDeclaration] = value - 1;
                                if (value == 1)
                                    newZeroCalledMethods.Add(usage.MethodDeclaration);
                            }
                        }
                    }
                }

                startList.Clear();
                startList.UnionWith(newZeroCalledMethods);
            }

            return finalMethodsList;
        }

        /// <summary>
        /// Finds all the function with the name
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <returns>a collection of all the functions with that name, correctly ordered</returns>
        private MethodDefinition FindEntryPoint(string name)
        {
            for (int i = mixinInheritance.Count - 1; i >= 0; --i)
            {
                var mixin = mixinInheritance[i];
                var count = 0;
                for (int j = 0; j < i; ++j)
                {
                    count += mixin.MixinName == mixinInheritance[j].MixinName ? 1 : 0;
                }

                var method = mixin.LocalVirtualTable.Methods.FirstOrDefault(x => x.Method.Name.Text == name && x.Method is MethodDefinition);
                if (method != null && (count == 0 || method.Method.Qualifiers.Contains(ParadoxStorageQualifier.Clone)))
                    return method.Method as MethodDefinition;
            }
            return null;
        }

        /// <summary>
        /// Get the streams usage for this entrypoint
        /// </summary>
        /// <param name="moduleMixin">the current module mixin</param>
        /// <param name="entryPoint">the entrypoint method</param>
        /// <returns>a StreamStageUsage containing the streams usages</returns>
        private StreamStageUsage StreamAnalysisPerShader(ModuleMixin moduleMixin, MethodDeclaration entryPoint, PdxShaderStage shaderStage)
        {
            var visitedMethods = new List<MethodDeclaration>();
            var streamStageUsage = new StreamStageUsage { ShaderStage = shaderStage };
            FindStreamsUsage(entryPoint, streamStageUsage.InStreamList, streamStageUsage.OutStreamList, visitedMethods);
            visitedMethods.Clear();

            return streamStageUsage;
        }

        /// <summary>
        /// Finds the usage of the streams
        /// </summary>
        /// <param name="currentMethod">the current method</param>
        /// <param name="inStreamList">list of in-streams</param>
        /// <param name="outStreamList">list of out-streams</param>
        /// <param name="visitedMethods">list of already visited methods</param>
        private void FindStreamsUsage(MethodDeclaration currentMethod, List<IDeclaration> inStreamList, List<IDeclaration> outStreamList, List<MethodDeclaration> visitedMethods)
        {
            if (visitedMethods.Contains(currentMethod))
            {
                errorWarningLog.Error(ParadoxMessageCode.ErrorRecursiveCall, currentMethod.Span, currentMethod);
                return;
            }

            if (currentMethod != null)
            {
                var newListVisitedMethods = new List<MethodDeclaration>();
                newListVisitedMethods.AddRange(visitedMethods);
                newListVisitedMethods.Add(currentMethod);

                List<StreamUsageInfo> streamUsageList;
                if (streamsUsages.TryGetValue(currentMethod, out streamUsageList))
                {
                    // look for stream usage inside the function
                    foreach (var streamUsage in streamUsageList)
                    {
                        if (streamUsage.CallType == StreamCallType.Member)
                        {
                            var isOutStream = outStreamList.Contains(streamUsage.Variable);
                            var isInStream = inStreamList.Contains(streamUsage.Variable);

                            if (streamUsage.Usage == StreamUsage.Write && !isOutStream)
                                outStreamList.Add(streamUsage.Variable);
                            else if (streamUsage.Usage == StreamUsage.Read && !isOutStream && !isInStream) // first read
                                inStreamList.Add(streamUsage.Variable);
                        }
                        else if (streamUsage.CallType == StreamCallType.Method)
                        {
                            if (streamUsage.MethodDeclaration != null) // way to check the built-in functions (could be improved?)
                                FindStreamsUsage(streamUsage.MethodDeclaration, inStreamList, outStreamList, newListVisitedMethods);
                        }
                        else if (streamUsage.CallType != StreamCallType.Direct) // should not happen
                            errorWarningLog.Error(ParadoxMessageCode.ErrorStreamUsageInitialization, streamUsage.Expression.Span, streamUsage.Expression);
                    }
                }
            }
        }

        /// <summary>
        /// Pass the stream usage accros the stages.
        /// </summary>
        /// <param name="streamStageUsages">the ordered stream usage list</param>
        private void BubbleUpStreamUsages(List<StreamStageUsage> streamStageUsages)
        {
            for (int i = streamStageUsages.Count - 1; i > 0; --i)
            {
                var nextStreamUsage = streamStageUsages[i];
                var prevStreamUsage = streamStageUsages[i - 1];

                // pixel output is only SV_Targetx and SV_Depth, everything else intermediate variable
                if (nextStreamUsage.ShaderStage == PdxShaderStage.Pixel)
                {
                    var semVar = new List<IDeclaration>();
                    var nonSemVar = new List<IDeclaration>();
                    foreach (var variable in nextStreamUsage.OutStreamList)
                    {
                        var sem = (variable as Variable).Qualifiers.OfType<Semantic>().FirstOrDefault();
                        if (sem != null && (sem.Name.Text.StartsWith("SV_Target") || sem.Name.Text == "SV_Depth"))
                            semVar.Add(variable);
                        else
                            nonSemVar.Add(variable);
                    }
                    nextStreamUsage.InterStreamList.AddRange(nonSemVar);
                    nextStreamUsage.OutStreamList = semVar;
                }

                // NOTE: from this point, nextStreamUsage.OutStreamList is correct.

                // add necessary variables to output and input of previous stage
                foreach (var variable in nextStreamUsage.InStreamList)
                {
                    if (!prevStreamUsage.OutStreamList.Contains(variable))
                    {
                        prevStreamUsage.OutStreamList.Add(variable);

                        if (!prevStreamUsage.InStreamList.Contains(variable))
                            prevStreamUsage.InStreamList.Add(variable);
                    }
                }

                // keep variable from prev output only if they are necessary to next stage OR their semantics force them to be in it
                var toKeep = new List<IDeclaration>();
                foreach (var variable in prevStreamUsage.OutStreamList)
                {
                    var sem = (variable as Variable).Qualifiers.OfType<Semantic>().FirstOrDefault();
                    if (nextStreamUsage.InStreamList.Contains(variable)
                        || ((nextStreamUsage.ShaderStage == PdxShaderStage.Pixel || nextStreamUsage.ShaderStage == PdxShaderStage.Geometry) && sem != null && sem.Name.Text == "SV_Position"))
                        toKeep.Add(variable);
                    else if (nextStreamUsage.ShaderStage == PdxShaderStage.Pixel && prevStreamUsage.ShaderStage == PdxShaderStage.Geometry && sem != null && sem.Name.Text == "SV_RenderTargetArrayIndex")
                    {
                        toKeep.Add(variable);
                        nextStreamUsage.InStreamList.Add(variable);
                    }
                    else
                        prevStreamUsage.InterStreamList.Add(variable);
                }

                prevStreamUsage.OutStreamList.Clear();
                prevStreamUsage.OutStreamList.AddRange(toKeep);
            }
        }

        /// <summary>
        /// Organize the streams for the compute shader
        /// </summary>
        /// <param name="streamStageUsage">the StreamStageUsage of the compute stage</param>
        private void ComputeShaderStreamAnalysis(StreamStageUsage streamStageUsage)
        {
            if (streamStageUsage.ShaderStage == PdxShaderStage.Compute)
            {
                streamStageUsage.InterStreamList.AddRange(streamStageUsage.OutStreamList);
                streamStageUsage.OutStreamList.Clear();
            }
        }

        /// <summary>
        /// Generates a stream structure and add them to the Ast
        /// </summary>
        /// <param name="entryPoint">the entrypoint function</param>
        /// <param name="streamStageUsage">the stream usage in this stage</param>
        /// <param name="stageName">the name of the stage</param>
        /// <param name="prevOuputStructure">the output structutre from the previous stage</param>
        /// <returns>the new output structure</returns>
        private StructType GenerateStreams(MethodDefinition entryPoint, StreamStageUsage streamStageUsage, string stageName, StructType prevOuputStructure, bool autoGenSem = true)
        {
            if (entryPoint != null)
            {
                // create the stream structures
                var inStreamStruct = prevOuputStructure ?? CreateStreamStructure(streamStageUsage.InStreamList, stageName + "_INPUT");
                var outStreamStruct = CreateStreamStructure(streamStageUsage.OutStreamList, stageName + "_OUTPUT", true, autoGenSem);

                var intermediateStreamStruct = CreateIntermediateStructType(streamStageUsage, stageName);

                // modify the entrypoint
                if (inStreamStruct.Fields.Count != 0)
                {
                    entryPoint.Parameters.Add(new Parameter(new TypeName(inStreamStruct.Name), "__input__"));
                    entryPoint.Parameters[0].Qualifiers.Values.Remove(ParameterQualifier.InOut);
                }

                // add the declaration statements to the entrypoint and fill with the values
                entryPoint.Body.InsertRange(0, CreateStreamFromInput(intermediateStreamStruct, "streams", inStreamStruct, new VariableReferenceExpression("__input__")));
                if (outStreamStruct.Fields.Count != 0)
                {
                    entryPoint.Body.AddRange(CreateOutputFromStream(outStreamStruct, "__output__", intermediateStreamStruct, "streams"));
                    entryPoint.Body.Add(new ReturnStatement { Value = new VariableReferenceExpression("__output__") });
                    entryPoint.ReturnType = new TypeName(outStreamStruct.Name);
                }

                // explore all the called functions
                var visitedMethods = new HashSet<MethodDeclaration>();
                var methodsWithStreams = new List<MethodDeclaration>();
                PropagateStreamsParameter(entryPoint, inStreamStruct, intermediateStreamStruct, outStreamStruct, visitedMethods, methodsWithStreams);
                
                CheckCrossStageMethodCall(streamStageUsage.ShaderStage, methodsWithStreams);

                if (prevOuputStructure == null)
                    shader.Members.Insert(0, inStreamStruct);
                if (outStreamStruct.Fields.Count != 0)
                    shader.Members.Insert(0, outStreamStruct);
                shader.Members.Insert(0, intermediateStreamStruct);

                return outStreamStruct;
            }

            return prevOuputStructure;
        }

        /// <summary>
        /// Generates a stream structure and add them to the Ast - for the geometry shader
        /// </summary>
        /// <param name="entryPoint">the entrypoint function</param>
        /// <param name="streamStageUsage">the stream usage in this stage</param>
        /// <param name="stageName">the name of the stage</param>
        /// <param name="prevOuputStructure">the output structutre from the previous stage</param>
        /// <returns>the new output structure</returns>
        private StructType GenerateStreamsWithSpecialDataInput(MethodDefinition entryPoint, StreamStageUsage streamStageUsage, string stageName, StructType prevOuputStructure)
        {
            if (entryPoint != null)
            {
                var inStreamStruct = prevOuputStructure ?? CreateStreamStructure(streamStageUsage.InStreamList, stageName + "_INPUT");
                var outStreamStruct = CreateStreamStructure(streamStageUsage.OutStreamList, stageName + "_OUTPUT");

                var mixin = entryPoint.GetTag(ParadoxTags.ShaderScope) as ModuleMixin;

                var intermediateStreamStruct = CreateIntermediateStructType(streamStageUsage, stageName);

                // put the streams declaration at the beginning of the method body
                var streamsDeclaration = new DeclarationStatement(new Variable(new TypeName(intermediateStreamStruct.Name), "streams") { InitialValue = new CastExpression { From = new LiteralExpression(0), Target = new TypeName(intermediateStreamStruct.Name) } });
                entryPoint.Body.Insert(0, streamsDeclaration);

                // add the declaration statements to the entrypoint and fill with the values
                var outputStatements = CreateOutputFromStream(outStreamStruct, "output", intermediateStreamStruct, "streams").ToList();
                var outputVre = new VariableReferenceExpression(((outputStatements.First() as DeclarationStatement).Content as Variable).Name);

                var replacor = new ParadoxReplaceAppend(streamAnalyzer.AppendMethodCalls, outputStatements, outputVre);
                ReplaceAppendMethod(entryPoint, replacor);
                
                var visitedMethods = new Stack<MethodDeclaration>();
                var inStructType = new TypeName(inStreamStruct.Name);
                var outStructType = new TypeName(outStreamStruct.Name);
                RecursiveRename(entryPoint, inStructType, null, outStructType, null, visitedMethods);

                // explore all the called functions
                var streamsVisitedMethods = new HashSet<MethodDeclaration>();
                var methodsWithStreams = new List<MethodDeclaration>();
                PropagateStreamsParameter(entryPoint, inStreamStruct, intermediateStreamStruct, outStreamStruct, streamsVisitedMethods, methodsWithStreams);

                CheckCrossStageMethodCall(streamStageUsage.ShaderStage, methodsWithStreams);

                if (prevOuputStructure == null)
                    shader.Members.Insert(0, inStreamStruct);
                shader.Members.Insert(0, outStreamStruct);
                shader.Members.Insert(0, intermediateStreamStruct);

                return outStreamStruct;
            }

            return prevOuputStructure;
        }

        /// <summary>
        /// Replace the append methods
        /// </summary>
        /// <param name="entryPoint">the entrypoint method</param>
        /// <param name="replacor">the visitor</param>
        private void ReplaceAppendMethod(MethodDefinition entryPoint, ParadoxReplaceAppend replacor)
        {
            replacor.Run(entryPoint);

            List<StreamUsageInfo> nextMethods;
            if (streamsUsages.TryGetValue(entryPoint, out nextMethods))
                nextMethods.Where(x => x.CallType == StreamCallType.Method).Select(x => x.MethodDeclaration as MethodDefinition).Where(x => x != null).ToList().ForEach(x => ReplaceAppendMethod(x, replacor));
        }



        /// <summary>
        /// Generates a stream structure and add them to the Ast - for the hull shader and hull shader constant
        /// </summary>
        /// <param name="entryPoint">the entrypoint function</param>
        /// <param name="entryPointHSConstant">entrypoint for the hull shader constant</param>
        /// <param name="streamStageUsage">the stream usage in this stage</param>
        /// <param name="stageName">the name of the stage</param>
        /// <param name="prevOuputStructure">the output structutre from the previous stage</param>
        /// <returns>the new output structure</returns>
        private StructType GenerateStreamsForHullShader(MethodDefinition entryPoint, MethodDefinition entryPointHSConstant, StreamStageUsage streamStageUsage, string stageName, StructType prevOuputStructure)
        {
            if (entryPoint != null)
            {
                // same behavior as geometry shader
                var outStreamStruct = GenerateStreamsWithSpecialDataInput(entryPoint, streamStageUsage, stageName, prevOuputStructure);
                var inStreamStruct = prevOuputStructure ?? shader.Members.OfType<StructType>().FirstOrDefault(x => x.Name.Text == stageName + "_INPUT");
                var intermediateStreamStruct = shader.Members.OfType<StructType>().FirstOrDefault(x => x.Name.Text == stageName + "_STREAMS");

                if (inStreamStruct == null)
                    throw new Exception("inStreamStruct cannot be null");

                var inStructType = new TypeName(inStreamStruct.Name);
                var outStructType = new TypeName(outStreamStruct.Name);

                // get the Output parameter, its name and remove it
                var outputName = "output";
                var outputParam = entryPoint.Parameters.FirstOrDefault(x => x.Type.Name.Text == outStreamStruct.Name.Text);
                if (outputParam != null)
                {
                    outputName = outputParam.Name.Text; // get the name of the parameter
                    entryPoint.Parameters.Remove(outputParam); // remove the parameter
                }

                entryPoint.Body.Add(new ReturnStatement { Value = new VariableReferenceExpression(outputName) });
                entryPoint.Body.Insert(0, CreateStructInit(outStreamStruct, outputName));
                entryPoint.ReturnType = outStructType;

                if (entryPointHSConstant != null)
                    GenerateStreamsForHullShaderConstant(entryPointHSConstant, inStructType, outStructType);

                return outStreamStruct;
            }

            return prevOuputStructure;
        }

        /// <summary>
        /// Modify the Hull shader constant
        /// </summary>
        /// <param name="entryPoint">the entrypoint method</param>
        /// <param name="inStreamStructTypeName">the input structure of the Hull shader</param>
        /// <param name="outStreamStructTypeName">the output structure of the Hull shader</param>
        private void GenerateStreamsForHullShaderConstant(MethodDefinition entryPoint, TypeName inStreamStructTypeName, TypeName outStreamStructTypeName)
        {
            if (entryPoint != null)
            {
                var constStreamStruct = CreateStreamStructure(mainModuleMixin.VirtualTable.Variables.Select(x => x.Variable).Where(x => x.Qualifiers.Contains(ParadoxStorageQualifier.PatchStream)).Distinct().ToList<IDeclaration>(), "HS_CONSTANTS");
                var typeConst = new TypeName(constStreamStruct.Name);

                var visitedMethods = new Stack<MethodDeclaration>();
                RecursiveRename(entryPoint, inStreamStructTypeName, outStreamStructTypeName, outStreamStructTypeName, typeConst, visitedMethods);

                // get the Constants parameter, its name and remove it
                var constParamName = "constants";
                var constParam = entryPoint.Parameters.FirstOrDefault(x => x.Type.Name.Text == constStreamStruct.Name.Text);
                if (constParam != null)
                {
                    constParamName = constParam.Name.Text;
                    entryPoint.Parameters.Remove(constParam); // remove the parameter
                }

                var constDecl = new DeclarationStatement(
                    new Variable(typeConst, constParamName)
                        {
                            InitialValue = new CastExpression { From = new LiteralExpression(0), Target = new TypeName(constStreamStruct.Name) }
                        });

                entryPoint.Body.Insert(0, constDecl); // insert structure instance declaration
                entryPoint.Body.Add(new ReturnStatement(new VariableReferenceExpression(constParamName))); // add a return statement

                entryPoint.ReturnType = typeConst; // change the return type

                shader.Members.Insert(0, constStreamStruct);
            }
        }

        /// <summary>
        /// Generates a stream structure and add them to the Ast - for the domain shader
        /// </summary>
        /// <param name="entryPoint">the entrypoint function</param>
        /// <param name="streamStageUsage">the stream usage in this stage</param>
        /// <param name="stageName">the name of the stage</param>
        /// <param name="prevOuputStructure">the output structutre from the previous stage</param>
        /// <returns>the new output structure</returns>
        private StructType GenerateStreamsForDomainShader(MethodDefinition entryPoint, StreamStageUsage streamStageUsage, string stageName, StructType prevOuputStructure)
        {
            if (entryPoint != null)
            {
                var outStreamStruct = GenerateStreamsForHullShader(entryPoint, null, streamStageUsage, stageName, prevOuputStructure);

                var visitedMethods = new Stack<MethodDeclaration>();
                RecursiveRename(entryPoint, null, null, null, new TypeName("HS_CONSTANTS"), visitedMethods);

                return outStreamStruct;
            }

            return prevOuputStructure;
        }

        /// <summary>
        /// Checks if a function needs to have a stream strucutre added in its declaration
        /// </summary>
        /// <param name="methodDefinition">the method definition</param>
        /// <param name="inputStream">The stage input structure stream.</param>
        /// <param name="intermediateStream">the stream structure</param>
        /// <param name="outputStream">The stage output stream structure.</param>
        /// <param name="visitedMethods">the list of already visited methods</param>
        /// <param name="methodsWithStreams">The list of methods that have a streams argument.</param>
        /// <returns>true if needed, false otherwise</returns>
        private bool PropagateStreamsParameter(MethodDefinition methodDefinition, StructType inputStream, StructType intermediateStream, StructType outputStream, HashSet<MethodDeclaration> visitedMethods, List<MethodDeclaration> methodsWithStreams)
        {
            var needStream = false;

            if (methodDefinition != null)
            {
                if (visitedMethods.Contains(methodDefinition))
                    return methodDefinition.Parameters.Count > 0 && methodDefinition.Parameters[0].Type == intermediateStream;

                List<StreamUsageInfo> streamUsageInfos;
                if (streamsUsages.TryGetValue(methodDefinition, out streamUsageInfos))
                {
                    needStream = streamUsageInfos.Any(x => x.CallType == StreamCallType.Member || x.CallType == StreamCallType.Direct);
                    visitedMethods.Add(methodDefinition);

                    List<MethodDeclaration> calls;
                    if (TryGetMethodCalls(methodDefinition, out calls))
                        needStream = calls.Aggregate(needStream, (res, calledmethod) => res | PropagateStreamsParameter(calledmethod as MethodDefinition, inputStream, intermediateStream, outputStream, visitedMethods, methodsWithStreams));

                    if (needStream && !entryPointMethods.Contains(methodDefinition))
                    {
                        var param = new Parameter(new TypeName(intermediateStream.Name), "streams");

                        foreach (var methodRef in mainModuleMixin.ClassReferences.MethodsReferences[methodDefinition])
                        {
                            var vre = new VariableReferenceExpression(param.Name) { TypeInference = { Declaration = param, TargetType = param.Type } };
                            methodRef.Arguments.Insert(0, vre);
                        }

                        param.Qualifiers |= ParameterQualifier.InOut;
                        methodDefinition.Parameters.Insert(0, param);

                        methodsWithStreams.Add(methodDefinition);
                    }
                }

                TransformStreamsAssignments(methodDefinition, inputStream, intermediateStream, outputStream);
            }
            return needStream;
        }


        /// <summary>
        /// Transform stream assignments with correct input/ouput structures
        /// </summary>
        /// <param name="methodDefinition">the current method</param>
        /// <param name="inputStreamStruct">the input structure of the stage</param>
        /// <param name="intermediateStreamStruct">the intermediate structure of the stage</param>
        /// <param name="outputStreamStruct">the output structure of the stage</param>
        private void TransformStreamsAssignments(MethodDefinition methodDefinition, StructType inputStreamStruct, StructType intermediateStreamStruct, StructType outputStreamStruct)
        {
            // replace stream assignement with field values assignements
            foreach (var assignment in streamAnalyzer.AssignationsToStream)
            {
                StatementList parent;
                var index = SearchExpressionStatement(methodDefinition.Body, assignment, out parent);
                if (index < 0 || parent == null)
                    continue;

                // TODO: check that it is "output = streams"
                var statementList = CreateOutputFromStream(outputStreamStruct, (assignment.Target as VariableReferenceExpression).Name.Text, intermediateStreamStruct, "streams").ToList();
                statementList.RemoveAt(0); // do not keep the variable declaration
                methodDefinition.Body.RemoveAt(index);
                methodDefinition.Body.InsertRange(index, statementList);
            }

            // replace stream assignement with field values assignements
            foreach (var assignment in streamAnalyzer.StreamAssignations)
            {
                StatementList parent;
                var index = SearchExpressionStatement(methodDefinition.Body, assignment, out parent);
                if (index < 0 || parent == null)
                    continue;

                var statementList = CreateStreamFromInput(intermediateStreamStruct, "streams", inputStreamStruct, assignment.Value, false).ToList();
                statementList.RemoveAt(0); // do not keep the variable declaration
                parent.RemoveAt(index);
                parent.InsertRange(index, statementList);
            }
        }

        /// <summary>
        /// Search a statement in method.
        /// </summary>
        /// <param name="topStatement">The statement to look into</param>
        /// <param name="expression">The expression to look for.</param>
        /// <param name="parentStatement">The statement list where the expression was found.</param>
        /// <returns>The index of the statement in the statement list.</returns>
        private int SearchExpressionStatement(Statement topStatement, Expression expression, out StatementList parentStatement)
        {
            // handle special case because BlockStatement.Children returns children of child (Statements members)
            if (topStatement is BlockStatement)
                topStatement = ((BlockStatement)topStatement).Statements;
            if (topStatement is StatementList)
            {
                var statementList = (StatementList)topStatement;
                var index = statementList.IndexOf(statementList.OfType<ExpressionStatement>().FirstOrDefault(x => x.Expression == expression));
                if (index >= 0)
                {
                    parentStatement = (StatementList)topStatement;
                    return index;
                }

            }

            foreach (var statement in topStatement.Childrens().OfType<Statement>())
            {
                var index = SearchExpressionStatement(statement, expression, out parentStatement);
                if (index >= 0)
                    return index;
            }
            
            parentStatement = null;
            return -1;
        }

        /// <summary>
        /// Recursively rename the input/output types
        /// </summary>
        /// <param name="methodDeclaration">the method to explore</param>
        /// <param name="inputName">the TypeName for Input</param>
        /// <param name="input2Name">the TypeName for Input2</param>
        /// <param name="outputName">the TypeName for Output</param>
        /// <param name="constantsName">the TypeName for Constants</param>
        /// <param name="visitedMethods">the already visited methods</param>
        private void RecursiveRename(MethodDeclaration methodDeclaration, TypeName inputName, TypeName input2Name, TypeName outputName, TypeName constantsName, Stack<MethodDeclaration> visitedMethods)
        {
            if (methodDeclaration == null || visitedMethods.Contains(methodDeclaration))
                return;

            RenameInputOutput(methodDeclaration, inputName, input2Name, outputName, constantsName);
            visitedMethods.Push(methodDeclaration);

            List<MethodDeclaration> calls;
            if (TryGetMethodCalls(methodDeclaration, out calls))
            {
                foreach (var calledmethod in calls)
                    RecursiveRename(calledmethod, inputName, input2Name, outputName, constantsName, visitedMethods);
            }
        }

        /// <summary>
        /// Get all the calls from the current method
        /// </summary>
        /// <param name="currentMethod">the current method</param>
        /// <param name="calledMethods">list of method called</param>
        /// <returns>true if calls were found</returns>
        //private bool TryGetMethodCalls(MethodDeclaration currentMethod, out List<MethodInvocationExpression> calledMethods)
        private bool TryGetMethodCalls(MethodDeclaration currentMethod, out List<MethodDeclaration> calledMethods)
        {
            List<StreamUsageInfo> streamUsageInfos;
            if (streamsUsages.TryGetValue(currentMethod, out streamUsageInfos))
            {
                //calledMethods = streamUsageInfos.Where(x => x.CallType == StreamCallType.Method).Select(x => x.MethodReference).ToList();
                calledMethods = streamUsageInfos.Where(x => x.CallType == StreamCallType.Method).Select(x => x.MethodDeclaration).ToList();
                return true;
            }

            calledMethods = null;
            return false;
        }

        /// <summary>
        /// rename the input/ouput of a method
        /// </summary>
        /// <param name="methodDeclaration">the method</param>
        /// <param name="inputName">the type replacement for Input</param>
        /// <param name="input2Name">the type replacement for Input2</param>
        /// <param name="outputName">the type replacement for Output</param>
        /// <param name="constantsName">the type replacement for Constants</param>
        private void RenameInputOutput(MethodDeclaration methodDeclaration, TypeName inputName, TypeName input2Name, TypeName outputName, TypeName constantsName)
        {
            if (inputName != null)
            {
                var replacor = new ParadoxReplaceVisitor(StreamsType.Input, inputName);
                replacor.Run(methodDeclaration);
            }
            if (input2Name != null)
            {
                var replacor = new ParadoxReplaceVisitor(StreamsType.Input2, input2Name);
                replacor.Run(methodDeclaration);
            }
            if (outputName != null)
            {
                var replacor = new ParadoxReplaceVisitor(StreamsType.Output, outputName);
                replacor.Run(methodDeclaration);
            }
            if (constantsName != null)
            {
                var replacor = new ParadoxReplaceVisitor(StreamsType.Constants, constantsName);
                replacor.Run(methodDeclaration);
            }
        }

        /// <summary>
        /// Check that methods with streams are not called across several stages.
        /// </summary>
        /// <param name="shaderStage">The current shader stage to check.</param>
        /// <param name="methodsWithStreams">The list of methods that need streams in that stage.</param>
        private void CheckCrossStageMethodCall(PdxShaderStage shaderStage, List<MethodDeclaration> methodsWithStreams)
        {
            foreach (var stageList in methodsPerShaderStage)
            {
                var stage = stageList.Key;
                if (stage != shaderStage) // should always be true
                {
                    foreach (var method in methodsWithStreams)
                    {
                        if (stageList.Value.Contains(method))
                        {
                            errorWarningLog.Error(ParadoxMessageCode.ErrorCrossStageMethodCall, method.Span, method, stage, shaderStage);
                        }
                    }
                }
            }
            methodsPerShaderStage.Add(shaderStage, methodsWithStreams);
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Creates assignement statements with its default value
        /// </summary>
        /// <param name="streamStruct">the stream structure</param>
        /// <param name="streamName">the name of the stream</param>
        /// <param name="inputStruct">the input structure</param>
        /// <param name="initialValue">the initial value</param>
        /// <param name="scopeStack">???</param>
        /// <returns>A collection of statements</returns>
        private static IEnumerable<Statement> AssignStreamFromInput(StructType streamStruct, string streamName, StructType inputStruct, Expression initialValue, bool basicTransformation)
        {
            foreach (var currentField in inputStruct.Fields)
            {
                // Ignore fields that don't exist in Streams.
                // It could happen if HSConstantMain references a stream (gets added to HS_OUTPUT),
                // and in HSMain CreateStreamFromInput() is called (this stream doesn't exist in DS_STREAMS).
                if (streamStruct.Fields.All(x => x.Name != currentField.Name))
                    continue;

                // If we have a scope stack (advanced analysis), then convert expression by appending
                // field to each reference to a variable of inputStruct type
                // i.e. "output = input1 * 3 + input2 * 5" will become "output.A = input1.A * 3 + input2.A * 5"
                // Otherwise consider it is as a simple a variable reference and directly append field.
                if (basicTransformation)
                {
                    yield return new ExpressionStatement(
                        new AssignmentExpression(
                            AssignmentOperator.Default,
                            new MemberReferenceExpression(new VariableReferenceExpression(streamName), currentField.Name),
                            new MemberReferenceExpression(initialValue, currentField.Name)));
                }
                else
                {
                    //yield return AssignStreamFieldFromInput(streamName, inputStruct, initialValue, scopeStack, currentField);
                    foreach (var field in streamStruct.Fields.Where(x => x.Name == currentField.Name)) // TODO: where might be useless
                    {
                        if (field.Type is ArrayType)
                        {
                            //create a for loop

                            var iteratorName = field.Name.Text + "_Iter";
                            var iterator = new Variable(ScalarType.Int, iteratorName, new LiteralExpression(0));
                            var start = new DeclarationStatement(iterator);
                            var condition = new BinaryExpression(BinaryOperator.Less, new VariableReferenceExpression(iterator), (field.Type as ArrayType).Dimensions[0]);
                            var next = new UnaryExpression(UnaryOperator.PreIncrement, new VariableReferenceExpression(iterator));
                            var forLoop = new ForStatement(start, condition, next);

                            var fieldAssigner = new StreamFieldVisitor(field, new VariableReferenceExpression(iterator));
                            var clonedExpression = fieldAssigner.Run(ParadoxAssignmentCloner.Run(initialValue));
                            
                            forLoop.Body = new ExpressionStatement(
                                new AssignmentExpression(
                                    AssignmentOperator.Default,
                                    new IndexerExpression(new MemberReferenceExpression(new VariableReferenceExpression(streamName), currentField.Name), new VariableReferenceExpression(iterator)),
                                    clonedExpression));

                            yield return forLoop;
                        }
                        else
                        {
                            var fieldAssigner = new StreamFieldVisitor(field);
                            //var clonedExpression = fieldAssigner.Run(initialValue.DeepClone());
                            var clonedExpression = fieldAssigner.Run(ParadoxAssignmentCloner.Run(initialValue));
                            
                            yield return new ExpressionStatement(
                                new AssignmentExpression(
                                    AssignmentOperator.Default,
                                    new MemberReferenceExpression(new VariableReferenceExpression(streamName), currentField.Name),
                                    clonedExpression));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates assignement statements with its default value
        /// </summary>
        /// <param name="outputStruct">the output structure</param>
        /// <param name="outputName">the name of the output stream</param>
        /// <param name="streamStruct">the stream structure</param>
        /// <param name="streamName">the name of the stream</param>
        /// <returns>a collection of statements</returns>
        private static IEnumerable<Statement> AssignOutputFromStream(StructType outputStruct, string outputName, StructType streamStruct, string streamName)
        {
            foreach (var currentField in outputStruct.Fields)
            {
                yield return new ExpressionStatement(
                    new AssignmentExpression(
                        AssignmentOperator.Default,
                        new MemberReferenceExpression(new VariableReferenceExpression(outputName), currentField.Name),
                        new MemberReferenceExpression(new VariableReferenceExpression(streamName), currentField.Name)));
            }
        }

        /// <summary>
        /// Creates a stream structure and assign its default values
        /// </summary>
        /// <param name="streamStruct">the structure</param>
        /// <param name="streamName">the name of the stream</param>
        /// <param name="inputStruct">the inputStructure</param>
        /// <param name="initialValue">the initial value of the struture</param>
        /// <param name="scopeStack">???</param>
        /// <returns>a collection of statements to insert in the body of a method</returns>
        private static IEnumerable<Statement> CreateStreamFromInput(StructType streamStruct, string streamName, StructType inputStruct, Expression initialValue, bool basicTransformation = true)
        {
            yield return CreateStructInit(streamStruct, streamName);

            foreach (var statement in AssignStreamFromInput(streamStruct, streamName, inputStruct, initialValue, basicTransformation))
            {
                yield return statement;
            }
        }

        /// <summary>
        /// Creates an output stream structure and assign its default values
        /// </summary>
        /// <param name="outputStruct">the structuer</param>
        /// <param name="outputName">the name of the structure</param>
        /// <param name="streamStruct">>the initial value of the struture</param>
        /// <param name="streamName">the name of the stream</param>
        /// <returns>a collection of statements to insert in the body of a method</returns>
        private static IEnumerable<Statement> CreateOutputFromStream(StructType outputStruct, string outputName, StructType streamStruct, string streamName)
        {
            yield return CreateStructInit(outputStruct, outputName);

            foreach (var statement in AssignOutputFromStream(outputStruct, outputName, streamStruct, streamName))
            {
                yield return statement;
            }
        }

        /// <summary>
        /// Generate a stream structure
        /// </summary>
        /// <param name="streamsDeclarationList">the list of the declarations</param>
        /// <param name="structName">the name of the structure</param>
        /// <returns>the structure</returns>
        private static StructType CreateStreamStructure(List<IDeclaration> streamsDeclarationList, string structName, bool useSem = true, bool addAutoSem = true)
        {
            var tempStruct = new StructType { Name = new Identifier(structName) };
            foreach (var streamDecl in streamsDeclarationList)
            {
                var streamVar = streamDecl as Variable;
                if (streamVar != null)
                {
                    var variable = new Variable(streamVar.Type, streamVar.Name) { Span = streamVar.Span };
                    if (useSem)
                        foreach (var qualifier in streamVar.Qualifiers.OfType<Semantic>())
                            variable.Qualifiers |= qualifier;

                    if (useSem && addAutoSem)
                    {
                        var semantic = variable.Qualifiers.Values.OfType<Semantic>().FirstOrDefault();
                        if (semantic == null)
                            variable.Qualifiers |= new Semantic(variable.Name.Text.ToUpper() + "_SEM");
                    }

                    tempStruct.Fields.Add(variable);
                }
            }
            return tempStruct;
        }

        /// <summary>
        /// Creates an intermediate structure given the stream usage
        /// </summary>
        /// <param name="streamStageUsage">the StreamStageUsage</param>
        /// <param name="stageName">the name of the stage</param>
        /// <returns>the intermediate stream structure</returns>
        private static StructType CreateIntermediateStructType(StreamStageUsage streamStageUsage, string stageName)
        {
            var tempList = new List<IDeclaration>();
            tempList.AddRange(streamStageUsage.InStreamList);
            tempList.AddRange(streamStageUsage.InterStreamList);
            tempList.AddRange(streamStageUsage.OutStreamList);
            return CreateStreamStructure(tempList.Distinct().ToList(), stageName + "_STREAMS", false, false);
        }

        /// <summary>
        /// Creates an declaration for this structure
        /// </summary>
        /// <param name="structType">the structure</param>
        /// <param name="structVarName">the name of the variable</param>
        /// <returns>the declaration statement</returns>
        private static DeclarationStatement CreateStructInit(StructType structType, string structVarName)
        {
            return new DeclarationStatement(
                new Variable(new TypeName(structType.Name), structVarName)
                    {
                        InitialValue = new CastExpression { From = new LiteralExpression(0), Target = new TypeName(structType.Name) }
                    });
        }

        #endregion
    }

    enum PdxShaderStage
    {
        Vertex,
        Hull,
        Constant,
        Domain,
        Geometry,
        Pixel,
        Compute,
        None
    }

    class StreamStageUsage
    {
        public PdxShaderStage ShaderStage = PdxShaderStage.None;
        public List<IDeclaration> InStreamList = new List<IDeclaration>();
        public List<IDeclaration> InterStreamList = new List<IDeclaration>();
        public List<IDeclaration> OutStreamList = new List<IDeclaration>();
    }
}
