// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Importer.Common;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Data;

namespace SiliconStudio.Xenko.Assets.Model
{
    [CommandDependsOn(typeof(Xenko.Importer.AssimpNET.MeshConverter))]
    [Description("Import Assimp")]
    public class ImportAssimpCommand : ImportModelCommand
    {
        private static string[] supportedExtensions = { ".x", ".dae", ".dae", ".3ds", ".obj", ".blend" };

        /// <inheritdoc/>
        public override string Title { get { string title = "Import Assimp "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public static bool IsSupportingExtensions(string ext)
        {
            if (string.IsNullOrEmpty(ext))
                return false;

            var extToLower = ext.ToLower();

            return supportedExtensions.Any(supExt => supExt.Equals(extToLower));
        }

        private Xenko.Importer.AssimpNET.MeshConverter CreateMeshConverter(ICommandContext commandContext)
        {
            return new Xenko.Importer.AssimpNET.MeshConverter(commandContext.Logger)
            {
                AllowUnsignedBlendIndices = this.AllowUnsignedBlendIndices,
                ScaleImport = ScaleImport
            };
        }

        protected override Rendering.Model LoadModel(ICommandContext commandContext, ContentManager contentManager)
        {
            var converter = CreateMeshConverter(commandContext);

            // Note: FBX exporter uses Materials for the mapping, but Assimp already uses indices so we can reuse them
            // We should still unify the behavior to be more consistent at some point (i.e. if model was changed on the HDD but not in the asset).
            // This should probably be better done during a large-scale FBX/Assimp refactoring.
            var sceneData = converter.Convert(SourcePath, Location);
            return sceneData;
        }

        protected override Dictionary<string, AnimationClip> LoadAnimation(ICommandContext commandContext, ContentManager contentManager, out TimeSpan duration)
        {
            var meshConverter = this.CreateMeshConverter(commandContext);
            var sceneData = meshConverter.ConvertAnimation(SourcePath, Location);
            duration = sceneData.Duration;
            return sceneData.AnimationClips;
        }

        protected override Skeleton LoadSkeleton(ICommandContext commandContext, ContentManager contentManager)
        {
            var meshConverter = this.CreateMeshConverter(commandContext);
            var sceneData = meshConverter.ConvertSkeleton(SourcePath, Location);
            return sceneData;
        }

        public override string ToString()
        {
            return "Import Assimp " + base.ToString();
        }
    }
}