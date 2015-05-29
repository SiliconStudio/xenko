// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The base command to build thumbnails.
    /// This command overrides <see cref="GetInputFiles"/> so that it automatically returns all the item asset reference files.
    /// By doing so the thumbnail is re-generated every time one of the dependencies changes.
    /// </summary>
    /// <typeparam name="T">The type of the asset parameter</typeparam>
    public abstract class ThumbnailCommand<T> : AssetCommand<T>
    {
        protected readonly AssetItem AssetItem;

        protected readonly PackageSession AssetsSession;

        protected ThumbnailCommand(string url, PackageSession assetsSession, AssetItem assetItem, T assetParameters)
            : base(url, assetParameters)
        {
            if (assetsSession == null) throw new ArgumentNullException("assetsSession");
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            if (url == null) throw new ArgumentNullException("url");

            AssetItem = assetItem;
            AssetsSession = assetsSession;
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);
            var dependencies = AssetsSession.DependencyManager.ComputeDependencies(AssetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive);
            foreach (var assetReference in dependencies.LinksOut)
            {
                var refAsset = assetReference.Item.Asset;
                writer.SerializeExtended(ref refAsset, ArchiveMode.Serialize);
            }
        }

        public override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFiles()
        {
            var dependencies = AssetsSession.DependencyManager.ComputeDependencies(AssetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive);
            foreach (var assetReference in dependencies.LinksOut)
                yield return new ObjectUrl(UrlType.Internal, assetReference.Item.Location);

            foreach (var inputFile in base.GetInputFiles())
                yield return inputFile;
        }
    }
}