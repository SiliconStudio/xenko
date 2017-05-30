// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Importer.Common;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class AssimpAssetImporter : ModelAssetImporter
    {
        static AssimpAssetImporter()
        {
            NativeLibrary.PreloadLibrary("assimp-vc120-mt.dll");
        }

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

        /// <inheritdoc/>
        public override void GetAnimationDuration(UFile localPath, Logger logger, AssetImporterParameters importParameters, out TimeSpan startTime, out TimeSpan endTime)
        {
            var meshConverter = new Importer.AssimpNET.MeshConverter(logger);
            var sceneData = meshConverter.ConvertAnimation(localPath.FullPath, "");

            startTime = CompressedTimeSpan.MaxValue; // This will go down, so we start from positive infinity
            endTime = CompressedTimeSpan.MinValue;   // This will go up, so we start from negative infinity

            foreach (var animationClip in sceneData.AnimationClips)
            {
                foreach (var animationCurve in animationClip.Value.Curves)
                {
                    foreach (var compressedTimeSpan in animationCurve.Keys)
                    {
                        if (compressedTimeSpan < startTime)
                            startTime = compressedTimeSpan;
                        if (compressedTimeSpan > endTime)
                            endTime = compressedTimeSpan;
                    }
                }
            }

            if (startTime == CompressedTimeSpan.MaxValue)
                startTime = CompressedTimeSpan.Zero;
            if (endTime == CompressedTimeSpan.MinValue)
                endTime = CompressedTimeSpan.Zero;
        }
    }
}
