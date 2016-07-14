// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A command processing an <see cref="Asset"/>.
    /// </summary>
    public abstract class AssetCommand : IndexFileCommand
    {
        public string Url { get; set; }

        protected AssetCommand(string url)
        {
            Url = url;
        }

    }

    public abstract class AssetCommand<T> : AssetCommand
    {

        protected AssetCommand(string url, T assetParameters)
            : base (url)
        {
            AssetParameters = assetParameters;
        }

        public T AssetParameters { get; set; }
        
        public override string Title
        {
            get
            {
                return string.Format("Asset command processing {0}", Url);
            }
        }

        protected static void ComputeCompileTimeDependenciesHash(Package package, BinarySerializationWriter writer, Asset asset)
        {
            var assetWithCompileTimeDependencies = asset as IAssetCompileTimeDependencies;
            if (assetWithCompileTimeDependencies != null)
            {
                foreach (var dependentAssetReference in assetWithCompileTimeDependencies.EnumerateCompileTimeDependencies())
                {
                    var dependentAssetItem = package.FindAsset(dependentAssetReference);
                    var dependentAsset = dependentAssetItem != null ? dependentAssetItem.Asset : null;
                    if (dependentAsset == null)
                        continue;

                    if (dependentAsset == asset)
                    {
                        // TODO: We don't have access to the log here, so we are throwing an exception which is not really user friendly with the stacktrace exception
                        throw new InvalidOperationException(string.Format("Asset [{0}:{1}] cannot be used recursively", asset.Id, dependentAssetReference.Location));
                    }
                    
                    // Hash asset content (since it is embedded, not a real reference)
                    // Note: we hash child and not current, because when we start with main asset, it has already been hashed by base.ComputeParameterHash()
                    writer.SerializeExtended(ref dependentAsset, ArchiveMode.Serialize);

                    // Recurse
                    ComputeCompileTimeDependenciesHash(package, writer, dependentAsset);
                }
            }
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);
            
            var url = Url;
            var assetParameters = AssetParameters;
            writer.SerializeExtended(ref assetParameters, ArchiveMode.Serialize);
            writer.Serialize(ref url, ArchiveMode.Serialize);
        }

        public override string ToString()
        {
            // TODO provide automatic asset to string via YAML
            return AssetParameters.ToString();
        }
    }
}
