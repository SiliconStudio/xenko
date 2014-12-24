// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// The class in charge of updating the assets inter-dependent
    /// </summary>
    public class AssetUpdater
    {
        private AssetDependencyManager dependencyManager;

        public AssetUpdater(AssetDependencyManager dependencyManager)
        {
            if (dependencyManager == null) throw new ArgumentNullException("dependencyManager");

            this.dependencyManager = dependencyManager;
        }

        /// <summary>
        /// Gets or sets the asset dependency manager
        /// </summary>
        public AssetDependencyManager DependencyManager
        {
            get { return dependencyManager; }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");

                dependencyManager = value;
            }
        }

        /// <summary>
        /// Indicate if the referred member can be modified or not due to sealing.
        /// </summary>
        /// <param name="asset">The asset to modify</param>
        /// <param name="path">The path to the member to modify</param>
        /// <returns><value>true</value> if it can be modified</returns>
        public bool CanBeModified(Asset asset, MemberPath path)
        {
            if (path.GetNodeOverrides(asset).Any(x => x.IsNew()))
                return true;

            var assetBase = asset.Base;
            while (assetBase != null && assetBase.Asset != null)
            {
                var parent = assetBase.Asset;
                var parentPath = path.Resolve(asset, parent).FirstOrDefault(); // if several paths exist in parent, they should be all equal (same object instance)
                if (parentPath == null)
                    break;

                var parentOverrides = parentPath.GetNodeOverrides(parent).ToList();
                if (parentOverrides.LastOrDefault().IsSealed())
                    return false;

                if (parentOverrides.Any(x => x.IsNew()))
                    break;

                assetBase = parent.Base;
            }

            return true;
        }

        /// <summary>
        /// Indicate if the referred member can be modified or not due to sealing.
        /// </summary>
        /// <param name="assetMember">The asset member to modify</param>
        /// <returns><value>true</value> if it can be modified</returns>
        public bool CanBeModified(AssetMember assetMember)
        {
            return CanBeModified(assetMember.Asset, assetMember.MemberPath);
        }

        /// <summary>
        /// Reset the asset member to its base value.
        /// </summary>
        /// <param name="asset">The asset to reset</param>
        /// <param name="path">The path to the member to reset</param>
        /// <returns>The list of inheriting members that should to be recursively reset</returns>
        public IEnumerable<AssetMember> Reset(Asset asset, MemberPath path)
        {
            return Enumerable.Empty<AssetMember>();
        }

        /// <summary>
        /// Reset the asset member to its base value.
        /// </summary>
        /// <param name="assetMember">The asset member to reset</param>
        /// <returns>The list of inheriting members that should to be recursively reset</returns>
        public IEnumerable<AssetMember> Reset(AssetMember assetMember)
        {
            return Reset(assetMember.Asset, assetMember.MemberPath);
        }

        /// <summary>
        /// Seal the asset member.
        /// </summary>
        /// <param name="asset">The asset to seal</param>
        /// <param name="path">The path to the member to reset</param>
        /// <param name="isRecursive">Indicate if the seal is recursive or not</param>
        /// <returns>The list of inheriting members that should to be reset</returns>
        public IEnumerable<AssetMember> Seal(Asset asset, MemberPath path, bool isRecursive)
        {
            return Enumerable.Empty<AssetMember>();
        }

        /// <summary>
        /// Seal the asset member.
        /// </summary>
        /// <param name="assetMember">The asset member to seal</param>
        /// <param name="isRecursive">Indicate if the seal is recursive or not</param>
        /// <returns>The list of inheriting members that should to be reset</returns>
        public IEnumerable<AssetMember> Seal(AssetMember assetMember, bool isRecursive)
        {
            return Seal(assetMember.Asset, assetMember.MemberPath, isRecursive);
        }

        /// <summary>
        /// Set the value of an the asset member.
        /// </summary>
        /// <param name="asset">The asset to set</param>
        /// <param name="path">The path to the member to set</param>
        /// <param name="action">The action to perform on the member</param>
        /// <param name="value">The value to set to the member</param>
        /// <returns>The list of inheriting members that should to be reset</returns>
        public IEnumerable<AssetMember> Set(Asset asset, MemberPath path, MemberPathAction action, object value)
        {
            return Enumerable.Empty<AssetMember>();
        }

        /// <summary>
        /// Set the asset member.
        /// </summary>
        /// <param name="assetMember">The asset member to set</param>
        /// <param name="action">The action to perform on the member</param>
        /// <param name="value">The value to set to the member</param>
        /// <returns>The list of inheriting members that should to be reset</returns>
        public IEnumerable<AssetMember> Set(AssetMember assetMember, MemberPathAction action, object value)
        {
            return Set(assetMember.Asset, assetMember.MemberPath, action, value);
        }
    }
}