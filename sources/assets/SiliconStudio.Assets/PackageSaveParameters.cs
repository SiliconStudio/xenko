// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    public class PackageSaveParameters
    {
        private static readonly PackageSaveParameters DefaultParameters = new PackageSaveParameters();

        public static PackageSaveParameters Default()
        {
            return DefaultParameters.Clone();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PackageLoadParameters.</returns>
        public PackageSaveParameters Clone()
        {
            return (PackageSaveParameters)MemberwiseClone();
        }

        public Func<AssetItem, bool> AssetFilter;
    }
}
