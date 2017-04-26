// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Packages
{
    public class PackageOperationEventArgs
    {
        private readonly NuGet.PackageOperationEventArgs args;

        /// <summary>
        /// Initialize a new instance of <see cref="NuGet.PackageOperationEventArgs"/> using the corresponding NuGet abstraction.
        /// </summary>
        /// <param name="args">NuGet arguments to use to initialize new instance.</param>
        internal PackageOperationEventArgs(NuGet.PackageOperationEventArgs args)
        {
            this.args = args;
        }

        /// <summary>
        /// Id of the package.
        /// </summary>
        public string Id => args.Package.Id;

        /// <summary>
        /// Location where package is installed.
        /// </summary>
        public string InstallPath => args.InstallPath;
    }
}
