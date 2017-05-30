// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// An enum representing the user answer to a package upgrade request.
    /// </summary>
    public enum PackageUpgradeRequestedAnswer
    {
        /// <summary>
        /// The related package should be upgraded.
        /// </summary>
        Upgrade,
        /// <summary>
        /// The related package and all following packages should be upgraded.
        /// </summary>
        UpgradeAll,
        /// <summary>
        /// The related package should not be upgraded.
        /// </summary>
        DoNotUpgrade,
        /// <summary>
        /// The related package and all following packages should not be upgraded.
        /// </summary>
        DoNotUpgradeAny,
    }
}
