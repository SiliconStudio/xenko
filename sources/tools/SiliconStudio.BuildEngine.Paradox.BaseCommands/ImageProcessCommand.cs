using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.BuildEngine
{
    [Description("Process image")]
    public class ImageProcessCommand : IndexFileCommand
    {
        /// <inheritdoc/>
        public override string Title { get { return "Process image " + (InputUrl ?? "[InputUrl]"); } }

        public string InputUrl { get; set; }
        public string OutputUrl { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public bool IsAbsolute { get; set; }

        public PixelFormat? Format { get; set; }

        public bool GenerateMipmaps { get; set; }

        public ImageProcessCommand()
        {
            Width = 100.0f;
            Height = 100.0f;
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            yield return new ObjectUrl(UrlType.Internal, InputUrl);
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            // Load image
            var image = assetManager.Load<Image>(InputUrl);

            // Initialize TextureTool library
            using (var texTool = new TextureTool())
            using (var texImage = texTool.Load(image))
            {
                var outputFormat = Format.HasValue ? Format.Value : image.Description.Format;

                // Apply transformations
                texTool.Decompress(texImage);
                if (IsAbsolute)
                {
                    texTool.Resize(texImage, (int)Width, (int)Height, Filter.Rescaling.Lanczos3);
                }
                else
                {
                    texTool.Rescale(texImage, Width / 100.0f, Height / 100.0f, Filter.Rescaling.Lanczos3);
                }

                // Generate mipmaps
                if (GenerateMipmaps)
                {
                    texTool.GenerateMipMaps(texImage, Filter.MipMapGeneration.Box);
                }

                // Convert/Compress to output format
                texTool.Compress(texImage, outputFormat);

                // Save
                using (var outputImage = texTool.ConvertToParadoxImage(texImage))
                {
                    assetManager.Save(OutputUrl, outputImage);

                    commandContext.Logger.Verbose("Compression successful [{3}] to ({0}x{1},{2})",
                                                  outputImage.Description.Width,
                                                  outputImage.Description.Height, outputImage.Description.Format, OutputUrl);
                }
            }

            return Task.FromResult(ResultStatus.Successful);
        }

        protected override void ComputeParameterHash(Stream stream)
        {
            base.ComputeParameterHash(stream);

            // Really necesary? (need system for identical blob reuse)
            var writer = new BinarySerializationWriter(stream);
            writer.Write(InputUrl);
            writer.Write(OutputUrl);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(IsAbsolute);
            writer.Write(Format);
            writer.Write(GenerateMipmaps);
        }

        public override string ToString()
        {
            return "Process image " + (InputUrl ?? "[InputUrl]") + " > " + (OutputUrl ?? "[OutputUrl]");
        }
    }
}