using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    [Description("Import stream")]
    public class ImportStreamCommand : SingleFileImportCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import Stream "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            // This path for effects xml is now part of this tool, but it should be done in a separate exporter?
            using (var inputStream = File.OpenRead(SourcePath))
            using (var outputStream = AssetManager.FileProvider.OpenStream(Location, VirtualFileMode.Create, VirtualFileAccess.Write))
            {
                await inputStream.CopyToAsync(outputStream);
            }

            return ResultStatus.Successful;
        }

        protected override void ComputeParameterHash(Stream stream)
        {
            base.ComputeParameterHash(stream);

            var writer = new BinarySerializationWriter(stream);
            writer.Write(SourcePath);
            writer.Write(Location);
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