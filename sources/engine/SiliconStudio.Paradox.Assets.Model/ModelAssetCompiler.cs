// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Data;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class ModelAssetCompiler : AssetCompilerBase<ModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ModelAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;
        
            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            var allow32BitIndex = context.GetGraphicsProfile() >= GraphicsProfile.Level_9_2;
            var allowUnsignedBlendIndices = context.GetGraphicsPlatform() != GraphicsPlatform.OpenGLES;
            var extension = asset.Source.GetFileExtension();

            if (ImportFbxCommand.IsSupportingExtensions(extension))
            {
                result.BuildSteps = new AssetBuildStep(AssetItem)
                    {
                        new ImportFbxCommand
                            {
                                SourcePath = assetSource,
                                Location = urlInStorage,
                                Allow32BitIndex = allow32BitIndex,
                                AllowUnsignedBlendIndices = allowUnsignedBlendIndices,
                                Compact = asset.Compact,
                                PreservedNodes = asset.PreservedNodes,
                                Materials = asset.Materials,
                                ScaleImport = asset.ScaleImport,
                            },
                    };
            }
            else if (ImportAssimpCommand.IsSupportingExtensions(extension))
            {
                result.BuildSteps = new AssetBuildStep(AssetItem)
                    {
                        new ImportAssimpCommand
                            {
                                SourcePath = assetSource,
                                Location = urlInStorage,
                                Allow32BitIndex = allow32BitIndex,
                                AllowUnsignedBlendIndices = allowUnsignedBlendIndices,
                                Compact = asset.Compact,
                                PreservedNodes = asset.PreservedNodes,
                                Materials = asset.Materials,
                                ScaleImport = asset.ScaleImport,
                            },
                    };
            }
            else
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
            }
        }
    }

    
}