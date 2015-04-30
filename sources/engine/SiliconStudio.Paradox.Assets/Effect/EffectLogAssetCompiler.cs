// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Compiles same effects as a previous recorded session.
    /// </summary>
    public class EffectLogAssetCompiler : AssetCompilerBase<EffectLogAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EffectLogAsset asset, AssetCompilerResult result)
        {
            var originalSourcePath = asset.AbsoluteSourceLocation;
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new AssetBuildStep(AssetItem) { new EffectLogBuildStep(context, originalSourcePath) };
        }

        public class EffectLogBuildStep : EnumerableBuildStep
        {
            private UFile originalSourcePath;
            private AssetCompilerContext context;

            public EffectLogBuildStep(AssetCompilerContext context, UFile originalSourcePath)
            {
                this.context = context;
                this.originalSourcePath = originalSourcePath;
            }

            /// <inheritdoc/>
            public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
            {
                var steps = new List<BuildStep>();
                Steps = steps;

                var urlRoot = originalSourcePath.GetParent();

                var fileStream = new FileStream(originalSourcePath.ToWindowsPath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                var recordedEffectCompile = new EffectLogStore(fileStream);
                recordedEffectCompile.LoadNewValues();

                foreach (var entry in recordedEffectCompile.GetValues())
                {
                    var effectCompileRequest = entry.Key;

                    var compilerParameters = new CompilerParameters();
                    effectCompileRequest.UsedParameters.CopyTo(compilerParameters);
                    compilerParameters.Platform = context.GetGraphicsPlatform();
                    compilerParameters.Profile = context.GetGraphicsProfile();
                    steps.Add(new CommandBuildStep(new EffectCompileCommand(context, urlRoot, effectCompileRequest.EffectName, compilerParameters)));
                }

                return base.Execute(executeContext, builderContext);
            }

            /// <inheritdoc/>
            public override BuildStep Clone()
            {
                return new EffectLogBuildStep(context, originalSourcePath);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                string title = "Recompile effects "; try { title += Path.GetFileName(originalSourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title;
            }
        }
    }
}