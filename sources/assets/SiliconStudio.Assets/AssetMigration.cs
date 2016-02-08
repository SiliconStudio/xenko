// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;
using System.IO;
using System.Text;

using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using System.Linq;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Helper for migrating asset to newer versions.
    /// </summary>
    public static class AssetMigration
    {
        public static bool MigrateAssetIfNeeded(AssetMigrationContext context, PackageLoadingAssetFile loadAsset, string dependencyName, PackageVersion untilVersion = null)
        {
            var assetFullPath = loadAsset.FilePath.FullPath;

            // Determine if asset was Yaml or not
            var assetFileExtension = Path.GetExtension(assetFullPath);
            if (assetFileExtension == null)
                return false;

            assetFileExtension = assetFileExtension.ToLowerInvariant();

            var serializer = AssetSerializer.FindSerializer(assetFileExtension);
            if (!(serializer is AssetYamlSerializer))
                return false;

            // We've got a Yaml asset, let's get expected and serialized versions
            var serializedVersion = PackageVersion.Zero;
            PackageVersion expectedVersion;
            Type assetType;

            // Read from Yaml file the asset version and its type (to get expected version)
            // Note: It tries to read as few as possible (SerializedVersion is expected to be right after Id, so it shouldn't try to read further than that)
            using (var assetStream = loadAsset.OpenStream())
            using (var streamReader = new StreamReader(assetStream))
            {
                var yamlEventReader = new EventReader(new Parser(streamReader));

                // Skip header
                yamlEventReader.Expect<StreamStart>();
                yamlEventReader.Expect<DocumentStart>();
                var mappingStart = yamlEventReader.Expect<MappingStart>();

                var yamlSerializerSettings = YamlSerializer.GetSerializerSettings();
                var tagTypeRegistry = yamlSerializerSettings.TagTypeRegistry;
                bool typeAliased;
                assetType = tagTypeRegistry.TypeFromTag(mappingStart.Tag, out typeAliased);

                var expectedVersions = AssetRegistry.GetCurrentFormatVersions(assetType);
                expectedVersion = expectedVersions?.FirstOrDefault(x => x.Key == dependencyName).Value ?? PackageVersion.Zero;

                Scalar assetKey;
                while ((assetKey = yamlEventReader.Allow<Scalar>()) != null)
                {
                    // Only allow Id before SerializedVersion
                    if (assetKey.Value == nameof(Asset.Id))
                    {
                        yamlEventReader.Skip();
                    }
                    else if (assetKey.Value == nameof(Asset.SerializedVersion))
                    {
                        // Check for old format: only a scalar
                        var scalarVersion = yamlEventReader.Allow<Scalar>();
                        if (scalarVersion != null)
                        {
                            serializedVersion = PackageVersion.Parse("0.0." + Convert.ToInt32(scalarVersion.Value, CultureInfo.InvariantCulture));

                            // Let's update to new format
                            using (var yamlAsset = loadAsset.AsYamlAsset())
                            {
                                yamlAsset.DynamicRootNode.RemoveChild(nameof(Asset.SerializedVersion));
                                AssetUpgraderBase.SetSerializableVersion(yamlAsset.DynamicRootNode, dependencyName, serializedVersion);

                                var baseBranch = yamlAsset.DynamicRootNode[Asset.BaseProperty];
                                if (baseBranch != null)
                                {
                                    var baseAsset = baseBranch["Asset"];
                                    if (baseAsset != null)
                                    {
                                        baseAsset.RemoveChild(nameof(Asset.SerializedVersion));
                                        AssetUpgraderBase.SetSerializableVersion(baseAsset, dependencyName, serializedVersion);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // New format: package => version mapping
                            yamlEventReader.Expect<MappingStart>();

                            while (!yamlEventReader.Accept<MappingEnd>())
                            {
                                var packageName = yamlEventReader.Expect<Scalar>().Value;
                                var packageVersion = PackageVersion.Parse(yamlEventReader.Expect<Scalar>().Value);

                                // For now, we handle only one dependency at a time
                                if (packageName == dependencyName)
                                {
                                    serializedVersion = packageVersion;
                                }
                            }

                            yamlEventReader.Expect<MappingEnd>();
                        }
                        break;
                    }
                    else
                    {
                        // If anything else than Id or SerializedVersion, let's stop
                        break;
                    }
                }
            }

            if (serializedVersion > expectedVersion)
            {
                // Try to open an asset newer than what we support (probably generated by a newer Xenko)
                throw new InvalidOperationException($"Asset of type {assetType} has been serialized with newer version {serializedVersion}, but only version {expectedVersion} is supported. Was this asset created with a newer version of Xenko?");
            }

            if (serializedVersion < expectedVersion)
            {
                // Perform asset upgrade
                context.Log.Verbose("{0} needs update, from version {1} to version {2}", Path.GetFullPath(assetFullPath), serializedVersion, expectedVersion);

                using (var yamlAsset = loadAsset.AsYamlAsset())
                {
                    var yamlRootNode = yamlAsset.RootNode;

                    // Check if there is any asset updater
                    var assetUpgraders = AssetRegistry.GetAssetUpgraders(assetType, dependencyName);
                    if (assetUpgraders == null)
                    {
                        throw new InvalidOperationException($"Asset of type {assetType} should be updated from version {serializedVersion} to {expectedVersion}, but no asset migration path was found");
                    }

                    // Instantiate asset updaters
                    var currentVersion = serializedVersion;
                    while (currentVersion != expectedVersion)
                    {
                        PackageVersion targetVersion;
                        // This will throw an exception if no upgrader is available for the given version, exiting the loop in case of error.
                        var upgrader = assetUpgraders.GetUpgrader(currentVersion, out targetVersion);

                        // Stop if the next version would be higher than what is expected
                        if (untilVersion != null && targetVersion > untilVersion)
                            break;

                        upgrader.Upgrade(context, dependencyName, currentVersion, targetVersion, yamlRootNode, loadAsset);
                        currentVersion = targetVersion;
                    }

                    // Make sure asset is updated to latest version
                    YamlNode serializedVersionNode;
                    PackageVersion newSerializedVersion = null;
                    if (yamlRootNode.Children.TryGetValue(new YamlScalarNode(nameof(Asset.SerializedVersion)), out serializedVersionNode))
                    {
                        var newSerializedVersionForDefaultPackage = ((YamlMappingNode)serializedVersionNode).Children[new YamlScalarNode(dependencyName)];
                        newSerializedVersion = PackageVersion.Parse(((YamlScalarNode)newSerializedVersionForDefaultPackage).Value);
                    }

                    if (untilVersion == null && newSerializedVersion != expectedVersion)
                    {
                        throw new InvalidOperationException($"Asset of type {assetType} was migrated, but still its new version {newSerializedVersion} doesn't match expected version {expectedVersion}.");
                    }

                    context.Log.Info("{0} updated from version {1} to version {2}", Path.GetFullPath(assetFullPath), serializedVersion, expectedVersion);
                }

                return true;
            }

            return false;
        }
    }
}