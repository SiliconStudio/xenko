// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class PrecompiledSpriteFontAssetCompiler : AssetCompilerBase<PrecompiledSpriteFontAsset>
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, PrecompiledSpriteFontAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;
            
            result.BuildSteps = new AssetBuildStep(AssetItem) { new PregeneratedSpriteFontCommand(urlInStorage, asset) };
        }

        internal class PregeneratedSpriteFontCommand : AssetCommand<PrecompiledSpriteFontAsset>
        {
            public PregeneratedSpriteFontCommand(string url, PrecompiledSpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                yield return new ObjectUrl(UrlType.File, AssetParameters.Source);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                using (var imageStream = File.OpenRead(AssetParameters.Source))
                {
                    Image image;
                    try
                    {
                        image = Image.Load(imageStream);
                    }
                    catch (FontNotFoundException ex)
                    {
                        commandContext.Logger.Error("The file [{0}] is not a proper image file.", ex.FontName);
                        return Task.FromResult(ResultStatus.Failed);
                    }

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
                    var assetManager = new AssetManager();
                    assetManager.Save(Url, staticFont);

                    image.Dispose();
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}