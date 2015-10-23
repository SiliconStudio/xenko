using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    [Description("Import image")]
    public class ImportImageCommand : SingleFileImportCommand
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "Import Image "; try { title += Path.GetFileName(SourcePath) ?? "[File]"; } catch { title += "[INVALID PATH]"; } return title; } }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            Image image;
            using (var fileStream = new FileStream(SourcePath, FileMode.Open, FileAccess.Read))
            {
                image = Image.Load(fileStream);
            }
            assetManager.Save(Location, image);
            image.Dispose();

            return Task.FromResult(ResultStatus.Successful);
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
            return "Import image " + (SourcePath ?? "[File]") + " > " + (Location ?? "[Location]");
        }
    }
}