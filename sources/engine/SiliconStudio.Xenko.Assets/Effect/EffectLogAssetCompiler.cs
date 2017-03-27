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
    [AssetCompiler(typeof(EffectLogAsset), typeof(AssetCompilationContext))]
    public class EffectLogAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<ObjectUrl> GetInputFiles(AssetCompilerContext context, AssetItem assetItem)
        {
            foreach (var sessionPackage in assetItem.Package.Session.Packages)
            {
                foreach (var sessionPackageAsset in sessionPackage.Assets)
                {
                    if (sessionPackageAsset.Asset is EffectShaderAsset)
                    {
                        yield return new ObjectUrl(UrlType.Content, sessionPackageAsset.Location);
                    }
                }
            }
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var originalSourcePath = assetItem.FullPath;

            result.BuildSteps = new AssetBuildStep(assetItem);

            var urlRoot = originalSourcePath.GetParent();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(((EffectLogAsset)assetItem.Asset).Text));
            using (var recordedEffectCompile = new EffectLogStore(stream))
            {
                recordedEffectCompile.LoadNewValues();

                foreach (var entry in recordedEffectCompile.GetValues())
                {
                    result.BuildSteps.Add(EffectCompileCommand.FromRequest(context, assetItem.Package, urlRoot, entry.Key));
                }
            }
        }
    }
}
