// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    [DataContract("UILibraryAsset")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetContentType(typeof(UILibrary))]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.9.0-beta01")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta01", "1.10.0-beta01", typeof(FixPartReferenceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta01", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    [Display("UI Library")]
    public class UILibraryAsset : UIAssetBase
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UILibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuilib";

        /// <summary>
        /// Gets the dictionary of publicly exposed controls.
        /// </summary>
        [DataMember(20)]
        [NonIdentifiableCollectionItems]
        public Dictionary<Guid, string> PublicUIElements { get; } = new Dictionary<Guid, string>();

        /// <summary>
        /// Creates a instance of the given control that can be added to another <see cref="UIAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of the <see paramref="targetContainer"/> asset.</param>
        /// <param name="elementId">The id of the element to instantiate.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> containing the cloned elements of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        [NotNull]
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, [NotNull] string targetLocation, Guid elementId)
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
        [NotNull]
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, [NotNull] string targetLocation, Guid elementId, out Guid instanceId)
        {
            // TODO: make a common base method in AssetCompositeHierarchy - the beginning of the method is similar to CreatePrefabInstance
            var idRemapping = new Dictionary<Guid, Guid>();
            var instance = (UILibraryAsset)CreateDerivedAsset(targetLocation, out idRemapping);

            var rootElementId = idRemapping[elementId];
            if (instance.Hierarchy.RootParts.All(x => x.Id != rootElementId))
                throw new ArgumentException(@"The given id cannot be found in the root parts of this library.", nameof(elementId));

            instanceId = instance.Hierarchy.Parts.FirstOrDefault()?.Base?.InstanceId ?? Guid.NewGuid();

            var result = new AssetCompositeHierarchyData<UIElementDesign, UIElement>();
            result.RootParts.Add(instance.Hierarchy.Parts[rootElementId].UIElement);
            result.Parts.Add(instance.Hierarchy.Parts[rootElementId]);
            foreach (var element in this.EnumerateChildPartDesigns(instance.Hierarchy.Parts[rootElementId], instance.Hierarchy, true))
            {
                result.Parts.Add(element);
            }
            return result;
        }
    }
}
