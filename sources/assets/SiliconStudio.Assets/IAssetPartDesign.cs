using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface representing a part in an <see cref="AssetComposite"/>.
    /// </summary>
    /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
    public interface IAssetPartDesign<out TAssetPart> where TAssetPart : IIdentifiable
    {
        BasePart Base { get; set; }

        /// <summary>
        /// Gets the actual part.
        /// </summary>
        TAssetPart Part { get; }
    }
}
