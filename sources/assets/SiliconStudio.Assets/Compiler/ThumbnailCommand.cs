// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The base command to build thumbnails.
    /// This command overrides <see cref="GetInputFiles"/> so that it automatically returns all the item asset reference files.
    /// By doing so the thumbnail is re-generated every time one of the dependencies changes.
    /// </summary>
    public abstract class ThumbnailCommand : AssetCommand<ThumbnailCommandParameters>
    {
        private readonly AssetItem assetItem;

        protected ThumbnailCommand(string url, AssetItem assetItem, ThumbnailCommandParameters assetParameters)
            : base(url, assetParameters)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            if (assetItem.Package == null) throw new ArgumentException("assetItem is not attached to a package");
            if (assetItem.Package.Session == null) throw new ArgumentException("assetItem is not attached to a package session");
            if (url == null) throw new ArgumentNullException("url");

            this.assetItem = assetItem;
        }

        protected UFile AssetUrl { get { return assetItem.Location; } }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);
            var dependencies = assetItem.Package.Session.DependencyManager.ComputeDependencies(assetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive);
            foreach (var assetReference in dependencies.LinksOut)
            {
                var refAsset = assetReference.Item.Asset;
                writer.SerializeExtended(ref refAsset, ArchiveMode.Serialize);
            }
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            var dependencies = assetItem.Package.Session.DependencyManager.ComputeDependencies(assetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive);
            foreach (var assetReference in dependencies.LinksOut)
                yield return new ObjectUrl(UrlType.Internal, assetReference.Item.Location);

            foreach (var inputFile in base.GetInputFiles())
                yield return inputFile;
        }
    }
}