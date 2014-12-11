// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// This command is responsible to compile a single permutation of an effect (pdxfx or pdxsl)
    /// </summary>
    internal class EffectCompileCommand : IndexFileCommand
    {
        private static readonly PropertyKey<EffectCompilerBase> CompilerKey = new PropertyKey<EffectCompilerBase>("CompilerKey", typeof(EffectCompileCommand));

        private readonly AssetCompilerContext context;
        private readonly UDirectory baseUrl;
        private string effectName;
        private CompilerParameters compilerParameters;
        private static Dictionary<string, int> PermutationCount = new Dictionary<string, int>();

        public EffectCompileCommand(AssetCompilerContext context, UDirectory baseUrl, string effectName, CompilerParameters compilerParameters)
        {
            this.context = context;
            this.effectName = effectName;
            this.baseUrl = baseUrl;
            this.compilerParameters = compilerParameters;
        }

        public override string Title
        {
            get
            {
                return string.Format("EffectCompile [{0}]", effectName);
            }
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);
            uint effectbyteCodeMagicNumber = EffectBytecode.MagicHeader;
            writer.Serialize(ref effectbyteCodeMagicNumber, ArchiveMode.Serialize);
            writer.Serialize(ref effectName, ArchiveMode.Serialize);
            writer.Serialize(ref compilerParameters, ArchiveMode.Serialize);
        }

        protected override void ComputeAssemblyHash(BinarySerializationWriter writer)
        {
            writer.Write(DataSerializer.BinaryFormatVersion);
            writer.Write(EffectBytecode.MagicHeader);
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var compiler = GetOrCreateEffectCompiler(context);

            var isPdxfx = ShaderMixinManager.Contains(effectName);
            var source = isPdxfx ? new ShaderMixinGeneratorSource(effectName) : (ShaderSource)new ShaderClassSource(effectName);

            int permutationCount;
            lock (PermutationCount)
            {
                PermutationCount.TryGetValue(effectName, out permutationCount);
                permutationCount++;
                PermutationCount[effectName] = permutationCount;
            }
            commandContext.Logger.Info("Trying permutation #{0} for effect [{1}]: \n{2}", permutationCount, effectName, compilerParameters.ToStringDetailed());

            var compilerResults = compiler.Compile(source, compilerParameters);

            // Copy logs and if there are errors, exit directlry
            compilerResults.CopyTo(commandContext.Logger);
            if (compilerResults.HasErrors)
            {
                return Task.FromResult(ResultStatus.Failed);
            }

            // Register all dependencies
            var allSources = new HashSet<string>(compilerResults.Bytecodes.SelectMany(bytecode => bytecode.Value.HashSources).Select(keyPair => keyPair.Key));
            foreach (var className in allSources)
            {
                commandContext.RegisterInputDependency(new ObjectUrl(UrlType.Internal, EffectCompilerBase.GetStoragePathFromShaderType(className)));
            }

            // Generate sourcecode if configured
            if (compilerParameters.ContainsKey(EffectSourceCodeKeys.Enable))
            {
                var outputDirectory = UPath.Combine(context.Package.RootDirectory, baseUrl);
                var outputClassFile = effectName + ".bytecode." + compilerParameters.Platform + "." + compilerParameters.Profile + ".cs";
                var fullOutputClassFile = Path.Combine(outputDirectory, outputClassFile);

                commandContext.Logger.Info("Writing shader bytecode to .cs source [{0}]", fullOutputClassFile);
                using (var stream = new FileStream(fullOutputClassFile, FileMode.Create, FileAccess.Write, FileShare.Write))
                    EffectByteCodeToSourceCodeWriter.Write(effectName, compilerParameters, compilerResults.MainBytecode, new StreamWriter(stream, Encoding.UTF8));
            }

            return Task.FromResult(ResultStatus.Successful);
        }

        public override string ToString()
        {
            return Title;
        }

        private static EffectCompilerBase GetOrCreateEffectCompiler(AssetCompilerContext context)
        {
            lock (context)
            {
                var compiler = context.Properties.Get(CompilerKey);
                if (compiler == null)
                {
                    // Create compiler
                    var effectCompiler = new Shaders.Compiler.EffectCompiler();
                    effectCompiler.SourceDirectories.Add(EffectCompilerBase.DefaultSourceShaderFolder);
                    compiler = new EffectCompilerCache(effectCompiler);
                    context.Properties.Set(CompilerKey, compiler);

                    var shaderLocations = context.Properties.Get(EffectShaderAssetCompiler.ShaderLocationsKey);

                    // Temp copy URL to absolute file path to inform the compiler the absolute file location
                    // of all pdxsl files.
                    if (shaderLocations != null)
                    {
                        foreach (var shaderLocation in shaderLocations)
                        {
                            effectCompiler.UrlToFilePath[shaderLocation.Key] = shaderLocation.Value;
                        }
                    }
                }

                return compiler;
            }
        }
    }
}