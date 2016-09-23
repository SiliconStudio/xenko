// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset selector
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class AssetSelector
    {
        public abstract IEnumerable<string> Select(PackageSession packageSession, IContentIndexMap contentIndexMap);
    }
}