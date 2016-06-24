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
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    [CommandDependsOn(typeof(Importer.FBX.MeshConverter))]
    [Description("Import FBX")]
    public class ImportFbxCommand : ImportModelCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import FBX "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public static bool IsSupportingExtensions(string ext)
        {
            return !String.IsNullOrEmpty(ext) && ext.ToLower().Equals(".fbx");
        }

        protected override Rendering.Model LoadModel(ICommandContext commandContext, ContentManager contentManager)
        {
            var meshConverter = CreateMeshConverter(commandContext);
            var materialMapping = Materials.Select((s, i) => new { Value = s, Index = i }).ToDictionary(x => x.Value.Name, x => x.Index);
            var sceneData = meshConverter.Convert(SourcePath, Location, materialMapping);
            return sceneData;
        }

        protected override Dictionary<string, AnimationClip> LoadAnimation(ICommandContext commandContext, ContentManager contentManager, out TimeSpan duration)
        {
            var meshConverter = CreateMeshConverter(commandContext);
            var sceneData = meshConverter.ConvertAnimation(SourcePath, Location);
            duration = sceneData.Duration;
            return sceneData.AnimationClips;
        }

        protected override Skeleton LoadSkeleton(ICommandContext commandContext, ContentManager contentManager)
        {
            var meshConverter = CreateMeshConverter(commandContext);
            var sceneData = meshConverter.ConvertSkeleton(SourcePath, Location);
            return sceneData;
        }

        private Importer.FBX.MeshConverter CreateMeshConverter(ICommandContext commandContext)
        {
            return new Importer.FBX.MeshConverter(commandContext.Logger)
                {
                    AllowUnsignedBlendIndices = AllowUnsignedBlendIndices,
                    ScaleImport = ScaleImport
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