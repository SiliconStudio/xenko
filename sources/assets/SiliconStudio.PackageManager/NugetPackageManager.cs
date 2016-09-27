// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageManager : IPackageManager
    {
        private NuGet.PackageManager _manager;

        public NugetPackageManager(NuGet.PackageManager manager)
        {
            _manager = manager;
            _manager.PackageInstalled += (sender, args) => NugetPackageInstalled?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            _manager.PackageInstalling += (sender, args) => NugetPackageInstalling?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            _manager.PackageUninstalled += (sender, args) => NugetPackageUninstalled?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            _manager.PackageUninstalling += (sender, args) => NugetPackageUninstalling?.Invoke(sender, new NugetPackageOperationEventArgs(args));
        }

        public DependencyVersion DependencyVersion
        {
            get
            {
                return _manager.DependencyVersion;
            }

            set
            {
                _manager.DependencyVersion = value;
            }
        }

        public IFileSystem FileSystem
        {
            get
            {
                return _manager.FileSystem;
            }

            set
            {
                _manager.FileSystem = value;
            }
        }

        public IPackageRepository LocalRepository
        {
            get
            {
                return _manager.LocalRepository;
            }
        }

        public ILogger Logger
        {
            get
            {
                return _manager.Logger;
            }

            set
            {
                _manager.Logger = value;
            }
        }

        public IPackagePathResolver PathResolver
        {
            get
            {
                return _manager.PathResolver;
            }
        }

        public IPackageRepository SourceRepository
        {
            get
            {
                return _manager.SourceRepository;
            }
        }

        public bool WhatIf
        {
            get
            {
                return _manager.WhatIf;
            }

            set
            {
                _manager.WhatIf = value;
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageInstalled;
        public event EventHandler<PackageOperationEventArgs> PackageInstalling;
        public event EventHandler<PackageOperationEventArgs> PackageUninstalled;
        public event EventHandler<PackageOperationEventArgs> PackageUninstalling;

        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageInstalled;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageInstalling;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageUninstalled;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageUninstalling;

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            _manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions);
        }

        public IEnumerable<NugetPackage> GetUpdates(NugetPackageName[] nugetPackageName, bool includePrerelease, bool includeAllVersions)
        {
            var names = new PackageName[nugetPackageName.Length];
            for (int i = 0; i < nugetPackageName.Length; i++)
            {
                names[i] = nugetPackageName[i].Name;
            }
            var list = SourceRepository.GetUpdates(names, includePrerelease, includeAllVersions);
            var res = new List<NugetPackage>();
            foreach (var package in list)
            {
                res.Add(new NugetPackage(package)); 
            }
            return res;
        }

        public void InstallPackage(string packageId, SemanticVersion version, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            _manager.InstallPackage(packageId, version, ignoreDependencies, allowPrereleaseVersions);
        }

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions, bool ignoreWalkInfo)
        {
            _manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions, ignoreWalkInfo);
        }

        public void UninstallPackage(IPackage package, bool forceRemove, bool removeDependencies)
        {
            _manager.UninstallPackage(package, forceRemove, removeDependencies);
        }

        private IEnumerable<NugetPackage> ToNugetPackages(IEnumerable<IPackage> packages)
        {
            var res = new List<NugetPackage>();
            foreach (var package in packages)
            {
                res.Add(new NugetPackage(package)); 
            }
            return res;
        }

        public IEnumerable<NugetPackage> FindLocalPackagesById(string packageId)
        {
            return ToNugetPackages(LocalRepository.FindPackagesById(packageId));
        }
        public IEnumerable<NugetPackage> FindSourcePackagesById(string packageId)
        {
            return ToNugetPackages(SourceRepository.FindPackagesById(packageId));
        }
        public IEnumerable<NugetPackage> FindSourcePackages(IReadOnlyCollection<string> packageIds)
        {
            return ToNugetPackages(SourceRepository.FindPackages(packageIds));
        }

        public IEnumerable<NugetPackage> FindLocalPackages(IReadOnlyCollection<string> packageIds)
        {
            return ToNugetPackages(LocalRepository.FindPackages(packageIds));
        }

        public void UninstallPackage(string packageId, SemanticVersion version, bool forceRemove, bool removeDependencies)
        {
            _manager.UninstallPackage(packageId, version, forceRemove, removeDependencies);
        }

        public void UpdatePackage(IPackage newPackage, bool updateDependencies, bool allowPrereleaseVersions)
        {
            _manager.UpdatePackage(newPackage, updateDependencies, allowPrereleaseVersions);
        }

        public void UpdatePackage(string packageId, IVersionSpec versionSpec, bool updateDependencies, bool allowPrereleaseVersions)
        {
            _manager.UpdatePackage(packageId, versionSpec, updateDependencies, allowPrereleaseVersions);
        }

        public void UpdatePackage(string packageId, SemanticVersion version, bool updateDependencies, bool allowPrereleaseVersions)
        {
            _manager.UpdatePackage(packageId, version, updateDependencies, allowPrereleaseVersions);
        }

        internal void UninstallPackage(IPackage package)
        {
            _manager.UninstallPackage(package);
        }

    }
}
