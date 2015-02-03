// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [CommandDependsOn(typeof(Paradox.Importer.AssimpNET.MeshConverter))]
    [Description("Import Assimp")]
    public class ImportAssimpCommand : ImportModelCommand
    {
        private static string[] supportedExtensions = { ".dae", ".dae", ".3ds", ".obj", ".blend" };

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
                ViewDirectionForTransparentZSort = this.ViewDirectionForTransparentZSort,
                AllowUnsignedBlendIndices = this.AllowUnsignedBlendIndices
            };
        }

        protected override ModelData LoadModel(ICommandContext commandContext, AssetManager assetManager)
        {
            var converter = CreateMeshConverter(commandContext);
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