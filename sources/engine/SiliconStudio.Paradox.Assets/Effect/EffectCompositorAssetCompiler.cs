// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Shaders.Parser;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Entry point to compile an <see cref="EffectCompositorAsset"/> (pdxfx).
    /// </summary>
    public class EffectCompositorAssetCompiler : AssetCompilerBase<EffectCompositorAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EffectCompositorAsset asset, AssetCompilerResult result)
        {
            // Note The pdxfx is actually already pre-compiled but we are double checking its integrity
            // with this command 
            result.BuildSteps = new ListBuildStep           
                {
                    new EffectCompositorAssetCommand(urlInStorage, asset),
                };
        }

        internal class EffectCompositorAssetCommand : AssetCommand<EffectCompositorAsset>
        {
            private readonly string sourceLocationOnDisk;

            public EffectCompositorAssetCommand(string url, EffectCompositorAsset description) : base(url, description)
            {
                this.sourceLocationOnDisk = description.AbsoluteSourceLocation;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var logger = commandContext.Logger;

                var status = ResultStatus.Successful;
                try
                {
                    var parsingResults = ParadoxShaderParser.TryPreProcessAndParse(asset.Text, sourceLocationOnDisk);
                    if (parsingResults.HasErrors)
                    {
                        foreach (var message in parsingResults.Messages)
                        {
                            if (message.Level == ReportMessageLevel.Error)
                            {
                                logger.Error(message.ToString());
                            }
                        }
                        return Task.FromResult(ResultStatus.Failed);
                    }

                    var shader = parsingResults.Shader;
                    var loggerResult = new LoggerResult();

                    // Run shader codegen mixin in order to check that everything is well defined and compiled
                    var shaderMixinCodeGen = new ShaderMixinCodeGen(shader, loggerResult);
                    shaderMixinCodeGen.Run();
                }
                catch (Exception ex)
                {
                    commandContext.Logger.Error("Error while processing pdxfx [{0}]", ex, sourceLocationOnDisk);
                    status = ResultStatus.Failed;
                }

                return Task.FromResult(status);
            }
        }
    }
}