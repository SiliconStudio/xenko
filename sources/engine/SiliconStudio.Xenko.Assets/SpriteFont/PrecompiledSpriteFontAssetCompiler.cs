// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.TextureConverter;
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class PrecompiledSpriteFontAssetCompiler : AssetCompilerBase<PrecompiledSpriteFontAsset>
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, PrecompiledSpriteFontAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            result.BuildSteps = new AssetBuildStep(AssetItem) { new PrecompiledSpriteFontCommand(urlInStorage, asset) };
        }

        internal class PrecompiledSpriteFontCommand : AssetCommand<PrecompiledSpriteFontAsset>
        {
            public PrecompiledSpriteFontCommand(string url, PrecompiledSpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                yield return new ObjectUrl(UrlType.File, AssetParameters.FontDataFile);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(AssetParameters.FontDataFile, AssetParameters.IsSrgb))
                {
                    //make sure we are RGBA and not BGRA
                    texTool.Convert(texImage, AssetParameters.IsSrgb ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm);

                    var image = texTool.ConvertToXenkoImage(texImage);

                    Graphics.SpriteFont staticFont = FontDataFactory.NewStatic(
                        AssetParameters.Size,
                        AssetParameters.Glyphs,
                        new[] { image },
                        AssetParameters.BaseOffset,
                        AssetParameters.DefaultLineSpacing,
                        AssetParameters.Kernings,
                        AssetParameters.ExtraSpacing,
                        AssetParameters.ExtraLineSpacing,
                        AssetParameters.DefaultCharacter);

                    // save the data into the database
                    var assetManager = new ContentManager();
                    assetManager.Save(Url, staticFont);

                    image.Dispose();
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
