using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface representing a part in an <see cref="AssetComposite"/>.
    /// </summary>
    /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
    public interface IAssetPartDesign<TAssetPart> where TAssetPart : IIdentifiable
    {
        [CanBeNull]
        BasePart Base { get; set; }

        /// <summary>
        /// The actual part.
        /// </summary>
        [NotNull]
        TAssetPart Part { get; set; }
    }
}
