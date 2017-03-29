// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

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
        protected readonly Package Package;
        protected int Version;

        protected AssetCommand(string url, T parameters, Package package)
            : base (url)
        {
            Parameters = parameters;
            Package = package;
            InputFilesGetter = GetInputFilesImpl;
        }

        public T Parameters { get; set; }
        
        public override string Title => $"Asset command processing {Url}";

        protected void ComputeCompileTimeDependenciesHash(BinarySerializationWriter writer, Asset asset)
        {
            var assetWithCompileTimeDependencies = asset as IAssetCompileTimeDependencies;
            if (assetWithCompileTimeDependencies != null)
            {
                foreach (var dependentAssetReference in assetWithCompileTimeDependencies.EnumerateCompileTimeDependencies(Package.Session))
                {
                    var dependentAssetItem = Package.FindAsset(dependentAssetReference);
                    var dependentAsset = dependentAssetItem?.Asset;
                    if (dependentAsset == null)
                        continue;

                    if (dependentAsset == Parameters as Asset)
                    {
                        // TODO: We don't have access to the log here, so we are throwing an exception which is not really user friendly with the stacktrace exception
                        throw new InvalidOperationException($"Asset [{asset.Id}:{dependentAssetReference.Location}] cannot be used recursively");
                    }
                    
                    // Hash asset content (since it is embedded, not a real reference)
                    // Note: we hash child and not current, because when we start with main asset, it has already been hashed by base.ComputeParameterHash()
                    writer.SerializeExtended(ref dependentAsset, ArchiveMode.Serialize);

                    // Recurse
                    ComputeCompileTimeDependenciesHash(writer, dependentAsset);
                }
            }
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.Serialize(ref Version);
            
            var url = Url;
            var assetParameters = Parameters;
            writer.SerializeExtended(ref assetParameters, ArchiveMode.Serialize);
            writer.Serialize(ref url, ArchiveMode.Serialize);

            var asset = Parameters as Asset;
            if (asset != null)
            {
                ComputeCompileTimeDependenciesHash(writer, asset);
            }
        }

        public override string ToString()
        {
            // TODO provide automatic asset to string via YAML
            return Parameters.ToString();
        }

        private IEnumerable<ObjectUrl> GetInputFilesImpl()
        {
            var depsEnumerator = Parameters as IAssetCompileTimeDependencies;
            if (depsEnumerator == null) yield break;
            foreach (var reference in depsEnumerator.EnumerateCompileTimeDependencies(Package.Session))
            {
                if (reference != null)
                {
                    yield return new ObjectUrl(UrlType.Content, reference.Location);
                }
            }
        }
    }
}
