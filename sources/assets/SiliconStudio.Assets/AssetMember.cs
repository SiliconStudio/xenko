// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Represents a member of an asset.
    /// </summary>
    public struct AssetMember
    {
        /// <summary>
        /// The asset.
        /// </summary>
        public Asset Asset;

        /// <summary>
        /// The path to the member in the asset.
        /// </summary>
        public MemberPath MemberPath;
    }
}
