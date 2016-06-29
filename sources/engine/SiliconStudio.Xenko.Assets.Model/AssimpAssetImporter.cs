// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Importer.Common;

namespace SiliconStudio.Xenko.Assets.Model
{
    public class AssimpAssetImporter : ModelAssetImporter
    {
        // Supported file extensions for this importer
        private const string FileExtensions = ".dae;.3ds;.obj;.blend;.x;.md2;.md3;.dxf";

        private static readonly Guid Uid = new Guid("30243FC0-CEC7-4433-977E-95DCA29D846E");

        public override Guid Id => Uid;

        public override string Description => "Assimp importer used for creating entities, 3D Models or animations assets";

        public override string SupportedFileExtensions => FileExtensions;

        /// <inheritdoc/>
        public override EntityInfo GetEntityInfo(UFile localPath, Logger logger, AssetImporterParameters importParameters)
        {
            var meshConverter = new Importer.AssimpNET.MeshConverter(logger);
            var entityInfo = meshConverter.ExtractEntity(localPath.FullPath, null, importParameters.IsTypeSelectedForOutput(typeof(TextureAsset)));
            return entityInfo;
        }
    }
}
