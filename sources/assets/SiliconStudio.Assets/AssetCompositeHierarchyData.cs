using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A class containing the information of a hierarchy of asset parts contained in an <see cref="AssetCompositeHierarchy{TAssetPartDesign, TAssetPart}"/>.
    /// </summary>
    /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
    /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
    [DataContract("AssetCompositeHierarchyData")]
    public class AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : IIdentifiable
    {
        /// <summary>
        /// Gets a collection if identifier of all the parts that are root of this hierarchy.
        /// </summary>
        [DataMember(10)]
        public List<Guid> RootPartIds { get; } = new List<Guid>();

        /// <summary>
        /// Gets a collection of all the parts, root or not, contained in this hierarchy.
        /// </summary>
        [DataMember(20)]
        public AssetPartCollection<TAssetPartDesign, TAssetPart> Parts { get; } = new AssetPartCollection<TAssetPartDesign, TAssetPart>();
    }
}