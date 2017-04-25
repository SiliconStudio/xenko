// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Assets.Models
{
    /// <summary>
    /// This interface represents an asset containing a model.
    /// </summary>
    public interface IModelAsset
    {
        /// <summary>
        /// The materials.
        /// </summary>
        /// <userdoc>
        /// The list of materials in the model.
        /// </userdoc>
        List<ModelMaterial> Materials { get; }
    }
}
