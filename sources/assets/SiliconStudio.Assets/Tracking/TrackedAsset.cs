using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tracking
{
    /// <summary>
    /// Represents a single asset which has source files currently being tracked for changes.
    /// </summary>
    internal class TrackedAsset : IDisposable
    {
        private readonly AssetSourceTracker tracker;
        private readonly Asset sessionAsset;
        private Dictionary<UFile, bool> sourceFiles = new Dictionary<UFile, bool>();
        private Asset clonedAsset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedAsset"/> class.
        /// </summary>
        /// <param name="tracker">The source tracker managing this object.</param>
        /// <param name="sessionAsset">The actual asset in the current session.</param>
        /// <param name="clonedAsset">A clone of the actual asset. If the actual asset is read-only, it is acceptable to use it instead of a clone.</param>
        public TrackedAsset(AssetSourceTracker tracker, Asset sessionAsset, Asset clonedAsset)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            this.tracker = tracker;
            this.sessionAsset = sessionAsset;
            this.clonedAsset = clonedAsset;
            UpdateAssetImportPathsTracked(true);
        }

        /// <summary>
        /// Gets the id of this asset.
        /// </summary>
        internal Guid AssetId => sessionAsset.Id;

        /// <inheritdoc/>
        public void Dispose()
        {
            // Track asset import paths
            UpdateAssetImportPathsTracked(false);
        }

        /// <summary>
        /// Notifies this object that the asset has been modified.
        /// </summary>
        /// <remarks>This method will trigger the re-evaluation of properties containing the path to a source file.</remarks>
        public void NotifyAssetChanged()
        {
            clonedAsset = (Asset)AssetCloner.Clone(sessionAsset, AssetClonerFlags.KeepBases);
            UpdateAssetImportPathsTracked(true);
        }

        private void UpdateAssetImportPathsTracked(bool isTracking)
        {
            if (isTracking)
            {
                var collector = new SourceFilesCollector();
                var newSourceFiles = collector.GetSourceFiles(clonedAsset);
                bool changed = false;
                // Untrack previous paths
                foreach (var sourceFile in sourceFiles.Keys)
                {
                    if (!newSourceFiles.ContainsKey(sourceFile))
                    {
                        tracker.UnTrackAssetImportInput(AssetId, sourceFile);
                        changed = true;
                    }
                }

                // Track new paths
                foreach (var sourceFile in newSourceFiles.Keys)
                {
                    if (!sourceFiles.ContainsKey(sourceFile))
                    {
                        tracker.TrackAssetImportInput(AssetId, sourceFile);
                        changed = true;
                    }
                }

                sourceFiles = newSourceFiles;

                if (changed)
                {
                    tracker.SourceFileChanged.Post(new[] { new SourceFileChangedData(SourceFileChangeType.Asset, AssetId, sourceFiles.Select(x => x.Key).ToList()) });
                }
            }
            else
            {
                foreach (var sourceFile in sourceFiles.Keys)
                {
                    tracker.UnTrackAssetImportInput(AssetId, sourceFile);
                }
            }
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
