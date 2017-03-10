// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Compiles same effects as a previous recorded session.
    /// </summary>
    public class EffectLogAssetCompiler : AssetCompilerBase
    {
        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (EffectLogAsset)assetItem.Asset;
            var originalSourcePath = assetItem.FullPath;
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new AssetBuildStep(assetItem) { new EffectLogBuildStep(context, originalSourcePath, assetItem) };
        }

        public class EffectLogBuildStep : EnumerableBuildStep
        {
            private readonly UFile originalSourcePath;
            private readonly AssetCompilerContext context;
            private readonly AssetItem assetItem;

            public EffectLogBuildStep(AssetCompilerContext context, UFile originalSourcePath, AssetItem assetItem)
            {
                this.context = context;
                this.originalSourcePath = originalSourcePath;
                this.assetItem = assetItem;
            }

            /// <inheritdoc/>
            public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
            {
                var steps = new List<BuildStep>();

                var urlRoot = originalSourcePath.GetParent();

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(((EffectLogAsset)assetItem.Asset).Text));
                using (var recordedEffectCompile = new EffectLogStore(stream))
                {
                    recordedEffectCompile.LoadNewValues();

                    foreach (var entry in recordedEffectCompile.GetValues())
                    {
                        steps.Add(EffectCompileCommand.FromRequest(context, assetItem.Package, urlRoot, entry.Key));
                    }
                }

                Steps = steps;

                return base.Execute(executeContext, builderContext);
            }

            /// <inheritdoc/>
            public override BuildStep Clone()
            {
                return new EffectLogBuildStep(context, originalSourcePath, assetItem);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                string title = "Recompile effects "; try { title += Path.GetFileName(originalSourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title;
            }
        }
    }
}
