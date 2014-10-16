// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A raw asset being imported that will generate possibly multiple <see cref="AssetItem"/>
    /// </summary>
    [DebuggerDisplay("Import [{File}] Importers [{ByImporters.Count}]")]
    public class AssetToImport
    {
        private readonly UFile file;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetToImport"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <exception cref="System.ArgumentNullException">file</exception>
        internal AssetToImport(UFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            this.file = file;
            ByImporters = new List<AssetToImportByImporter>();
            Enabled = true;
        }

        /// <summary>
        /// Gets the file/raw asset being imported.
        /// </summary>
        /// <value>The file.</value>
        public UFile File
        {
            get
            {
                return file;
            }
        }

        /// <summary>
        /// Gets the list of importers and asset to import by importers.
        /// </summary>
        /// <value>The by importers.</value>
        public List<AssetToImportByImporter> ByImporters { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AssetToImport"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        internal Package Package { get; set; }

        internal UDirectory Directory { get; set; }
    }
}