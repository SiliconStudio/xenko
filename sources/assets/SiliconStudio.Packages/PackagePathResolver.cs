// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for detail

using System;
using System.IO;
using NuGet;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Packages
{
    public class PackagePathResolver : IPackagePathResolver
    {
        /// <summary>
        /// Location where Packages are installed.
        /// </summary>
        private readonly IFileSystem fileSystem;

        public PackagePathResolver(IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc cref="IPackagePathResolver"/>
        [NotNull]
        public virtual string GetInstallPath(IPackage package)
        {
            return Path.Combine(fileSystem.Root, GetPackageDirectory(package));
        }

        /// <inheritdoc cref="IPackagePathResolver"/>
        [NotNull]
        public virtual string GetPackageDirectory(IPackage package)
        {
            return GetPackageDirectory(package.Id, package.Version);
        }

        /// <inheritdoc cref="IPackagePathResolver"/>
        [NotNull]
        public virtual string GetPackageFileName(IPackage package)
        {
            return GetPackageFileName(package.Id, package.Version);
        }

        /// <inheritdoc cref="IPackagePathResolver"/>
        [NotNull]
        public virtual string GetPackageDirectory(string packageId, SemanticVersion version)
        {
            var directory = packageId;
            directory += "." + version.ToNormalizedString();
            return directory;
        }

        /// <inheritdoc cref="IPackagePathResolver"/>
        [NotNull]
        public virtual string GetPackageFileName(string packageId, SemanticVersion version)
        {
            var fileNameBase = packageId;
            fileNameBase += "." + version.ToNormalizedString();
            return fileNameBase + PackageConstants.PackageExtension;
        }
    }
}
