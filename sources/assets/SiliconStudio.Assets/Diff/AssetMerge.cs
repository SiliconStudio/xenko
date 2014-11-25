// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Merges asset differences.
    /// </summary>
    public static class AssetMerge
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("AssetMerge");

        /// <summary>
        /// A policy that returns the change to apply to the current <see cref="Diff3Node"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The type of merge for the node.</returns>
        public delegate Diff3ChangeType MergePolicyDelegate(Diff3Node node);

        static AssetMerge()
        {
            try
            {
                // TODO We need to discover different diff tools
                var key = Registry.CurrentUser.OpenSubKey("Software\\KDiff3");
                if (key != null)
                {
                    var kDiffPath = key.GetValue(null) as string;
                    if (kDiffPath != null)
                    {
                        DefaultMergeTool = Path.Combine(kDiffPath, "kdiff3.exe");
                    }
                    key.Close();
                }
                if (DefaultMergeTool == null)
                {
                    Log.Error("Unable to find a default merge tool");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to find a default merge tool", ex);
            }
        }

        /// <summary>
        /// Gets or sets the default merge tool exe filepath.
        /// </summary>
        /// <value>The default merge tool exe filepath.</value>
        public static string DefaultMergeTool { get; set; }

        /// <summary>
        /// Merges the specified assets from <c>base</c> and <c>from2</c> into <c>from1</c>.
        /// </summary>
        /// <param name="assetBase">The asset base.</param>
        /// <param name="assetFrom1">The asset from1.</param>
        /// <param name="assetFrom2">The asset from2.</param>
        /// <param name="mergePolicy">The merge policy. See <see cref="AssetMergePolicies" /> for default policies.</param>
        /// <returns>The result of the merge.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// assetFrom1
        /// or
        /// mergePolicy
        /// </exception>
        public static MergeResult Merge(Asset assetBase, Asset assetFrom1, Asset assetFrom2, MergePolicyDelegate mergePolicy)
        {
            if (assetFrom1 == null) throw new ArgumentNullException("assetFrom1");
            if (mergePolicy == null) throw new ArgumentNullException("mergePolicy");
            return Merge(new AssetDiff((Asset)AssetCloner.Clone(assetBase), (Asset)AssetCloner.Clone(assetFrom1), (Asset)AssetCloner.Clone(assetFrom2)), mergePolicy);
        }

        /// <summary>
        /// Merges the specified assets from <c>base</c> and <c>from2</c> into <c>from1</c>.
        /// </summary>
        /// <param name="assetDiff">A precomputed asset difference.</param>
        /// <param name="mergePolicy">The merge policy.</param>
        /// <param name="previewOnly">if set to <c>true</c> then the merge will not change the object.</param>
        /// <returns>MergePreviewResult.</returns>
        /// <exception cref="System.ArgumentNullException">assetDiff
        /// or
        /// mergePolicy</exception>
        public static MergeResult Merge(AssetDiff assetDiff, MergePolicyDelegate mergePolicy, bool previewOnly = false)
        {
            if (assetDiff == null) throw new ArgumentNullException("assetDiff");
            if (mergePolicy == null) throw new ArgumentNullException("mergePolicy");

            var allDiffs = assetDiff.Compute();
            var diff3 = allDiffs.FindDifferences().ToList();

            var result = new MergeResult(assetDiff.Asset1);

            // Try to merge
            foreach (var diff3Node in diff3)
            {
                Diff3ChangeType changeType;
                try
                {
                    changeType = mergePolicy(diff3Node);

                    if (changeType >= Diff3ChangeType.Conflict)
                    {
                        result.Error("Unresolved conflict [{0}] on node [{1}/{2}/{3}]", diff3Node.ChangeType, diff3Node.BaseNode, diff3Node.Asset1Node, diff3Node.Asset2Node);
                        continue;
                    }

                    // If we are in preview only mode, just skip the update
                    if (previewOnly)
                    {
                        continue;
                    }

                    object dataInstance2;
                    switch (changeType)
                    {
                        case Diff3ChangeType.MergeFromAsset2:
                            // As we are merging into asset1, the only relevant changes can only come from asset2
                            dataInstance2 = diff3Node.Asset2Node != null ? diff3Node.Asset2Node.Instance : null;
                            break;
                        case Diff3ChangeType.Children:
                            if (diff3Node.Asset1Node == null)
                            {
                                diff3Node.Asset1Node = diff3Node.Asset2Node.CreateWithEmptyInstance();
                            }

                            // If a node has children, since DiffCollection/DiffDictionary takes null as empty arrays,
                            // we should now create this array for it to be properly merged
                            if (diff3Node.Asset1Node.Instance != null)
                                continue;

                            dataInstance2 = Activator.CreateInstance(diff3Node.InstanceType);
                            break;
                        default:
                            continue;
                    }

                    // Sets the value on the node
                    diff3Node.ReplaceValue(dataInstance2, node => node.Asset1Node, diff3Node.Asset2Node == null);
                }
                catch (Exception ex)
                {
                    result.Error("Unexpected error while merging [{0}] on node [{1}]", ex, diff3Node.ChangeType, diff3Node.InstanceType);
                    break;
                }
            }

            if (!previewOnly)
            {
                foreach (var node in allDiffs.Asset1Node.Children(node => true))
                {
                    if (node.Instance is IDataDiffProxy)
                    {
                        ((IDataDiffProxy)node.Instance).ApplyChanges();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 3-way merge assets using an external diff tool.
        /// </summary>
        /// <param name="assetBase0">The asset base0.</param>
        /// <param name="assetFrom1">The asset from1.</param>
        /// <param name="assetFrom2">The asset from2.</param>
        /// <returns>The result of the merge.</returns>
        public static MergeResult MergeWithExternalTool(Asset assetBase0, Asset assetFrom1, Asset assetFrom2)
        {
            var result = new MergeResult();

            // If they are all null, nothing to do
            if (assetBase0 == null && assetFrom1 == null && assetFrom2 == null)
            {
                return result;
            }

            if (!File.Exists(DefaultMergeTool))
            {
                result.Error("Unable to use external diff3 merge tool [{0}]. File not found", DefaultMergeTool);
                return result;
            }

            var assetBase = (Asset)AssetCloner.Clone(assetBase0);
            var asset1 = (Asset)AssetCloner.Clone(assetFrom1);
            var asset2 = (Asset)AssetCloner.Clone(assetFrom2);

            // Clears base as we are not expecting to work with them directly
            // The real base must be passed by the assetBase0 parameter
            if (assetBase != null)
            {
                assetBase.Base = null;
            }
            if (asset1 != null)
            {
                asset1.Base = null;
            }
            if (asset2 != null)
            {
                asset2.Base = null;
            }

            var assetBasePath = Path.GetTempFileName();
            var asset1Path = Path.GetTempFileName();
            var asset2Path = Path.GetTempFileName();
            try
            {
                AssetSerializer.Save(assetBasePath, assetBase);
                AssetSerializer.Save(asset1Path, asset1);
                AssetSerializer.Save(asset2Path, asset2);
            }
            catch (Exception exception)
            {
                result.Error("Unexpected error while serializing assets on disk before using diff tool", exception);
                return result;
            }

            var outputPath = Path.GetTempFileName();
            try
            {
                // TODO We need to access different diff tools command line
                // kdiff3.exe file1 file2 file3 -o outputfile
                var process = Process.Start(DefaultMergeTool, string.Format("{0} {1} {2} -o {3}", assetBasePath, asset1Path, asset2Path, outputPath));
                if (process == null)
                {
                    result.Error("Unable to launch diff3 tool exe from [{0}]", DefaultMergeTool);
                }
                else
                {
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        result.Error("Error, failed to merge files");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error("Unable to launch diff3 tool exe from [{0}]", ex, DefaultMergeTool);
            }

            if (!result.HasErrors)
            {
                try
                {
                    var mergedAsset = (Asset)AssetSerializer.Load(outputPath);

                    if (mergedAsset != null)
                    {
                        if (assetFrom1 == null)
                        {
                            mergedAsset.Base = assetFrom2 == null ? assetBase0.Base : assetFrom2.Base;
                        }
                        else
                        {
                            mergedAsset.Base = assetFrom1.Base;
                        }
                    }
                    result.Asset = mergedAsset;
                }
                catch (Exception ex)
                {
                    result.Error("Unexpected exception while loading merged assets from [{0}]", ex, outputPath);
                }

            }

            return result;
        }
    }
}