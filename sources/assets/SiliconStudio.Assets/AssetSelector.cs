// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
