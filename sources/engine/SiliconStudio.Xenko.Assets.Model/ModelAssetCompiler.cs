// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Model
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

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var allow32BitIndex = gameSettingsAsset.DefaultGraphicsProfile >= GraphicsProfile.Level_9_2;
            var allowUnsignedBlendIndices = context.GetGraphicsPlatform() != GraphicsPlatform.OpenGLES;
            var extension = asset.Source.GetFileExtension();

            // Find skeleton asset, if any
            AssetItem skeleton = null;
            if (asset.Skeleton != null)
                skeleton = AssetItem.Package.FindAssetFromAttachedReference(asset.Skeleton);

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
                                Compact = ((SkeletonAsset)skeleton?.Asset)?.Compact ?? true,
                                OptimizedNodes = ((SkeletonAsset)skeleton?.Asset)?.OptimizedNodes,
                                Materials = asset.Materials,
                                ScaleImport = asset.ScaleImport,
                                SkeletonUrl = skeleton?.Location,
                            }
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
                                Compact = ((SkeletonAsset)skeleton?.Asset)?.Compact ?? true,
                                OptimizedNodes = ((SkeletonAsset)skeleton?.Asset)?.OptimizedNodes,
                                Materials = asset.Materials,
                                ScaleImport = asset.ScaleImport,
                                SkeletonUrl = skeleton?.Location,
                            }
                    };
            }
            else
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
            }
        }
    }

    
}