// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Assets.Effect
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
            result.BuildSteps = new AssetBuildStep(AssetItem) { new EffectLogBuildStep(context, originalSourcePath, AssetItem.Package) };
        }

        public class EffectLogBuildStep : EnumerableBuildStep
        {
            private readonly UFile originalSourcePath;
            private readonly AssetCompilerContext context;
            private readonly Package package;

            public EffectLogBuildStep(AssetCompilerContext context, UFile originalSourcePath, Package package)
            {
                this.context = context;
                this.originalSourcePath = originalSourcePath;
                this.package = package;
            }

            /// <inheritdoc/>
            public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
            {
                var steps = new List<BuildStep>();

                var urlRoot = originalSourcePath.GetParent();

                var fileStream = new FileStream(originalSourcePath.ToWindowsPath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (var recordedEffectCompile = new EffectLogStore(fileStream))
                {
                    recordedEffectCompile.LoadNewValues();

                    foreach (var entry in recordedEffectCompile.GetValues())
                    {
                        steps.Add(EffectCompileCommand.FromRequest(context, package, urlRoot, entry.Key));
                    }
                }

                Steps = steps;

                return base.Execute(executeContext, builderContext);
            }

            /// <inheritdoc/>
            public override BuildStep Clone()
            {
                return new EffectLogBuildStep(context, originalSourcePath, package);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                string title = "Recompile effects "; try { title += Path.GetFileName(originalSourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title;
            }
        }
    }
}