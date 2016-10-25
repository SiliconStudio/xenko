// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageOperationEventArgs
    {
        private readonly PackageOperationEventArgs _args;

        /// <summary>
        /// Initialize a new instance of <see cref="PackageOperationEventArgs"/> using the corresponding NuGet abstraction.
        /// </summary>
        /// <param name="args">NuGet arguments to use to initialize new instance.</param>
        internal NugetPackageOperationEventArgs(PackageOperationEventArgs args)
        {
            _args = args;
        }

        /// <summary>
        /// Id of the package.
        /// </summary>
        public string Id => _args.Package.Id;

        /// <summary>
        /// Location where package is installed.
        /// </summary>
        public string InstallPath => _args.InstallPath;
    }
}
