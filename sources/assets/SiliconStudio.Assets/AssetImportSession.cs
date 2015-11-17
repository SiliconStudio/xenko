// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// This class is handling importItem of assets into a session. See remarks for usage.
    /// </summary>
    /// <remarks>
    /// <code>
    ///  var importSession = new AssetImportSession(session);
    ///  // First add files to the session
    ///  importSession.AddFile("C:\xxx\yyy\test.fbx", package, "zzz");
    /// 
    ///  // accessing importSession.Imports will return the list of files to importItem
    /// 
    ///  // Prepare files for importItem.
    ///  importSession.Stage();
    /// 
    ///  // importSession.Imports.Items contains the list of items that will be imported
    /// 
    ///  // Here we need to select the assets that will be used for merge if there are any
    ///  foreach(var fileItem in importSession.Imports)
    ///  {
    ///      foreach(var importItemByImporter in importSession.ByImporters)
    ///      {
    ///          foreach(var importItem in importItemByImporter.Items)
    ///          { 
    ///              // Select for example the first mergeable item
    ///              importItem.SelectedItem = (importItem.Merges.Count > 0) ? importItem.Merges[0].PreviousItem : importItem.Item;
    ///          }
    ///      }
    ///  }
    /// 
    ///  // Merge assets if necessary
    ///  importSession.Merge();
    /// 
    ///  // Import all assets
    ///  importSession.Import();
    /// </code>
    /// </remarks>
    public sealed class AssetImportSession
    {
        private readonly PackageSession session;
        private readonly List<AssetToImport> imports; 

        // Associate source assets (fbx...etc.) to asset being imported
        private readonly Dictionary<AssetLocationTyped, HashSet<AssetItem>> sourceFileToAssets = new Dictionary<AssetLocationTyped, HashSet<AssetItem>>();

        /// <summary>
        /// Occurs when this import session is making progress.
        /// </summary>
        public event EventHandler<AssetImportSessionEvent> Progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetImportSession"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException">session</exception>
        public AssetImportSession(PackageSession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.session = session;
            imports = new List<AssetToImport>();
        }

        /// <summary>
        /// Gets the list of import being processed by this instance.
        /// </summary>
        /// <value>The imports.</value>
        public List<AssetToImport> Imports
        {
            get
            {
                return imports;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public bool HasErrors
        {
            get
            {
                return Imports.SelectMany(import => import.ByImporters).Any(step => step.HasErrors);
            }
        }

        /// <summary>
        /// Determines whether the specified file is supported
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if the specified file is supported; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">file</exception>
        public bool IsFileSupported(UFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!file.IsAbsolute) return false;
            if (file.GetFileExtension() == null) return false;
            if (!File.Exists(file)) return false;

            return true;
        }

        /// <summary>
        /// Adds a file to import.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="package">The package where to import this file.</param>
        /// <param name="directory">The directory relative to package where to import this file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// file
        /// or
        /// package
        /// or
        /// directory
        /// </exception>
        /// <exception cref="System.ArgumentException">File [{0}] does not exist or is not an absolute path.ToFormat(file)</exception>
        public AssetToImport AddFile(UFile file, Package package, UDirectory directory)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (package == null) throw new ArgumentNullException("package");
            if (directory == null) throw new ArgumentNullException("directory");
            if (!IsFileSupported(file))
            {
                throw new ArgumentException("File [{0}] does not exist or is not an absolute path".ToFormat(file));
            }

            // Sort by importer display rank
            var importerList = AssetRegistry.FindImporterForFile(file).ToList().OrderBy(value => value.Order).ToList();

            AssetToImport assetToImport = null;
            foreach (var importer in importerList)
            {
                assetToImport = AddFile(file, importer, package, directory);
            }
            return assetToImport;
        }

        /// <summary>
        /// Adds files to import.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="package">The package where to import this file.</param>
        /// <param name="directory">The directory relative to package where to import this file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// files
        /// or
        /// package
        /// or
        /// directory
        /// </exception>
        public IEnumerable<AssetToImport> AddFiles(IEnumerable<UFile> files, Package package, UDirectory directory)
        {
            if (files == null) throw new ArgumentNullException("files");
            if (package == null) throw new ArgumentNullException("package");
            if (directory == null) throw new ArgumentNullException("directory");
            var result = new List<AssetToImport>();
            foreach (var file in files)
            {
                var assetToImport = AddFile(file, package, directory);
                if (assetToImport != null && !result.Contains(assetToImport))
                    result.Add(assetToImport);
            }
            return result;
        }

        /// <summary>
        /// Adds a file to import.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="importer">The associated importer to this file.</param>
        /// <param name="package">The package where to import this file.</param>
        /// <param name="directory">The directory relative to package where to import this file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// file
        /// or
        /// importer
        /// or
        /// package
        /// or
        /// directory
        /// or
        /// importer [{0}] is not supporting file [{1}].ToFormat(importer.Name, file)
        /// </exception>
        /// <exception cref="System.ArgumentException">File [{0}] does not exist or is not an absolute path.ToFormat(file)</exception>
        /// <exception cref="System.InvalidOperationException">Current session does not contain package</exception>
        public AssetToImport AddFile(UFile file, IAssetImporter importer, Package package, UDirectory directory)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (importer == null) throw new ArgumentNullException("importer");
            if (package == null) throw new ArgumentNullException("package");
            if (directory == null) throw new ArgumentNullException("directory");

            if (!IsFileSupported(file))
            {
                throw new ArgumentException("File [{0}] does not exist or is not an absolute path".ToFormat(file));
            }

            if (!importer.IsSupportingFile(file)) throw new ArgumentNullException("importer [{0}] is not supporting file [{1}]".ToFormat(importer.Name, file));

            if (!session.Packages.Contains(package))
            {
                throw new InvalidOperationException("Current session does not contain package");
            }

            return RegisterImporter(file, package, directory, importer);
        }

        /// <summary>
        /// Determines whether the specified asset is supporting re-import.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns><c>true</c> if the specified asset is supporting re-import; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">assetItem</exception>
        public bool IsExistingAssetForReImportSupported(AssetItem assetItem)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            if (assetItem.Package == null) return false;
            if (assetItem.Package.Session != session) return false;

            var asset = assetItem.Asset as AssetImportTracked;
            if (asset == null) return false;
            if (asset.Base == null) return false;
            if (asset.Source == null) return false;

            var baseAsset = asset.Base.Asset as AssetImportTracked;
            if (baseAsset == null) return false;

            if (baseAsset.ImporterId.HasValue)
            {
                var importer = AssetRegistry.FindImporterById(baseAsset.ImporterId.Value);
                if (importer == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds an existing asset for reimport
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <exception cref="System.ArgumentNullException">assetItem</exception>
        public AssetToImport AddExistingAssetForReImport(AssetItem assetItem)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            if (assetItem.Package == null) throw new ArgumentException("AssetItem is not attached to a package");
            if (assetItem.Package.Session != session) throw new ArgumentException("AssetItem is not attached to the same session of this importItem");

            var asset = assetItem.Asset as AssetImportTracked;
            if (asset == null) throw new ArgumentException("The asset is not an existing importable asset");
            if (asset.Base == null) throw new ArgumentException("The asset to importItem must have a base to reimport");
            if (asset.Source == null) throw new ArgumentException("The asset to importItem has no source/location to an existing raw asset");

            var baseAsset = asset.Base.Asset as AssetImportTracked;
            if (baseAsset == null) throw new ArgumentException("The base asset to importItem is invalid");

            IAssetImporter importer = null;
            // Try to use the previous importer if it exists
            if (baseAsset.ImporterId.HasValue)
            {
                importer = AssetRegistry.FindImporterById(baseAsset.ImporterId.Value);
            }

            // If not, take the first default importer
            if (importer == null)
            {
                importer = AssetRegistry.FindImporterForFile(asset.Source).FirstOrDefault();
            }

            if (importer == null)
            {
                throw new ArgumentException("No importer found for this asset item");
            }

            return RegisterImporter(asset.Source, assetItem.Package, assetItem.Location.GetDirectory(), importer, assetItem);
        }

        /// <summary>
        /// Analyze files for preparing them for the merge and import steps. This must be called first 
        /// after calling <see cref="AddFile(SiliconStudio.Core.IO.UFile,Package,SiliconStudio.Core.IO.UDirectory)"/> methods.
        /// </summary>
        /// <returns><c>true</c> if staging was successful otherwise. See remarks for checking errors</returns>
        /// <remarks>
        /// If this method returns false, errors should be checked on each <see cref="AssetToImport"/> from the <see cref="Imports"/> list.
        /// </remarks>
        public bool Stage(CancellationToken? cancelToken = null)
        {
            bool isImportOk = true;
            var ids = new HashSet<Guid>();

            // Clear previously created items
            foreach (var fileToImport in Imports)
            {
                foreach (var toImportByImporter in fileToImport.ByImporters)
                {
                    toImportByImporter.Items.Clear();
                    toImportByImporter.Log.Clear();
                }
            }

            // Process files to import
            foreach (var fileToImport in Imports)
            {
                if (!fileToImport.Enabled)
                {
                    continue;
                }

                foreach (var toImportByImporter in fileToImport.ByImporters)
                {
                    // Skip importers that are not activated or for which output types are not selected
                    if (!toImportByImporter.Enabled || !toImportByImporter.ImporterParameters.HasSelectedOutputTypes)
                    {
                        continue;
                    }

                    if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                    {
                        toImportByImporter.Log.Warning("Cancellation requested before importing asset [{0}] with importer [{1}]", fileToImport.File, toImportByImporter.Importer.Name);
                        return false;
                    }

                    OnProgress(AssetImportSessionEventType.Begin, AssetImportSessionStepType.Staging, toImportByImporter);
                    try
                    {
                        // Call the importer. For some assts, this operation can take some time
                        var itemsToImport = toImportByImporter.Importer.Import(fileToImport.File, toImportByImporter.ImporterParameters).ToList();
                        if (!CheckAssetsToImport(itemsToImport, ids))
                        {
                            toImportByImporter.Log.Warning("Unsuccessfully processed file [{0}]. Expecting at least one [AssetImport] while importing assets to a package", fileToImport.File);
                            isImportOk = false;
                        }
                        else
                        {
                            foreach (var itemToImport in itemsToImport)
                            {
                                if (itemToImport == null)
                                    continue;

                                itemToImport.SourceFolder = fileToImport.Package.GetDefaultAssetFolder();

                                var assetToImport = new AssetToImportMergeGroup(toImportByImporter, itemToImport);
                                toImportByImporter.Items.Add(assetToImport);
                            }

                            toImportByImporter.Log.Info("Successfully processed file [{0}]", fileToImport.File); 
                        }
                    }
                    catch (Exception ex)
                    {
                        toImportByImporter.Log.Error("Unexpected exception while importing file [{0}]", ex, fileToImport.File);
                        isImportOk = false;
                    }
                    finally
                    {
                        OnProgress(AssetImportSessionEventType.End, AssetImportSessionStepType.Staging, toImportByImporter);
                    }
                }
            }

            // Compute hash for new assets
            ComputeAssetHash(cancelToken);

            // Prepare assets for merge if any
            PrepareMerge(cancelToken);

            // Fix asset locations after prepare for merge
            FixAssetLocations();

            return isImportOk;
        }

        /// <summary>
        /// Merges each asset with the selected asset specified in <see cref="AssetToImportMergeGroup.SelectedItem"/>
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        public void Merge(CancellationToken? cancelToken = null)
        {
            var idRemapping = new Dictionary<Guid, AssetItem>();

            // Generates a list of remapping, between the original asset that we are trying to import
            // and the selected asset we are going to merge into.
            foreach (var fileToImport in Imports.Where(it => it.Enabled))
            {
                foreach (var toImportByImporter in fileToImport.ByImporters.Where(it => it.Enabled))
                {
                    foreach (var toImport in toImportByImporter.Items)
                    {
                        if (toImport.SelectedItem != null)
                        {
                            if (toImport.Item.Id != toImport.SelectedItem.Id)
                            {
                                idRemapping.Add(toImport.Item.Id, toImport.SelectedItem);
                            }
                        }
                        else if (toImport.Enabled)
                        {
                            // By default set selected item from new if nothing selected
                            toImport.SelectedItem = toImport.Item;
                        }
                    }
                }
            }

            if (idRemapping.Count == 0)
            {
                return;
            }

            // Perform the merge for each asset
            foreach (var fileToImport in Imports.Where(it => it.Enabled))
            {
                foreach (var toImportByImporter in fileToImport.ByImporters.Where(it => it.Enabled))
                {
                    foreach (var toImport in toImportByImporter.Items.Where(it => it.Enabled))
                    {
                        // If the asset is not being merged, we still need to fix references to the real asset that will be
                        // imported into the package
                        if (toImport.SelectedItem == null || toImport.Item.Id == toImport.SelectedItem.Id)
                        {
                            FixAssetReferencesToMergeItem(toImport, idRemapping);
                            continue;
                        }

                        var selectedItem = toImport.SelectedItem;
                        var selectedMerge = toImport.Merges.FirstOrDefault(merge => merge.PreviousItem == selectedItem);

                        if (selectedMerge == null)
                        {
                            toImport.Log.Error("Selected item [{0}] does not exist in the merge");
                            continue;
                        }

                        if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                        {
                            toImport.Log.Warning("Cancellation requested before merging asset from [{0}] to location [{1}] ", fileToImport.File, selectedItem.Location);
                            return;
                        }

                        OnProgress(AssetImportSessionEventType.Begin, AssetImportSessionStepType.Merging, toImport);
                        try
                        {
                            MergeAsset(toImport, selectedMerge, idRemapping);
                        }
                        catch (Exception ex)
                        {
                            toImport.Log.Error("Unexpected error while merging asset [{0}]", ex, toImport.Item);
                        }
                        finally
                        {
                            OnProgress(AssetImportSessionEventType.End, AssetImportSessionStepType.Merging, toImport);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fixes asset references for asset being imported but not being merged. See remarks.
        /// </summary>
        /// <param name="toImport">To import.</param>
        /// <param name="idRemapping">The identifier remapping.</param>
        /// <remarks>
        /// As it is possible to re-use existing asset when importing but also at the same time to create new asset,
        /// new need to update references from asset not being merged to assets that are merged back to an existing asset in 
        /// the package.
        /// </remarks>
        private static void FixAssetReferencesToMergeItem(AssetToImportMergeGroup toImport, Dictionary<Guid, AssetItem> idRemapping)
        {
            var asset = (toImport.SelectedItem ?? toImport.Item).Asset;

            // Fix assets references
            var referencesToUpdate = AssetReferenceAnalysis.Visit(asset);
            foreach (var assetReferenceLink in referencesToUpdate)
            {
                var refToUpdate = assetReferenceLink.Reference as IContentReference;
                if (refToUpdate == null || refToUpdate.Id == Guid.Empty || !idRemapping.ContainsKey(refToUpdate.Id))
                {
                    continue;
                }

                var realItem = idRemapping[refToUpdate.Id];
                assetReferenceLink.UpdateReference(realItem.Id, realItem.Location);
            }
        }

        private static void MergeAsset(AssetToImportMergeGroup toImport, AssetToImportMerge selectedMerge, Dictionary<Guid, AssetItem> idRemapping)
        {
            // Perform a full merge which will replace output references if necessary
            var result = AssetMerge.Merge(selectedMerge.Diff, node =>
            {
                // Asset references are a special case while importing, as we are
                // going to try to rematch them, so if they changed, we expect to 
                // use the original instance
                var baseReference = GetContentReference(node.BaseNode?.Instance);
                var asset1Reference = GetContentReference(node.Asset1Node?.Instance);
                var asset2Instance = node.Asset2Node?.Instance;
                var asset2Reference = GetContentReference(asset2Instance);

                var baseFileName = baseReference != null ? new UFile(baseReference.Location).GetFileName() : null;
                var asset1FileName = asset1Reference != null ? new UFile(asset1Reference.Location).GetFileName() : null;
                var asset2FileName = asset2Reference != null ? new UFile(asset2Reference.Location).GetFileName() : null;

                // If we have the same reference between the base and imported item, but reference changes in the current item, keep the current item
                if (baseFileName == asset2FileName && asset1FileName != asset2FileName)
                {
                    return Diff3ChangeType.MergeFromAsset1;
                }

                if (node.Asset2Node != null && (baseReference != null || asset1Reference != null || asset2Reference != null))
                {
                    if (asset2Instance != null && asset2Reference != null)
                    { 
                        AssetItem realItem;
                        // Ids are remapped, so we are going to remap here, both on the
                        // new base and on the merged result
                        if (idRemapping.TryGetValue(asset2Reference.Id, out realItem))
                        {
                            object newReference;
                            if (asset2Instance is AssetReference)
                            {
                                newReference = AssetReference.New(asset2Instance.GetType(), realItem.Id, realItem.Location);
                            }
                            else if (asset2Instance is ContentReference)
                            {
                                newReference = ContentReference.New(asset2Instance.GetType(), realItem.Id, realItem.Location);
                            }
                            else
                            {
                                newReference = AttachedReferenceManager.CreateSerializableVersion(asset2Instance.GetType(), realItem.Id, realItem.Location);
                            }
                            node.ReplaceValue(newReference, diff3Node => diff3Node.Asset2Node);
                        }

                        return Diff3ChangeType.MergeFromAsset2;
                    }

                    if (node.Asset1Node != null)
                    {
                        return Diff3ChangeType.MergeFromAsset1;
                    }
                }

                return AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1(node);
            });

            toImport.MergedResult = result;
            if (result.HasErrors)
            {
                toImport.Log.Error("Error while trying to merge asset [{0}]", toImport.Item);
                result.CopyTo(toImport.Log);
                return;
            }

            var finalAsset = (Asset)result.Asset;
            finalAsset.Base = new AssetBase((Asset)selectedMerge.Diff.Asset2);

            // Set the final item
            toImport.MergedItem = new AssetItem(toImport.SelectedItem.Location, finalAsset) { SourceFolder = toImport.SelectedItem.SourceFolder, SourceProject = toImport.SelectedItem.SourceProject };
        }


        /// <summary>
        /// Imports all assets
        /// </summary>
        /// <returns>Result of the import.</returns>
        public ImportResult Import()
        {
            var result = new ImportResult();
            Import(result);
            return result;
        }

        /// <summary>
        /// Imports all assets
        /// </summary>
        /// <returns>Results of the import where logs will be outputed.</returns>
        public void Import(ImportResult result)
        {
            // Clears the result before appending to it
            result.Clear();

            // Gets the list of assets merged that will be imported
            var allImports = AllImports().ToList();
            var mergedAssets = new HashSet<Guid>();
            foreach (var toImport in allImports)
            {
                if (toImport.MergedItem != null)
                {
                    mergedAssets.Add(toImport.MergedItem.Id);

                    // If there are any errors on the selected merged result, output them in the final log
                    if (toImport.MergedResult.HasErrors)
                    {
                        result.Error("Cannot select a merge asset [{0}] that has the following merge errors:", toImport.MergedItem);
                        toImport.MergedResult.CopyTo(result);
                    }
                }
            }

            // Before importing, we have to fix names
            foreach (var fileToImport in Imports.Where(it => it.Enabled))
            {
                var assetPackage = fileToImport.Package;
                var assetResolver = AssetResolver.FromPackage(assetPackage);

                foreach (var toImportByImporter in fileToImport.ByImporters.Where(it => it.Enabled))
                {
                    // Copy errors from intermediate log to output
                    toImportByImporter.Log.CopyTo(result);

                    // If it has errors, don't try to importItem it
                    if (toImportByImporter.HasErrors)
                    {
                        result.Warning("Unexpected errors while importing source [{0}] with importer [{1}]. Check the details errors log", fileToImport.File, toImportByImporter.Importer.Name);
                    } 

                    foreach (var toImport in toImportByImporter.Items.Where(it => it.Enabled))
                    {
                        // Copy errors from intermediate log to output
                        toImport.Log.CopyTo(result);

                        // If the item is in error, don't try to import it
                        if (toImport.HasErrors)
                        {
                            result.Warning("Unexpected errors while importing asset [{0}/{1}]. Check the details errors log", toImport.Item.Location, toImport.Item.Id);
                            continue;
                        }

                        // If one asset in the group is in error, don't try to import the group
                        if (toImportByImporter.HasErrors)
                        {
                            result.Warning("Disable importing asset [{0}/{1}] while other assets in same group are in errors. Check the details errors log", toImport.Item.Location, toImport.Item.Id);
                            continue;
                        } 

                        OnProgress(AssetImportSessionEventType.Begin, AssetImportSessionStepType.Importing, toImport);
                        try
                        {
                            AssetItem item;

                            // If there is an asset merged, use it directly
                            if (toImport.MergedItem != null)
                            {
                                item = toImport.MergedItem;

                                var existingItem = session.FindAsset(item.Id);
                                if (existingItem != null && existingItem.Package != null)
                                {
                                    assetPackage = existingItem.Package;
                                }

                                // Remove the asset before reimporting it
                                if (assetPackage.Assets.RemoveById(item.Id))
                                {
                                    result.RemovedAssets.Add(item.Id);
                                    result.AddedAssets.RemoveWhere(x => x.Id == item.Id);
                                }
                            }
                            else
                            {
                                // Else simply use the asset in input
                                item = toImport.SelectedItem ?? toImport.Item;

                                // If the asset we are trying to import is a merged asset that will be imported
                                // don't try to import it
                                if (mergedAssets.Contains(item.Id))
                                {
                                    continue;
                                }

                                // Update the name just before adding to the package
                                // to make sure that there will be no clash
                                FixAssetLocation(item, fileToImport.Directory, assetResolver, allImports);
                            }

                            assetPackage.Assets.Add(item);
                            result.AddedAssets.Add(item);
                        }
                        finally
                        {
                            OnProgress(AssetImportSessionEventType.End, AssetImportSessionStepType.Importing, toImport);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets the current importItem session.
        /// </summary>
        public void Reset()
        {
            Imports.Clear();
            sourceFileToAssets.Clear();
        }

        /// <summary>
        /// List all <see cref="AssetItem"/> to import by sorting them for the asset having most dependencies to assets
        /// having no dependencies. This is then used by <see cref="PrepareMerge"/> to perform a top-down matching of asset
        /// to import with exisiting assets.
        /// </summary>
        /// <returns></returns>
        private List<AssetToImportMergeGroup> ComputeToImportListSorted()
        {
            var toImportListSorted = new List<AssetToImportMergeGroup>();

            var registeredTypes = new HashSet<Type>();
            var typeDependencies = new Dictionary<AssetToImportMergeGroup, HashSet<Type>>();
            foreach (var toImport in AllImports())
            {
                var references = AssetReferenceAnalysis.Visit(toImport.Item.Asset);
                var refTypes = new HashSet<Type>(references.Select(assetLink => assetLink.Reference).OfType<AssetReference>().Select(assetRef => assetRef.Type));
                // Optimized path, if an asset has no dependencies, directly add it to the sorted list
                if (refTypes.Count == 0)
                {
                    toImportListSorted.Add(toImport);
                    registeredTypes.Add(toImport.Item.Asset.GetType());
                }
                else
                {
                    typeDependencies[toImport] = refTypes;
                }
            }

            var typeToRegisters = new HashSet<Type>();
            while (true)
            {
                typeToRegisters.Clear();
                var toImportTempList = typeDependencies.ToList();
                foreach (var toImportWithTypes in toImportTempList)
                {
                    var toImport = toImportWithTypes.Key;

                    var areDependenciesResolved = toImportWithTypes.Value.All(registeredTypes.Contains);
                    if (areDependenciesResolved)
                    {
                        typeDependencies.Remove(toImport);
                        toImportListSorted.Add(toImport);
                        typeToRegisters.Add(toImport.Item.Asset.GetType());
                    }
                }

                // If we have not found new dependencies, than exit
                if (typeToRegisters.Count == 0)
                {
                    break;
                }

                foreach (var newType in typeToRegisters)
                {
                    registeredTypes.Add(newType);
                }
            }

            // In case there are some import elements not filtered remaining, we should still add them
            toImportListSorted.AddRange(typeDependencies.Keys);

            // We need to return a list from assets with most dependencies to asset with less dependencies
            toImportListSorted.Reverse();

            return toImportListSorted;
        }

        /// <summary>
        /// Prepares assets for merging by trying to match with existing assets.
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        private void PrepareMerge(CancellationToken? cancelToken = null)
        {
            // Presort the assets so that we will make sure to try to match them from top-down (model to texture for example)
            //
            // -----------------------------------||---------------------------------------------
            //   Assets being imported            ||  Assets found in the current session
            // -----------------------------------||---------------------------------------------
            //  - Model1                          =>      - Model0
            //      - Material1                   =>          - Material0
            //          - Texture 1.1             =>              - Texture 0.1
            //          - Texture 1.2             =>              - Texture 0.2       
            //
            var toImportListSorted = ComputeToImportListSorted();

            // Pass 2) => Calculate matches
            // We need to calculate matches after pass1, as we could have to match
            // between assets being imported
            foreach (var toImport in toImportListSorted)
            {
                var toImportByImporter = toImport.Parent;
                var fileToImport = toImportByImporter.Parent;

                OnProgress(AssetImportSessionEventType.Begin, AssetImportSessionStepType.Matching, toImport);

                var assetItem = toImport.Item;
                var assetImport = assetItem.Asset as AssetImport;
                if (assetImport == null || toImport.Merges.Count > 0)
                {
                    continue;
                }

                if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                {
                    toImport.Log.Warning("Cancellation requested before matching asset [{0}] with location [{1}] ", fileToImport.File, assetItem.Location);
                    return;
                }

                var assetType = assetImport.GetType();

                List<AssetItem> possibleMatches;

                // When we are explcitly importing an existing asset, we are going to match the import directly and only with it
                if (toImportByImporter.PreviousItems.Count > 0 && toImportByImporter.PreviousItems.Any(item => item.Asset.GetType() == assetType))
                {
                    possibleMatches = new List<AssetItem>(toImportByImporter.PreviousItems.Where(item => item.Asset.GetType() == assetType));
                }
                else
                {
                    // Else gets a list of existing assets that could match, as we are going to match against them.
                    possibleMatches = GetOrCreateAssetsPerInput(assetImport.Source, assetType).Where(item => item.Asset.GetType() == assetType && item != assetItem).ToList();
                }

                foreach (var possibleMatchItem in possibleMatches)
                {
                    // Main method to match assets
                    RecursiveCalculateMatchAndPrepareMerge(toImportByImporter, toImport, possibleMatchItem);
                }

                // If there are no merges, just take the original element to importItem
                // Or all matching factor are null (meaning most likely something not matching at all)
                if (!toImport.Merges.HasMatching())
                {
                    toImport.SelectedItem = toImport.Item;
                }
            }

            // Order matches by sort order
            foreach (var toImport in toImportListSorted)
            {
                toImport.Merges.Sort((left, right) =>
                {
                    if ((left.PreviousItem.Package == null && right.PreviousItem.Package != null)
                        || (left.PreviousItem.Package != null && right.PreviousItem.Package == null))
                    {
                        return left.PreviousItem.Package == null ? 1 : -1;
                    }

                    return -left.MatchingFactor.CompareTo(right.MatchingFactor);
                });

                OnProgress(AssetImportSessionEventType.End, AssetImportSessionStepType.Matching, toImport);
            }
        }

        private IEnumerable<AssetToImportMergeGroup> AllImports()
        {
            return from fileToImport in Imports.Where(it => it.Enabled) from toImportByImporter in fileToImport.ByImporters.Where(it => it.Enabled) from toImport in toImportByImporter.Items.Where(it => it.Enabled) select toImport;
        }

        private AssetToImport RegisterImporter(UFile file, Package package, UDirectory directory, IAssetImporter importer, AssetItem previousItem = null)
        {
            var previousEntry = Imports.FirstOrDefault(item => item.File == file);
            if (previousEntry == null)
            {
                previousEntry = new AssetToImport(file) { Package = package, Directory = directory};
                Imports.Add(previousEntry);
            }
            // This importer has not been registered yet
            var byImporter = previousEntry.ByImporters.FirstOrDefault(t => t.Importer == importer);
            if (byImporter == null)
            {
                previousEntry.ByImporters.Add(new AssetToImportByImporter(previousEntry, importer, previousItem));
                previousEntry.ByImporters.Sort((left, right) => left.Importer.Order.CompareTo(right.Importer.Order));
            }
            else
            {
                // Add the previous item if any
                if (previousItem != null)
                {
                    byImporter.PreviousItems.Add(previousItem);
                }
            }

            return previousEntry;
        }

        private void FixAssetLocation(AssetItem item, UDirectory targetDirectory, AssetResolver assetResolver, IEnumerable<AssetToImportMergeGroup> assetsToImport)
        {
            // find a possible asset location
            UFile newLocation;
            var path = new UFile(targetDirectory, item.Location.GetFileName(), null);
            assetResolver.RegisterLocation(path, out newLocation);

            // current location is valid -> nothing to do
            if (item.Location == newLocation)
                return;

            // location changed -> we need to fix all the reference to this item
            foreach (var referencingAsset in assetsToImport)
            {
                var referencesToUpdate = AssetReferenceAnalysis.Visit(referencingAsset.Item.Asset);
                foreach (var assetReferenceLink in referencesToUpdate)
                {
                    var refToUpdate = assetReferenceLink.Reference as IContentReference;
                    if (refToUpdate != null && refToUpdate.Id == item.Id)
                    {
                        assetReferenceLink.UpdateReference(item.Id, newLocation);
                    }
                }
            }
            item.Location = newLocation;
        }

        private void FreezeAssetImport(IAssetImporter importer, AssetItem assetItem)
        {
            // Base guid for assets to importItem must be empty
            var baseAsset = (Asset)AssetCloner.Clone(assetItem.Asset);
            var assetImport = assetItem.Asset as AssetImport;
            baseAsset.Id = Guid.Empty;

            // If we have an asset import, compute the hash and make sure the base doesn't include any info about sources
            if (assetImport != null)
            {
                var baseAssetImport = (AssetImport)baseAsset;
                baseAssetImport.SetAsRootImport();

                baseAssetImport.ImporterId = importer.Id;
                var assetImportTracked = assetImport as AssetImportTracked;
                if (assetImportTracked != null)
                {
                    assetImportTracked.SourceHash = FileVersionManager.Instance.ComputeFileHash(assetImport.Source);
                }
            }
            assetItem.Asset.Base = new AssetBase(baseAsset);
        }

        private void ComputeAssetHash(CancellationToken? cancelToken = null)
        {
            sourceFileToAssets.Clear();

            // Pass 1) => Prepare imports
            // - Check for assets with same source
            // - Fix location
            foreach (var fileToImport in Imports.Where(it => it.Enabled))
            {
                var assetPackage = fileToImport.Package;

                foreach (var import in fileToImport.ByImporters.Where(it => it.Enabled))
                {
                    foreach (var assetToImport in import.Items.Where(it => it.Enabled))
                    {
                        if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                        {
                            assetToImport.Log.Warning("Cancellation requested before computing hash for asset [{0}] with location [{1}] ", fileToImport.File, assetToImport.Item.Location);
                            return;
                        }

                        OnProgress(AssetImportSessionEventType.Begin, AssetImportSessionStepType.ComputeHash, assetToImport);
                        try
                        {
                            var assetItem = assetToImport.Item;

                            // Freeze asset importItem and calculate source hash
                            FreezeAssetImport(import.Importer, assetItem);

                            // Create mapping: assetitem => set of similar asset items
                            var assetImport = assetItem.Asset as AssetImport;
                            if (assetImport != null)
                            {
                                // Create mapping: source file => set of asset items
                                RegisterAssetPerInput(assetImport.Source, assetItem);

                                // Add assets from session
                                foreach (var existingAssetItem in session.DependencyManager.FindAssetItemsByInput(assetImport.Source))
                                {
                                    // Filter and only take current root imports
                                    if (existingAssetItem.Asset.Base == null || !existingAssetItem.Asset.Base.IsRootImport)
                                    {
                                        continue;
                                    }

                                    RegisterAssetPerInput(assetImport.Source, existingAssetItem);
                                }
                            }
                        }
                        finally
                        {
                            OnProgress(AssetImportSessionEventType.End, AssetImportSessionStepType.ComputeHash, assetToImport);
                        }
                    }
                }
            }
        }

        private void FixAssetLocations()
        {
            var allImports = AllImports().ToList();

            // Pass 1) => Prepare imports
            // - Check for assets with same source
            // - Fix location
            foreach (var fileToImport in Imports.Where(it => it.Enabled))
            {
                var assetPackage = fileToImport.Package;
                var assetResolver = AssetResolver.FromPackage(assetPackage);

                foreach (var import in fileToImport.ByImporters.Where(it => it.Enabled))
                {
                    foreach (var assetToImport in import.Items.Where(it => it.Enabled))
                    {
                        var assetItem = assetToImport.Item;

                        // Fix asset location
                        FixAssetLocation(assetItem, fileToImport.Directory, assetResolver, allImports);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively calculate the match between assets and prepare merge. See remarks.
        /// </summary>
        /// <param name="toImport">To import.</param>
        /// <param name="toImportMergeGroup">To import merge group.</param>
        /// <param name="previous">The previous.</param>
        /// <remarks>
        /// This method is a tricky part of merging assets, as it is able to match assets with existing asset (either from the
        /// current import session or from the current package session). The main problem is in the case of an asset being 
        /// imported that is an <see cref="AssetImport"/> that is referencing some assets being imported as well that are not
        /// <see cref="AssetImport"/>. In this case, these referenced assets cannot be detected directly when importing assets.
        /// But we still need to be able to match these assets with some existing assets.
        /// In order to detect these references, we are previewing a merge between the <see cref="AssetImport"/> and the asset
        /// in the current session. In the differences between them, we are handling specially differences for 
        /// <see cref="IContentReference"/> as we expect them to be remapped by the importing process. 
        /// 
        /// For example, suppose a package contains a model A from a specified FBX file that is referencing assets B1 and B2. 
        /// When we are trying to import the same model A, it will first create A' that will reference B1' and B2'.
        /// It is easy to find that correspondance between A and A', as they have the same FBX source file. But for B1/B1' or
        /// B2/B2', we need to first check that this references are broken, associate that B1' is in fact B1, B2' is in fact
        /// B2, and try to merge this assets recursively (as they could also contains references to other assets)
        /// 
        /// TODO: Review this comment by someone else!
        /// </remarks>
        private void RecursiveCalculateMatchAndPrepareMerge(AssetToImportByImporter toImport, AssetToImportMergeGroup toImportMergeGroup, AssetItem previous)
        {
            var newAssetBase = toImportMergeGroup.Item.Asset.Base.Asset;
            var previousAsset = previous.Asset;
            var previousBase = previous.Asset.Base == null || previous.Asset.Base.Asset == null ? newAssetBase : previous.Asset.Base.Asset;

            // If this matching has been already processed, exit immediately
            if (toImportMergeGroup.Merges.Any(matching => matching.PreviousItem.Id == previous.Id))
            {
                return;
            }

            // If the new asset is an asset import, we need to copy the freshly computed SourceHash to it.
            var newAsset = (Asset)AssetCloner.Clone(previousAsset);
            var newAssetImport = newAsset as AssetImportTracked;
            if (newAssetImport != null)
            {
                var originAssetImport = (AssetImportTracked)toImportMergeGroup.Item.Asset;
                newAssetImport.SourceHash = originAssetImport.SourceHash;
            }

            // Make a diff between the previous asset and the new asset to importItem
            var assetDiff = new AssetDiff(previousBase, newAsset, newAssetBase);
            
            // Perform a preview merge
            var result = AssetMerge.Merge(assetDiff, MergeImportPolicy, true);

            // Retrieve the precalculated list of diffs
            var diff3 = assetDiff.Compute();

            // Base matching factor
            var baseMatchingFactor = 0.0f;

            var totalChildren = diff3.CountChildren();
            var diffList = diff3.FindDifferencesWithWeights().ToList();

            // If there are any weighted field/properties without any conflicts, we will match them here
            for (int i = diffList.Count - 1; i >= 0; i--)
            {
                var diffItem = diffList[i];
                if (diffItem.Weight != 0 && diffItem.ChangeType == Diff3ChangeType.None)
                {
                    baseMatchingFactor += diffItem.Weight;
                    diffList.RemoveAt(i);
                }
            }

            var conflictCount = diffList.Count(node => node.HasConflict);

            // Gets the references differences
            var assetReferencesDiffs = diffList.Where(IsContentReference).ToList();

            // The matching is calculated taking into account the number of conflicts and the number
            // of unresolved references (implicit conflicts)
            var assetMatching = new AssetToImportMerge(previous, assetDiff, result);

            // The number of references
            var subReferenceCount = assetReferencesDiffs.Count;

            // The number of references whose their content exist
            var subReferenceMatch = 0;

            // Recursively calculate differences on referenced objects
            foreach (var referenceDiff in assetReferencesDiffs)
            {
                var base1 = referenceDiff.BaseNode.Instance;
                var newRef = referenceDiff.Asset2Node.Instance;
                var baseReference = GetContentReference(base1);
                var asset2Reference = GetContentReference(newRef);

                var baseFileName = baseReference != null ? new UFile(baseReference.Location).GetFileName() : null;
                var asset2FileName = asset2Reference != null ? new UFile(asset2Reference.Location).GetFileName() : null;

                if (base1 != null && newRef != null && baseFileName != null && baseFileName == asset2FileName)
                {
                    // Check if the referenced asset is existing in the session
                    var baseItem1 = session.FindAsset(baseReference.Id);
                    if (baseItem1 != null)
                    {
                        // Try to find an asset from the import session that is matching 
                        var subImport1 = toImport.Items.Where(it => it.Enabled).FirstOrDefault(importList => importList.Item.Id == asset2Reference.Id);
                        if (subImport1 != null && baseItem1.Asset.GetType() == subImport1.Item.Asset.GetType())
                        {
                            RecursiveCalculateMatchAndPrepareMerge(toImport, subImport1, baseItem1);

                            // Update dependencies so we will be able to sort the matching in the Merge() method.
                            assetMatching.DependencyGroups.Add(subImport1);
                            subReferenceMatch++;
                        }
                    }
                }
                else if (base1 == null && newRef != null)
                {
                    subReferenceMatch++;
                }
            }

            // Calculate a matching factor
            // In the standard case, we should have subReferenceCount == subReferenceMatch
            foreach (var diffItem in diffList)
            {
                if (diffItem.ChangeType != Diff3ChangeType.Children)
                {
                    if (diffItem.ChangeType == Diff3ChangeType.MergeFromAsset1And2)
                    {
                        // If both value from 1 and 2 are fine, it is like a match, so we increase the matching based on the weight
                        baseMatchingFactor += diffItem.Weight;
                    }
                    else
                    {
                        // If values are different (conflict...etc.), we decrease the matching based on the weight (by default, the weight is 0)
                        baseMatchingFactor -= diffItem.Weight;
                    }
                }
            }
            assetMatching.MatchingFactor = baseMatchingFactor + 1.0 - (double)(conflictCount + subReferenceCount - subReferenceMatch) / totalChildren;

            toImportMergeGroup.Merges.Add(assetMatching);
        }

        private static bool IsContentReference(Diff3Node node)
        {
            if (typeof(IContentReference).IsAssignableFrom(node.InstanceType))
                return true;

            // If the new asset version is a reference, we can try to merge it
            if (node.Asset2Node != null && node.Asset2Node.Instance != null)
                return AttachedReferenceManager.GetAttachedReference(node.Asset2Node.Instance) != null;

            return false;
        }

        private static IContentReference GetContentReference(object instance)
        {
            if (instance == null)
            {
                return null;
            }

            var contentReference = instance as IContentReference;
            if (contentReference != null)
            {
                return contentReference;
            }

            var attachedReference = AttachedReferenceManager.GetAttachedReference(instance);
            return attachedReference;
        }

        private Diff3ChangeType MergeImportPolicy(Diff3Node node)
        {
            // Asset references are a special case while importing, as we are
            // going to try to rematch them, so if they changed, we expect to 
            // use the original instance
            if (IsContentReference(node))
            {
                if (node.Asset1Node != null)
                {
                    return Diff3ChangeType.MergeFromAsset1;
                }

                if (node.Asset2Node != null)
                {
                    return Diff3ChangeType.MergeFromAsset2;
                }
            }
            return AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1(node);
        }

        private void RegisterAssetPerInput(string rawSourcePath, AssetItem assetItem)
        {
            GetOrCreateAssetsPerInput(rawSourcePath, assetItem.Asset.GetType()).Add(assetItem);
        }

        private HashSet<AssetItem> GetOrCreateAssetsPerInput(string rawSourcePath, Type assetType)
        {
            var assetKey = new AssetLocationTyped(rawSourcePath, assetType);
            HashSet<AssetItem> assetsPerFile;
            if (!sourceFileToAssets.TryGetValue(assetKey, out assetsPerFile))
            {
                assetsPerFile = new HashSet<AssetItem>(AssetItem.DefaultComparerById);
                sourceFileToAssets.Add(assetKey, assetsPerFile);
            }
            return assetsPerFile;
        }

        /// <summary>
        /// This method is validating all assets being imported. If there is any errors,
        /// it is throwing an <see cref="InvalidOperationException"/> as this is considered as an invalid usage of the API.
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="ids"></param>
        /// <returns>True if there is at least one AssetImport, False otherwise.</returns>
        private bool CheckAssetsToImport(IEnumerable<AssetItem> assets, HashSet<Guid> ids)
        {
            // Pre-check
            var log = new LoggerResult();
            var hasAssetImport = false;
            foreach (var assetItem in assets)
            {
                if (assetItem.Id == Guid.Empty)
                {
                    log.Error("Invalid arguments while importing asset [{0}]. Requiring an asset Id not empty", assetItem);
                }
                else if (ids.Contains(assetItem.Id))
                {
                    log.Error("Invalid arguments while importing asset [{0}]. An asset is already being imported with the same id", assetItem);
                }
                else
                {
                    var existingAssetItem = session.FindAsset(assetItem.Id);
                    if (existingAssetItem != null)
                    {
                        log.Error("Invalid arguments while importing asset [{0}]. An asset is already used by the package [{1}/{2}] in the current session", assetItem, existingAssetItem.Package.Id, existingAssetItem.Package.FullPath);
                    }
                }

                if (assetItem.Asset.Base != null)
                {
                    log.Error("Invalid arguments while importing asset [{0}]. Base must be null", assetItem);
                }

                var assetImport = assetItem.Asset as AssetImport;
                if (assetImport != null)
                {
                    hasAssetImport = true;
                    if (assetImport.Source == null || !assetImport.Source.IsAbsolute)
                    {
                        log.Error("Invalid arguments while importing asset [{0}]. Type [{1}] cannot be null and must be an absolute location", assetItem, assetImport.Source);
                    }
                }

                ids.Add(assetItem.Id);
            }

            // If we have any errors, don't process further
            if (log.HasErrors)
            {
                // Generate an exception as all checks above are supposed to be an invalid usage of the API
                throw new InvalidOperationException("Unexpected error while processing items to importItem: " + log.ToText());
            }
            return hasAssetImport;
        }

        [DebuggerDisplay("Location: {location}")]
        private struct AssetLocationTyped : IEquatable<AssetLocationTyped>
        {
            public AssetLocationTyped(string location, Type assetType)
            {
                this.location = location;
                this.assetType = assetType;
            }

            private readonly string location;

            private readonly Type assetType;

            public bool Equals(AssetLocationTyped other)
            {
                return string.Equals(location, other.location, StringComparison.OrdinalIgnoreCase) && assetType == other.assetType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is AssetLocationTyped && Equals((AssetLocationTyped)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (location.GetHashCode()*397) ^ assetType.GetHashCode();
                }
            }
        }

        private void OnProgress(AssetImportSessionEventType step, AssetImportSessionStepType type, AssetToImportByImporter toImportByImporter)
        {
            EventHandler<AssetImportSessionEvent> handler = Progress;
            if (handler != null) handler(this, new AssetImportSessionEvent(step, type, toImportByImporter));
        }

        private void OnProgress(AssetImportSessionEventType step, AssetImportSessionStepType type, AssetToImportMergeGroup toImportMergeGroup)
        {
            EventHandler<AssetImportSessionEvent> handler = Progress;
            if (handler != null) handler(this, new AssetImportSessionEvent(step, type, toImportMergeGroup));
        }
    }
}