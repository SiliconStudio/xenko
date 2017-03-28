// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Helper class that contains methods to retrieve and manipulate SDK locations.
    /// </summary>
    public static class DirectoryHelper
    {
        private const string CommonTargets = @"Targets\SiliconStudio.Common.targets";
        private const string XenkoSolution = @"build\Xenko.sln";
        private static string packageDirectoryOverride;

        /// <summary>
        /// If not null, the location where to find the package directory and the installation directory, overriding the default locations.
        /// It can only be set once.
        /// </summary>
        public static string PackageDirectoryOverride {
            get
            {
                return packageDirectoryOverride;
            }
            set
            {
                if (packageDirectoryOverride != null) throw new NotSupportedException("Cannot set more than once the directory override!");
                packageDirectoryOverride = value;
            }
        }

        /// <summary>
        /// Gets the directory of the package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded.
        /// </summary>
        /// <param name="packageName">The name of the expected package.</param>
        /// <returns>A string representing the path of the package directory.</returns>
        /// <exception cref="InvalidOperationException">The package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded does not match the <paramref name="packageName"/>.</exception>
        public static string GetPackageDirectory(string packageName)
        {
            if (PackageDirectoryOverride != null)
                return PackageDirectoryOverride;
            
            var appDomain = AppDomain.CurrentDomain;
            var baseDirectory = new DirectoryInfo(appDomain.BaseDirectory);
            var defaultPackageDirectoryTemp = baseDirectory.Parent?.Parent;
            // If we have a root directory, then store it as the default package directory
            if ((defaultPackageDirectoryTemp != null) && IsPackageDirectory(defaultPackageDirectoryTemp.FullName, packageName))
            {
                return defaultPackageDirectoryTemp.FullName;
            }

            throw new InvalidOperationException($"The current AppDomain.BaseDirectory [{baseDirectory}] is not part of the package [{packageName}]");
        }

        /// <summary>
        /// Gets the installation directory from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded.
        /// </summary>
        /// <param name="packageName">The name of the package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded..</param>
        /// <returns>A string representing the path of the package directory.</returns>
        /// <remarks>When executing from a development build, this method returns the root directory of the repository.</remarks>
        /// <exception cref="InvalidOperationException">The package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded does not match the <paramref name="packageName"/>.</exception>
        public static string GetInstallationDirectory(string packageName)
        {
            var packageDirectory = GetPackageDirectory(packageName);
            if (packageDirectory == null)
                return null;

            var packageDirectoryInfo = new DirectoryInfo(packageDirectory);
            var installationDirectoryTemp = packageDirectoryInfo.Parent?.Parent;
            // Check if we have a regular distribution
            if ((installationDirectoryTemp != null) && IsInstallationDirectory(installationDirectoryTemp.FullName))
            {
                return installationDirectoryTemp.FullName;
            }
            if (IsInstallationDirectory(packageDirectoryInfo.FullName))
            {
                // we have a dev distribution
                return packageDirectory;
            }
            return null;
        }

        /// <summary>
        /// Gets the path to the file corresponding to the given package name in the given directory.
        /// </summary>
        /// <param name="directory">The directory where the package file is located.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <returns>The path to the file corresponding to the given package name in the given directory.</returns>
        public static string GetPackageFile(string directory, string packageName)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            return Path.Combine(directory, packageName + Package.PackageFileExtension);
        }

        /// <summary>
        /// Indicates whether the given directory is the installation directory.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns><c>True</c> if the given directory is the installation directory, <c>false</c> otherwise.</returns>
        public static bool IsInstallationDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            var commonTargets = GetCommonTargets(directory);
            return File.Exists(commonTargets);
        }

        /// <summary>
        /// Indicates whether the given directory is the root directory of the repository, when executing from a development build. 
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns><c>True</c> if the given directory is the root directory of the repository, <c>false</c> otherwise.</returns>
        public static bool IsRootDevDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            var xenkoSolution = Path.Combine(directory, XenkoSolution);
            return File.Exists(xenkoSolution);
        }

        private static string GetCommonTargets(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            return Path.Combine(directory, CommonTargets);
        }

        private static bool IsPackageDirectory(string directory, string packageName)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            var packageFile = GetPackageFile(directory, packageName);
            return File.Exists(packageFile);
        }
    }
}