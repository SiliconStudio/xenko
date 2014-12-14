// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    internal class LightingAssetCompiler : AssetCompilerBase<LightingAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, LightingAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = false;
            result.BuildSteps = new ListBuildStep { new LightingConfigurationCompileCommand(urlInStorage, asset, context) };
        }

        private class LightingConfigurationCompileCommand : AssetCommand<LightingAsset>
        {
            private readonly AssetCompilerContext context;

            private UFile assetUrl;

            public LightingConfigurationCompileCommand(string url, LightingAsset value, AssetCompilerContext context)
                : base(url, value)
            {
                this.context = context;
                assetUrl = new UFile(url);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var compiler = new LightingCompileGenerator(asset, context);
                var results = compiler.Execute();

                if (results != null && results.Count != 0)
                {
                    // Check unrolls
                    // NOTE: this is note safe since it is possible that the parameters are overridden somewhere else... but the user should not do that
                    var configDict = new Dictionary<LightingConfiguration, CompilerParameters>();
                    foreach (var perm in results)
                    {
                        configDict.Add(new LightingConfiguration(perm), perm);
                    }
                    var configList = configDict.Select(x => x.Key).ToList();
                    configList.Sort((x, y) => x.TotalLightCount - y.TotalLightCount);
                    // TODO: remove duplicates
                    LightingConfiguration.CheckUnrolls(configList.ToArray());

                    var configs = new LightingConfigurationsSet();
                    configs.Configs = configList.ToArray();

                    var assetManager = new AssetManager();
                    assetManager.Save(assetUrl, configs);
                }
                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
