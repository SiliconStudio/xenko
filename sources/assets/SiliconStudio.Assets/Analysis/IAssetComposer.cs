// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// The interface for elements inheriting from assets via compositions.
    /// </summary>
    public interface IAssetComposer
    {
        /// <summary>
        /// Gets the list of base assets corresponding to the compositions of the current element.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IContentReference> GetCompositionBases();
    }
}