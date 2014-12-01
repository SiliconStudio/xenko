// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Concurrent;
using System.IO;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Entry point to compile an <see cref="EffectCompositorAsset"/>
    /// </summary>
    public class EffectShaderAssetCompiler : AssetCompilerBase<EffectShaderAsset>
    {
        public static readonly PropertyKey<ConcurrentDictionary<string, string>> ShaderLocationsKey = new PropertyKey<ConcurrentDictionary<string, string>>("ShaderPathsKey", typeof(EffectShaderAssetCompiler));

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EffectShaderAsset asset, AssetCompilerResult result)
        {
            var url = EffectCompilerBase.DefaultSourceShaderFolder + "/" + Path.GetFileName(assetAbsolutePath);

            var originalSourcePath = asset.AbsoluteSourceLocation;
            result.BuildSteps = new ListBuildStep { new ImportStreamCommand { SourcePath = originalSourcePath, Location = url, SaveSourcePath = true } };
            var shaderLocations = (ConcurrentDictionary<string, string>)context.Properties.GetOrAdd(ShaderLocationsKey, key => new ConcurrentDictionary<string, string>());

            // Store directly this into the context TODO this this temporary
            shaderLocations[url] = originalSourcePath;
        }
    }
}