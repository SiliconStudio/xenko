// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public static class ShaderGeneratorContextExtensions
    {
        public static void AddLoadingFromSession(this ShaderGeneratorContextBase shaderGeneratorContext, Package package)
        {
            shaderGeneratorContext.FindAsset = material =>
            {
                var reference = AttachedReferenceManager.GetAttachedReference(material);

                var assetItem = package.Session.FindAsset(reference.Id) ?? package.Session.FindAsset(reference.Url);

                if (assetItem == null)
                {
                    return null;
                }
                return assetItem.Asset;
            };            
        }
    }
}