// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Paradox.Shaders.Parser.Performance;
using SiliconStudio.Shaders.Analysis.Hlsl;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Shaders.Parser
{
    /// <summary>
    /// Parser for mixin.
    /// </summary>
    public class ShaderMixinParser
    {
        #region Private members

        /// <summary>
        /// An Objbect to lock the preprocess step (virtual tables building etc.).
        /// </summary>
        private readonly static Object PreprocessLock = new Object();

        /// <summary>
        /// An Objbect to lock the semantic analysis step.
        /// </summary>
        private readonly static Object SemanticAnalyzerLock = new Object();

        /// <summary>
        /// The CloneContext with the Hlsl classes and types
        /// </summary>
        private readonly CloneContext hlslCloneContext = new CloneContext();

        /// <summary>
        /// The library containing all the shaders
        /// </summary>
        private readonly ParadoxShaderLibrary shaderLibrary;

        #endregion

        #region Public members

        /// <summary>
        /// The shader source manager.
        /// </summary>
        public readonly ShaderSourceManager SourceManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinParser"/> class.
        /// </summary>
        public ShaderMixinParser()
        {
            SourceManager = new ShaderSourceManager();
            var shaderLoader = new ShaderLoader(SourceManager);
            
            if (shaderLibrary == null)
            {
                shaderLibrary = new ParadoxShaderLibrary(shaderLoader);
            }

            // Create the clone context with the instances of Hlsl classes
            HlslSemanticAnalysis.FillCloneContext(hlslCloneContext);
        }

        #endregion

        #region Public method

        /// <summary>
        /// Deletes the shader cache for the specified shaders.
        /// </summary>
        /// <param name="modifiedShaders">The modified shaders.</param>
        public void DeleteObsoleteCache(HashSet<string> modifiedShaders)
        {
            shaderLibrary.DeleteObsoleteCache(modifiedShaders);
        }
        public bool AllowNonInstantiatedGenerics
        {
            get
            {
                return shaderLibrary.AllowNonInstantiatedGenerics;
            }
            set
            {
                shaderLibrary.AllowNonInstantiatedGenerics = value;
            }
        }

        internal ShaderCompilationContext ParseAndAnalyze(ShaderMixinSource shaderMixinSource, Paradox.Shaders.ShaderMacro[] macros, out ShaderMixinParsingResult parsingResult, out HashSet<ModuleMixinInfo> mixinsToAnalyze)
        {
            // Creates a parsing result
            parsingResult = new ShaderMixinParsingResult();

            SiliconStudio.Shaders.Parser.ShaderMacro[] macrosParser;
            if (macros == null)
            {
                macrosParser = new SiliconStudio.Shaders.Parser.ShaderMacro[0];
            }
            else
            {
                macrosParser = new SiliconStudio.Shaders.Parser.ShaderMacro[macros.Length];
                for (var i = 0; i < macros.Length; ++i)
                    macrosParser[i] = new SiliconStudio.Shaders.Parser.ShaderMacro(macros[i].Name, macros[i].Definition);
            }
            //PerformanceLogger.Start(PerformanceStage.Global);

            // ----------------------------------------------------------
            // Load all shaders
            // ----------------------------------------------------------
            lock (shaderLibrary)
            {
                //PerformanceLogger.Start(PerformanceStage.Loading);
                mixinsToAnalyze = shaderLibrary.LoadShaderSource(shaderMixinSource, macrosParser);
                //PerformanceLogger.Stop(PerformanceStage.Loading);
            }

            // Extract all ModuleMixinInfo and check for any errors
            var allMixinInfos = new HashSet<ModuleMixinInfo>();
            foreach (var moduleMixinInfo in mixinsToAnalyze)
            {
                allMixinInfos.UnionWith(moduleMixinInfo.MinimalContext);
            }
            foreach (var moduleMixinInfo in allMixinInfos)
            {
                moduleMixinInfo.Log.CopyTo(parsingResult);

                var ast = moduleMixinInfo.MixinAst;
                var shaderClassSource = moduleMixinInfo.ShaderSource as ShaderClassSource;
                if (ast != null && shaderClassSource != null)
                {
                    parsingResult.HashSources[shaderClassSource.ClassName] = ast.SourceHash;
                }
            }

            // Return directly if there was any errors
            if (parsingResult.HasErrors)
                return null;

            // ----------------------------------------------------------
            // Perform Type Analysis
            // ----------------------------------------------------------
            //PerformanceLogger.Start(PerformanceStage.TypeAnalysis);
            var context = GetCompilationContext(mixinsToAnalyze, parsingResult);
            //PerformanceLogger.Stop(PerformanceStage.TypeAnalysis);

            // Return directly if there was any errors
            if (parsingResult.HasErrors)
                return context;

            lock (SemanticAnalyzerLock)
            {
                //PerformanceLogger.Start(PerformanceStage.SemanticAnalysis);
                //SemanticPerformance.Start(SemanticStage.Global);
                foreach (var mixin in mixinsToAnalyze)
                    context.Analyze(mixin);
                //SemanticPerformance.Pause(SemanticStage.Global);
                //PerformanceLogger.Stop(PerformanceStage.SemanticAnalysis);
            }

            return context;
        }

        /// <summary>
        /// Mixes shader parts to produces a single HLSL file shader.
        /// </summary>
        /// <param name="shaderMixinSource">The shader source.</param>
        /// <param name="macros">The shader perprocessor macros.</param>
        /// <param name="modifiedShaders">The list of modified shaders.</param>
        /// <returns>The combined shader in AST form.</returns>
        public ShaderMixinParsingResult Parse(ShaderMixinSource shaderMixinSource, Paradox.Shaders.ShaderMacro[] macros = null)
        {
            // Creates a parsing result
            HashSet<ModuleMixinInfo> mixinsToAnalyze;
            ShaderMixinParsingResult parsingResult;
            var context = ParseAndAnalyze(shaderMixinSource, macros, out parsingResult, out mixinsToAnalyze);

            // Return directly if there was any errors
            if (parsingResult.HasErrors)
                return parsingResult;

            // Update the clone context in case new instances of classes are created
            lock (hlslCloneContext)
            {
                HlslSemanticAnalysis.UpdateCloneContext(hlslCloneContext);
            }

            // only clone once the stage classes
            var mixCloneContext = new CloneContext(hlslCloneContext);
            foreach (var mixinInfo in mixinsToAnalyze)
            {
                foreach (var mixin in mixinInfo.Mixin.MinimalContext.Where(x => x.StageOnlyClass))
                {
                    mixin.DeepClone(mixCloneContext);
                }
            }

            // ----------------------------------------------------------
            // Perform Shader Mixer
            // ----------------------------------------------------------
            var externDict = new Dictionary<Variable, List<ModuleMixin>>();
            var finalModuleList = BuildCompositionsDictionary(shaderMixinSource, externDict, context, mixCloneContext);
            //PerformanceLogger.Stop(PerformanceStage.DeepClone);
            var mixinDictionary = BuildMixinDictionary(finalModuleList);

            if (finalModuleList != null)
            {
                var finalModule = finalModuleList[0];
                //PerformanceLogger.Start(PerformanceStage.Mix);
                var mixer = new ParadoxShaderMixer(finalModule, parsingResult, mixinDictionary, externDict, new CloneContext(mixCloneContext));
                mixer.Mix();
                //PerformanceLogger.Stop(PerformanceStage.Mix);

                // Return directly if there was any errors
                if (parsingResult.HasErrors)
                    return parsingResult;

                var finalShader = mixer.GetMixedShader();

                parsingResult.Reflection = new EffectReflection();
                var pdxShaderLinker = new ShaderLinker(parsingResult);
                pdxShaderLinker.Run(finalShader);

                // Return directly if there was any errors
                if (parsingResult.HasErrors)
                    return parsingResult;

                // Find all entry points
                // TODO: make this configurable by CompileParameters
                foreach (var stage in new[] {ShaderStage.Compute, ShaderStage.Vertex, ShaderStage.Hull, ShaderStage.Domain, ShaderStage.Geometry, ShaderStage.Pixel})
                {
                    var entryPoint = finalShader.Declarations.OfType<MethodDefinition>().FirstOrDefault(f => f.Attributes.OfType<AttributeDeclaration>().Any(a => a.Name == "EntryPoint" && (string)a.Parameters[0].Value == stage.ToString()));

                    if (entryPoint == null)
                    {
                        continue;
                    }

                    parsingResult.EntryPoints[stage] = entryPoint.Name.Text;
                    
                    // When this is a compute shader, there is no need to scan other stages
                    if (stage == ShaderStage.Compute)
                        break;
                }

                var typeCleaner = new ParadoxShaderCleaner();
                typeCleaner.Run(finalShader);

                //PerformanceLogger.Stop(PerformanceStage.Global);

                //PerformanceLogger.PrintLastResult();
                //SemanticPerformance.PrintResult();
                //MixPerformance.PrintResult();
                //GenerateShaderPerformance.PrintResult();
                //StreamCreatorPerformance.PrintResult();
                //ShaderLoader.PrintTime();

                //PerformanceLogger.WriteOut(52);

                parsingResult.Shader = finalShader;
            }

            return parsingResult;
        }

        #endregion

        #region Internal methods

        internal ModuleMixinInfo GetMixin(string mixinName)
        {
            return shaderLibrary.MixinInfos.FirstOrDefault(x => x.MixinGenericName == mixinName);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// create the context for each composition by cloning their dependencies
        /// </summary>
        /// <param name="shaderSource">the entry ShaderSource (root)</param>
        /// <param name="dictionary">the ouputed compositions</param>
        /// <param name="compilationContext">the compilation context</param>
        /// <param name="cloneContext">The clone context.</param>
        /// <returns>a list of all the needed mixins</returns>
        private List<ModuleMixin> BuildCompositionsDictionary(ShaderSource shaderSource, Dictionary<Variable, List<ModuleMixin>> dictionary, ShaderCompilationContext compilationContext, CloneContext cloneContext)
        {
            if (shaderSource is ShaderMixinSource)
            {
                var shaderMixinSource = shaderSource as ShaderMixinSource;

                var finalModule = compilationContext.GetModuleMixinFromShaderSource(shaderSource);

                //PerformanceLogger.Start(PerformanceStage.DeepClone);
                finalModule = finalModule.DeepClone(new CloneContext(cloneContext));
                //PerformanceLogger.Pause(PerformanceStage.DeepClone);

                foreach (var composition in shaderMixinSource.Compositions)
                {
                    //look for the key
                    var foundVars = finalModule.FindAllVariablesByName(composition.Key).Where(value => value.Variable.Qualifiers.Contains(ParadoxStorageQualifier.Compose)).ToList();

                    if (foundVars.Count > 0)
                    {
                        Variable foundVar = foundVars[0].Variable;
                        var moduleMixins = BuildCompositionsDictionary(composition.Value, dictionary, compilationContext, cloneContext);
                        if (moduleMixins == null)
                            return null;

                        dictionary.Add(foundVar, moduleMixins);
                    }
                    else
                    {
                        // TODO: log an error?
                    }
                }
                return new List<ModuleMixin> { finalModule };
            }


            if (shaderSource is ShaderClassSource)
            {
                var finalModule = compilationContext.GetModuleMixinFromShaderSource(shaderSource);

                //PerformanceLogger.Start(PerformanceStage.DeepClone);
                finalModule = finalModule.DeepClone(new CloneContext(cloneContext));
                //PerformanceLogger.Pause(PerformanceStage.DeepClone);

                return new List<ModuleMixin> { finalModule };
            }

            if (shaderSource is ShaderArraySource)
            {
                var shaderArraySource = shaderSource as ShaderArraySource;
                var compositionArray = new List<ModuleMixin>();
                foreach (var shader in shaderArraySource.Values)
                {
                    var mixin = BuildCompositionsDictionary(shader, dictionary, compilationContext, cloneContext);
                    if (mixin == null)
                        return null;
                    compositionArray.AddRange(mixin);
                }
                return compositionArray;
            }

            return null;
        }

        /// <summary>
        /// Get a compilation context based on the macros
        /// </summary>
        /// <param name="mixinToAnalyze">List of mixin to analyze</param>
        /// <param name="log">The log.</param>
        /// <returns>the correct compilation context</returns>
        private ShaderCompilationContext GetCompilationContext(IEnumerable<ModuleMixinInfo> mixinToAnalyze, LoggerResult log)
        {
            var mixinInfos = new HashSet<ModuleMixinInfo>();
            foreach (var mixin in mixinToAnalyze)
                mixinInfos.UnionWith(mixin.MinimalContext);

            var context = new ShaderCompilationContext(log);
            context.Preprocess(mixinInfos);
            return context;
        }

        /// <summary>
        /// Build a dictionary of mixins
        /// </summary>
        /// <param name="finalMixins">a list of mixins</param>
        /// <returns>a dictionary of all the necessary mixins</returns>
        private Dictionary<string, ModuleMixin> BuildMixinDictionary(IEnumerable<ModuleMixin> finalMixins)
        {
            var allMixins = new HashSet<ModuleMixin>();
            foreach (var mixin in finalMixins)
            {
                if (allMixins.All(x => x.MixinName != mixin.MixinName))
                    allMixins.Add(mixin);
            }

            return allMixins.ToDictionary(x => x.MixinName, x => x);
        }

        #endregion
    }
}
