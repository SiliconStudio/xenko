// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("PrefabAsset")]
    [AssetDescription(FileExtension, false)]
    //[AssetCompiler(typeof(SceneAssetCompiler))]
    [Display("Prefab")]
    public class PrefabAsset : EntityGroupAssetBase
    {
        public const int AssetFormatVersion = 0;

        /// <summary>
        /// The default file extension used by the <see cref="PrefabAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefab";
    }
}