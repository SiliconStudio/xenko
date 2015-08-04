// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Rendering.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [CommandDependsOn(typeof(Paradox.Importer.AssimpNET.MeshConverter))]
    [Description("Import Assimp")]
    public class ImportAssimpCommand : ImportModelCommand
    {
        private static string[] supportedExtensions = { ".x", ".dae", ".dae", ".3ds", ".obj", ".blend" };

        /// <inheritdoc/>
        public override string Title { get { string title = "Import Assimp "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public static bool IsSupportingExtensions(string ext)
        {
            var extToLower = ext.ToLower();

            return !String.IsNullOrEmpty(ext) && supportedExtensions.Any(supExt => supExt.Equals(extToLower));
        }

        private Paradox.Importer.AssimpNET.MeshConverter CreateMeshConverter(ICommandContext commandContext)
        {
            return new Paradox.Importer.AssimpNET.MeshConverter(commandContext.Logger)
            {
                AllowUnsignedBlendIndices = this.AllowUnsignedBlendIndices
            };
        }

        protected override Rendering.Model LoadModel(ICommandContext commandContext, AssetManager assetManager)
        {
            var converter = CreateMeshConverter(commandContext);

            // Note: FBX exporter uses Materials for the mapping, but Assimp already uses indices so we can reuse them
            // We should still unify the behavior to be more consistent at some point (i.e. if model was changed on the HDD but not in the asset).
            // This should probably be better done during a large-scale FBX/Assimp refactoring.
            var sceneData = converter.Convert(SourcePath, Location);
            return sceneData;
        }

        protected override AnimationClip LoadAnimation(ICommandContext commandContext, AssetManager assetManager)
        {
            var meshConverter = this.CreateMeshConverter(commandContext);
            var sceneData = meshConverter.ConvertAnimation(SourcePath, Location);
            return sceneData;
        }

        public override string ToString()
        {
            return "Import Assimp " + base.ToString();
        }
    }
}