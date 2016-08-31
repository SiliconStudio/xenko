// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    [DataContract("UILibraryAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(UILibraryAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI Library")]
    public class UILibraryAsset : UIAssetBase
    {
        private const string CurrentVersion = "0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UILibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuilib";

        /// <summary>
        /// Gets the dictionary of publicly exposed controls.
        /// </summary>
        [DataMember(20)]
        public Dictionary<Guid, string> PublicUIElements { get; } = new Dictionary<Guid, string>();

        /// <summary>
        /// Creates a instance of the given control that can be added to another <see cref="UIAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of the <see paramref="targetContainer"/> asset.</param>
        /// <param name="elementId">The id of the element to instantiate.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> containing the cloned elements of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, string targetLocation, Guid elementId)
        {
            Guid unused;
            return CreateElementInstance(targetContainer, targetLocation, elementId, out unused);
        }

        /// <summary>
        /// Creates a instance of the given control that can be added to another <see cref="UIAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of this asset.</param>
        /// <param name="elementId">The id of the element to instantiate.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> containing the cloned elements of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, string targetLocation, Guid elementId, out Guid instanceId)
        {
            // TODO: make a common base method in AssetCompositeHierarchy - the beginning of the method is similar to CreatePrefabInstance
            var idRemapping = new Dictionary<Guid, Guid>();
            var instance = (UILibraryAsset)CreateChildAsset(targetLocation, idRemapping);

            var rootElementId = idRemapping[elementId];
            if (!instance.Hierarchy.RootPartIds.Contains(rootElementId))
                throw new ArgumentException(@"The given id cannot be found in the root parts of this library.", nameof(elementId));

            targetContainer.AddBasePart(instance.Base);
            instanceId = Guid.NewGuid();
            foreach (var elementEntry in instance.Hierarchy.Parts)
            {
                elementEntry.BasePartInstanceId = instanceId;
            }

            var result = new AssetCompositeHierarchyData<UIElementDesign, UIElement>();
            result.RootPartIds.Add(rootElementId);
            result.Parts.Add(instance.Hierarchy.Parts[rootElementId]);
            foreach (var element in EnumerateChildParts(instance.Hierarchy.Parts[rootElementId], instance.Hierarchy, true))
            {
                result.Parts.Add(element);
            }
            return result;
        }
    }
}
