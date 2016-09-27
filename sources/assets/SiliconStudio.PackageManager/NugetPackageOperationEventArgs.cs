// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageOperationEventArgs
    {
        private readonly PackageOperationEventArgs _args;

        internal NugetPackageOperationEventArgs(PackageOperationEventArgs args)
        {
            _args = args;
        }

        public string InstallPath => _args.InstallPath;

        public NugetPackage Package => new NugetPackage(_args.Package);
    }
}
