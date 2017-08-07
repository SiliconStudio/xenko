// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface representing a design-time part in an <see cref="AssetComposite"/>.
    /// </summary>
    public interface IAssetPartDesign
    {
        [CanBeNull]
        BasePart Base { get; set; }

        [NotNull]
        IIdentifiable Part { get; }
    }

    /// <summary>
    /// An interface representing a design-time part in an <see cref="AssetComposite"/>.
    /// </summary>
    /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
    public interface IAssetPartDesign<out TAssetPart> : IAssetPartDesign
        where TAssetPart : IIdentifiable
    {
        /// <summary>
        /// The actual part.
        /// </summary>
        [NotNull]
        new TAssetPart Part { get; }
    }
}
