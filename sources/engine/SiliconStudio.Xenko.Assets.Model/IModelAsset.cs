// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Assets.Model
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