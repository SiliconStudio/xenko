// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.IO;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [CommandDependsOn(typeof(Paradox.Importer.FBX.MeshConverter))]
    [Description("Import FBX")]
    public class ImportFbxCommand : ImportModelCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import FBX "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public bool InverseNormals { get; set; }
        
        public static bool IsSupportingExtensions(string ext)
        {
            return !String.IsNullOrEmpty(ext) && ext.ToLower().Equals(".fbx");
        }

        protected override ModelData LoadModel(ICommandContext commandContext, AssetManager assetManager)
        {
            var meshConverter = this.CreateMeshConverter(commandContext, assetManager);
            var sceneData = meshConverter.Convert(SourcePath, Location);
            return sceneData;
        }

        protected override AnimationClip LoadAnimation(ICommandContext commandContext, AssetManager assetManager)
        {
            var meshConverter = this.CreateMeshConverter(commandContext, assetManager);
            var sceneData = meshConverter.ConvertAnimation(SourcePath, Location);
            return sceneData;
        }

        private Paradox.Importer.FBX.MeshConverter CreateMeshConverter(ICommandContext commandContext, AssetManager assetManager)
        {
            return new Paradox.Importer.FBX.MeshConverter(commandContext.Logger)
                {
                    InverseNormals = this.InverseNormals,
                    TextureTagSymbol = this.TextureTagSymbol,
                    ViewDirectionForTransparentZSort = this.ViewDirectionForTransparentZSort,
                    AllowUnsignedBlendIndices = this.AllowUnsignedBlendIndices
                };
        }

        public override bool ShouldSpawnNewProcess()
        {
            return true;
        }

        public override string ToString()
        {
            return "Import FBX " + base.ToString();
        }
    }
}