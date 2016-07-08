using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    [DataContract("UILibraryAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(UIPageAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI")]
    [AssetPartReference(typeof(UIElement))]
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
        /// <returns>An <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> containing the cloned elements of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, string targetLocation)
        {
            Guid unused;
            return CreateElementInstance(targetContainer, targetLocation, out unused);
        }

        /// <summary>
        /// Creates a instance of the given control that can be added to another <see cref="UIAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of this asset.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> containing the cloned elements of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CreateElementInstance(UIAssetBase targetContainer, string targetLocation, out Guid instanceId)
        {
            var instance = (UILibraryAsset)CreateChildAsset(targetLocation);

            targetContainer.AddBasePart(instance.Base);
            instanceId = Guid.NewGuid();
            foreach (var elementEntry in instance.Hierarchy.Parts)
            {
                elementEntry.BasePartInstanceId = instanceId;
            }
            return instance.Hierarchy;
        }
    }
}
