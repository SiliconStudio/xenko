// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Rendering.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [CommandDependsOn(typeof(Paradox.Importer.FBX.MeshConverter))]
    [Description("Import FBX")]
    public class ImportFbxCommand : ImportModelCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import FBX "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public static bool IsSupportingExtensions(string ext)
        {
            return !String.IsNullOrEmpty(ext) && ext.ToLower().Equals(".fbx");
        }

        protected override Rendering.Model LoadModel(ICommandContext commandContext, AssetManager assetManager)
        {
            var meshConverter = this.CreateMeshConverter(commandContext, assetManager);
            var materialMapping = Materials.Select((s, i) => new { Value = s, Index = i }).ToDictionary(x => x.Value.Name, x => x.Index);
            var sceneData = meshConverter.Convert(SourcePath, Location, materialMapping);
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
                    TextureTagSymbol = this.TextureTagSymbol,
                    AllowUnsignedBlendIndices = this.AllowUnsignedBlendIndices,
                    ScaleImport = this.ScaleImport
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