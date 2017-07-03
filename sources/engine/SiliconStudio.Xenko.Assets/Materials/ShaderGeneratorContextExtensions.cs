// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Assets;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Assets.Materials
{
    public static class ShaderGeneratorContextExtensions
    {
        public static void AddLoadingFromSession(this ShaderGeneratorContext context, IAssetFinder package)
        {
            var previousGetAssetFriendlyName = context.GetAssetFriendlyName;
            var previousFindAsset = context.FindAsset;

            // Setup the GetAssetFriendlyName callback
            context.GetAssetFriendlyName = runtimeAsset =>
            {
                string assetFriendlyName = null;

                if (previousGetAssetFriendlyName != null)
                {
                    assetFriendlyName = previousGetAssetFriendlyName(runtimeAsset);
                }

                if (string.IsNullOrEmpty(assetFriendlyName))
                {
                    var referenceAsset = AttachedReferenceManager.GetAttachedReference(runtimeAsset);
                    assetFriendlyName = $"{referenceAsset.Id}:{referenceAsset.Url}";
                }

                return assetFriendlyName;
            };

            // Setup the FindAsset callback
            context.FindAsset = runtimeAsset =>
            {
                object newAsset = null; 
                if (previousFindAsset != null)
                {
                    newAsset = previousFindAsset(runtimeAsset);
                }

                if (newAsset != null)
                {
                    return newAsset;
                }

                var reference = AttachedReferenceManager.GetAttachedReference(runtimeAsset);


                var assetItem = package.FindAsset(reference.Id);

                return assetItem?.Asset;
            };            
        }
    }
}
