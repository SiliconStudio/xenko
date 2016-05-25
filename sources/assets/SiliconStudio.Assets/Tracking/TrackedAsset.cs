using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tracking
{
    internal class TrackedAsset : IDisposable
    {
        private readonly AssetSourceTracker tracker;
        private readonly Guid assetId;
        private readonly Asset sessionAsset;
        private Dictionary<UFile, bool> sourceFiles = new Dictionary<UFile, bool>();
        private Asset clonedAsset;

        public TrackedAsset(AssetSourceTracker tracker, Guid assetId, Asset sessionAsset, Asset clonedAsset)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            this.tracker = tracker;
            this.assetId = assetId;
            this.sessionAsset = sessionAsset;
            this.clonedAsset = clonedAsset;
            UpdateAssetImportPathsTracked(true);
        }

        public bool NotifySourceFileChanged(UFile sourceFile, ObjectId newHash, ref SourceFileChangedData data)
        {
            bool hasSourceChanged = false;
            bool updateAssetIfChanged;
            if (sourceFiles.TryGetValue(sourceFile, out updateAssetIfChanged))
            {
                var oldHash = SourceHashesHelper.FindSourceHash(assetId, sourceFile);
                if (oldHash != newHash)
                {
                    data = new SourceFileChangedData(assetId, sourceFile, updateAssetIfChanged);
                    hasSourceChanged = true;
                }

                SourceHashesHelper.UpdateHash(assetId, sourceFile, newHash);
            }

            return hasSourceChanged;
        }

        private void UpdateAssetImportPathsTracked(bool isTracking)
        {
            if (isTracking)
            {
                var collector = new SourceFilesCollector();
                var newSourceFiles = collector.GetSourceFiles(clonedAsset);

                // Untrack previous paths
                foreach (var sourceFile in sourceFiles.Keys)
                {
                    if (!newSourceFiles.ContainsKey(sourceFile))
                    {
                        tracker.UnTrackAssetImportInput(assetId, sourceFile);
                        SourceHashesHelper.RemoveHash(assetId, sourceFile);
                    }
                }

                // Track new paths
                foreach (var sourceFile in newSourceFiles.Keys)
                {
                    if (!sourceFiles.ContainsKey(sourceFile))
                    {
                        tracker.TrackAssetImportInput(assetId, sourceFile);
                    }
                }

                sourceFiles = newSourceFiles;
            }
            else
            {
                foreach (var sourceFile in sourceFiles.Keys)
                {
                    tracker.UnTrackAssetImportInput(assetId, sourceFile);
                }
            }
        }

        public void NotifyAssetChanged()
        {
            clonedAsset = (Asset)AssetCloner.Clone(sessionAsset, AssetClonerFlags.KeepBases);
            UpdateAssetImportPathsTracked(true);
        }

        public void Dispose()
        {
            // Track asset import paths
            UpdateAssetImportPathsTracked(false);
        }

        private class SourceFilesCollector : AssetVisitorBase
        {
            private Dictionary<UFile, bool> sourceFileMembers;

            public Dictionary<UFile, bool> GetSourceFiles(Asset asset)
            {
                sourceFileMembers = new Dictionary<UFile, bool>();
                Visit(asset);
                return sourceFileMembers;
            }

            public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
            {
                // Don't visit base parts as they are visited at the top level.
                if (typeof(Asset).IsAssignableFrom(member.DeclaringType) && (member.Name == Asset.BasePartsProperty))
                {
                    return;
                }

                if (member.Type == typeof(UFile) && value != null)
                {
                    var file = (UFile)value;
                    if (!string.IsNullOrWhiteSpace(file.ToString()))
                    {
                        var attribute = member.GetCustomAttributes<SourceFileMemberAttribute>(true).SingleOrDefault();
                        if (attribute != null)
                        {
                            if (!sourceFileMembers.ContainsKey(file))
                            {
                                sourceFileMembers.Add(file, attribute.UpdateAssetIfChanged);
                            }
                            else if (attribute.UpdateAssetIfChanged)
                            {
                                // If the file has already been collected, just update whether it should update the asset when changed
                                sourceFileMembers[file] = true;
                            }
                        }
                    }
                }
                base.VisitObjectMember(container, containerDescriptor, member, value);
            }
        }
    }
}
