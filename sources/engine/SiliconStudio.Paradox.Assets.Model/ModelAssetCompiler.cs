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
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class ModelAssetCompiler : AssetCompilerBase<ModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ModelAsset asset, AssetCompilerResult result)
        {
            if (asset.Source == null)
            {
                result.Error("Source cannot be null for Texture Asset [{0}]", asset);
                return;
            }
            
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
                            },
                        new WaitBuildStep(),
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
                            },
                        new WaitBuildStep(),
                    };
            }
            else
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
            }
        }
    }

    
}