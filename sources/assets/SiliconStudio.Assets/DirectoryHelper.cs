// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Helper class that contains methods to retrieve and manipulate SDK locations.
    /// </summary>
    public static class DirectoryHelper
    {
        private const string CommonTargets = @"Targets\SiliconStudio.Common.targets";
        private const string XenkoSolution = @"build\Xenko.sln";

        public static string packageDirectoryOverride;

        /// <summary>
        /// Gets the directory of the package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded.
        /// </summary>
        /// <param name="packageName">The name of the expected package.</param>
        /// <returns>A string representing the path of the package directory, or null if the <see cref="SiliconStudio.Assets"/> assembly has been loaded from memory.</returns>
        /// <exception cref="InvalidOperationException">The package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded does not match the <paramref name="packageName"/>.</exception>
        public static string GetPackageDirectory(string packageName)
        {
            if (packageDirectoryOverride != null)
                return packageDirectoryOverride;


            var appDomain = AppDomain.CurrentDomain;
            var binDirectory = new DirectoryInfo(appDomain.BaseDirectory);
            if (binDirectory.Parent != null && binDirectory.Parent.Parent != null)
            {
                var defaultPackageDirectoryTemp = binDirectory.Parent.Parent;

                // If we have a root directory, then store it as the default package directory
                if (!IsPackageDirectory(defaultPackageDirectoryTemp.FullName, packageName))
                {
                    throw new InvalidOperationException($"The current AppDomain.BaseDirectory [{binDirectory}] is not part of the package [{packageName}]");
                }
                return defaultPackageDirectoryTemp.FullName;
            }
            return null;
        }

        /// <summary>
        /// Gets the installation directory from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded.
        /// </summary>
        /// <param name="packageName">The name of the package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded..</param>
        /// <returns>A string representing the path of the package directory, or null if the <see cref="SiliconStudio.Assets"/> assembly has been loaded from memory.</returns>
        /// <remarks>When executing from a development build, this method returns the root directory of the repository.</remarks>
        /// <exception cref="InvalidOperationException">The package from which the <see cref="SiliconStudio.Assets"/> assembly has been loaded does not match the <paramref name="packageName"/>.</exception>
        public static string GetInstallationDirectory(string packageName)
        {
            var packageDirectory = GetPackageDirectory(packageName);
            if (packageDirectory == null)
                return null;

            var packageDirectoryInfo = new DirectoryInfo(packageDirectory);
            // Check if we have a regular distribution
            if (packageDirectoryInfo.Parent != null && packageDirectoryInfo.Parent.Parent != null && IsInstallationDirectory(packageDirectoryInfo.Parent.Parent.FullName))
            {
                return packageDirectoryInfo.Parent.Parent.FullName;
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
            if (directory == null) throw new ArgumentNullException("directory");
            return Path.Combine(directory, packageName + Package.PackageFileExtension);
        }

        /// <summary>
        /// Indicates whether the given directory is the installation directory.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns><c>True</c> if the given directory is the installation directory, <c>false</c> otherwise.</returns>
        public static bool IsInstallationDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
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
            if (directory == null) throw new ArgumentNullException("directory");
            var xenkoSolution = Path.Combine(directory, XenkoSolution);
            return File.Exists(xenkoSolution);
        }

        private static string GetCommonTargets(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            return Path.Combine(directory, CommonTargets);
        }

        private static bool IsPackageDirectory(string directory, string packageName)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            var packageFile = GetPackageFile(directory, packageName);
            return File.Exists(packageFile);
        }
    }
}