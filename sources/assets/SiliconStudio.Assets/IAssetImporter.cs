// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Imports a raw asset into the asset system.
    /// </summary>
    public interface IAssetImporter
    {
        /// <summary>
        /// Gets an unique identifier to identify the importer. See remarks.
        /// </summary>
        /// <value>The identifier.</value>
        /// <remarks>This identifier is used to recover the importer used for a particular asset. This 
        /// Guid must be unique and stored statically in the definition of an importer. It is used to 
        /// reimport an existing asset with the same importer.
        /// </remarks>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of this importer.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the description of this importer.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// Gets the supported file extensions (separated by ',' for multiple extensions) by this importer.
        /// </summary>
        /// <returns>Returns a list of supported file extensions handled by this importer.</returns>
        string SupportedFileExtensions { get; }

        /// <summary>
        /// Gets the default parameters for this importer.
        /// </summary>
        /// <param name="isForReImport"></param>
        /// <value>The supported types.</value>
        AssetImporterParameters GetDefaultParameters(bool isForReImport);

        /// <summary>
        /// Gets the rank of this importer, higher is the value, higher the importer is important or commonly used. Default is <c>100</c>.
        /// </summary>
        /// <value>The rank.</value>
        // TODO this could be done also at runtime dynamically based on real usage
        int DisplayRank { get; }

        /// <summary>
        /// Imports a raw assets from the specified path into the specified package.
        /// </summary>
        /// <param name="rawAssetPath">The path to a raw asset on the disk.</param>
        /// <param name="importParameters">The parameters. It is mandatory to call <see cref="GetDefaultParameters"/> and pass the parameters instance here</param>
        IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters);
    }

    public static class AssetImportExtensions
    {
        public static bool IsSupportingFile(this IAssetImporter importer, UFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (file.GetFileExtension() == null) return false;

            return FileUtility.GetFileExtensionsAsSet(importer.SupportedFileExtensions).Contains(file.GetFileExtension());
        }
    }

}