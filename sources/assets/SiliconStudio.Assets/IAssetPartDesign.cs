using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface representing a part in an <see cref="AssetComposite"/>.
    /// </summary>
    /// <typeparam name="TAssetPart">The underlaying type of part.</typeparam>
    public interface IAssetPartDesign<out TAssetPart> where TAssetPart : IIdentifiable
    {
        /// <summary>
        /// Gets or sets the unique identifier of the base part.
        /// </summary>
        Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group. If null, the entity doesn't belong to a part group.
        /// </summary>
        Guid? BasePartInstanceId { get; set; }

        /// <summary>
        /// Gets the actual part.
        /// </summary>
        TAssetPart Part { get; }
    }
}
