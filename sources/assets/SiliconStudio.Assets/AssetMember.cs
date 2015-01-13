// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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