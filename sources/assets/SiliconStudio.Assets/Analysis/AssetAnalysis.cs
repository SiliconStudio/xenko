// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Assets.Tracking;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Analysis for <see cref="AssetItem"/>.
    /// </summary>
    public sealed class AssetAnalysis
    {
        public static LoggerResult Run(IEnumerable<AssetItem> items, AssetAnalysisParameters parameters)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var result = new LoggerResult();
            Run(items, result, parameters);
            return result;
        }

        public static void Run(IEnumerable<AssetItem> items, ILogger log, AssetAnalysisParameters parameters)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            foreach (var assetItem in items)
            {
                Run(assetItem, log, parameters);
            }
        }

        public static LoggerResult FixAssetReferences(IEnumerable<AssetItem> items)
        {
            var parameters = new AssetAnalysisParameters() { IsProcessingAssetReferences = true, IsLoggingAssetNotFoundAsError =  true};
            var result = new LoggerResult();
            Run(items, result, parameters);
            return result;
        }

        public static void Run(AssetItem assetItem, ILogger log, AssetAnalysisParameters parameters)
        {
            if (assetItem == null) throw new ArgumentNullException(nameof(assetItem));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (assetItem.Package == null)
            {
                throw new InvalidOperationException("AssetItem must belong to an existing package");
            }

            var package = assetItem.Package;

            // Check that there is no duplicate in assets
            if (package.Session != null)
            {
                var packages = package.FindDependencies();

                foreach (var otherPackage in packages)
                {
                    var existingAsset = otherPackage.Assets.Find(assetItem.Id);

                    if (existingAsset != null)
                    {
                        log.Error("Assets [{0}] with id [{1}] from Package [{2}] is already loaded from package [{3}]", existingAsset.FullPath, existingAsset.Id, package.FullPath, existingAsset.Package.FullPath);
                    }
                    else
                    {
                        existingAsset = otherPackage.Assets.Find(assetItem.Location);
                        if (existingAsset != null)
                        {
                            log.Error("Assets [{0}] with location [{1}] from Package [{2}] is already loaded from package [{3}]", existingAsset.FullPath, existingAsset.Location, package.FullPath, existingAsset.Package.FullPath);
                        }
                    }
                }
            }

            var assetReferences = AssetReferenceAnalysis.Visit(assetItem.Asset);

            if (package.Session != null && parameters.IsProcessingAssetReferences)
            {
                UpdateAssetReferences(assetItem, assetReferences, log, parameters);
            }
            // Update paths for asset items

            if (parameters.IsProcessingUPaths)
            {
                // Find where this asset item was previously stored (in a different package for example)
                CommonAnalysis.UpdatePaths(assetItem, assetReferences.Where(link => link.Reference is UPath), parameters);
                // Source hashes are not processed by analysis, we need to manually indicate them to update
                SourceHashesHelper.UpdateUPaths(assetItem.Asset, assetItem.FullPath.GetParent(), parameters.ConvertUPathTo);
            }
        }

        internal static void UpdateAssetReferences(AssetItem assetItem, IEnumerable<AssetReferenceLink> assetReferences, ILogger log, AssetAnalysisParameters parameters)
        {
            var package = assetItem.Package;

            // Update reference
            foreach (var assetReferenceLink in assetReferences.Where(link => link.Reference is IReference))
            {
                var contentReference = (IReference)assetReferenceLink.Reference;
                // If the content reference is an asset base that is in fact a root import, just skip it
                if ((contentReference is AssetBase) && ((AssetBase)contentReference).IsRootImport)
                {
                    continue;
                }

                // Update Asset references (AssetReference, AssetBase, reference)
                var id = contentReference.Id;
                var newItemReference = package.FindAsset(id);

                // If asset was not found by id try to find by its location
                if (newItemReference == null)
                {
                    newItemReference = package.FindAsset(contentReference.Location);
                    if (newItemReference != null)
                    {
                        // If asset was found by its location, just emit a warning
                        log.Warning(package, contentReference, AssetMessageCode.AssetReferenceChanged, contentReference, newItemReference.Id);
                    }
                }

                // If asset was not found, display an error or a warning
                if (newItemReference == null)
                {
                    if (parameters.IsLoggingAssetNotFoundAsError)
                    {
                        log.Error(package, contentReference, AssetMessageCode.AssetNotFound, contentReference);
                    }
                    else
                    {
                        log.Warning(package, contentReference, AssetMessageCode.AssetNotFound, contentReference);
                    }
                    continue;
                }

                // Only update location that are actually different
                var newLocationWithoutExtension = newItemReference.Location;
                if (newLocationWithoutExtension != contentReference.Location || newItemReference.Id != contentReference.Id)
                {
                    assetReferenceLink.UpdateReference(newItemReference.Id, newLocationWithoutExtension);
                    assetItem.IsDirty = true;
                }
            }
        }
    }
}
