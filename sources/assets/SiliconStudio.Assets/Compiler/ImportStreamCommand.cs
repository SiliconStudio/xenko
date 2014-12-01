// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using System.Threading.Tasks;

namespace SiliconStudio.Assets.Compiler
{
    [Description("Import stream")]
    public class ImportStreamCommand : SingleFileImportCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import Stream "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public bool DisableCompression { get; set; }

        public bool SaveSourcePath { get; set; }

        protected TagSymbol DisableCompressionSymbol;

        public ImportStreamCommand()
        {
            DisableCompressionSymbol = RegisterTag(Builder.DoNotCompressTag, () => Builder.DoNotCompressTag);
            SaveSourcePath = false;
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            // This path for effects xml is now part of this tool, but it should be done in a separate exporter?
            using (var inputStream = File.OpenRead(SourcePath))
            using (var outputStream = AssetManager.FileProvider.OpenStream(Location, VirtualFileMode.Create, VirtualFileAccess.Write))
            {
                inputStream.CopyTo(outputStream);

                var objectURL = new ObjectUrl(UrlType.Internal, Location);

                if (DisableCompression)
                    commandContext.AddTag(objectURL, DisableCompressionSymbol);
            }

            if (SaveSourcePath)
            {
                // store absolute path to source
                // TODO: the "/path" is hardcoded, used in EffectSystem and ShaderSourceManager. Find a place to share this correctly.
                var pathLocation = new UFile(Location.FullPath + "/path");
                using (var outputStreamPath = AssetManager.FileProvider.OpenStream(pathLocation, VirtualFileMode.Create, VirtualFileAccess.Write))
                {
                    using (var sw = new StreamWriter(outputStreamPath))
                    {
                        sw.Write(SourcePath.FullPath);
                    }
                }
            }

            return Task.FromResult(ResultStatus.Successful);
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            yield return new ObjectUrl(UrlType.File, SourcePath);
        }

        public override string ToString()
        {
            return "Import stream " + (SourcePath ?? "[File]") + " > " + (Location ?? "[Location]");
        }
    }
}