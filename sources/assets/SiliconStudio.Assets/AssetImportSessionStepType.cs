// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// The step being processed by the <see cref="AssetImportSession"/>
    /// </summary>
    public enum AssetImportSessionStepType
    {
        /// <summary>
        /// The asset is being staged into the <see cref="AssetImportSession"/> and 
        /// is calling each importer to generate the list of assets to import.
        /// </summary>
        Staging,

        /// <summary>
        /// The <see cref="AssetImportSession"/> is calculating hash for assets to import.
        /// </summary>
        ComputeHash,

        /// <summary>
        /// The <see cref="AssetImportSession"/> is trying to match assets to import with existing assets
        /// from the current session.
        /// </summary>
        Matching,

        /// <summary>
        /// The <see cref="AssetImportSession"/> is merging assets to import with selected previous assets.
        /// </summary>
        Merging,

        /// <summary>
        /// The <see cref="AssetImportSession"/> is importing assets into the session.
        /// </summary>
        Importing
    }
}