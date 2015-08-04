// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Rendering.Materials;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public static class ShaderGeneratorContextExtensions
    {
        public static void AddLoadingFromSession(this ShaderGeneratorContextBase shaderGeneratorContext, Package package)
        {
            shaderGeneratorContext.FindAsset = material =>
            {
                if (material.Descriptor != null)
                {
                    return material.Descriptor;
                }

                var reference = AttachedReferenceManager.GetAttachedReference(material);

                var assetItem = package.Session.FindAsset(reference.Id) ?? package.Session.FindAsset(reference.Url);

                if (assetItem == null)
                {
                    return null;
                }
                return (IMaterialDescriptor)assetItem.Asset;
            };            
        }
    }
}