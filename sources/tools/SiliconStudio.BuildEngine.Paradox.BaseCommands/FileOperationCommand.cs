using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    [Description("File operation")]
    public class FileOperationCommand : Command
    {
        /// <inheritdoc/>
        public override string Title { get { return Type + " " + (Source ?? "[Source]"); } }

        public enum Operation
        {
            Move,
            Copy,
            Delete
        }

        public string Source { get; set; }
        public string Target { get; set; }
        public bool Overwrite { get; set; }
        public Operation Type { get; set; }

        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            try
            {
                switch (Type)
                {
                    case Operation.Move:
                        if (File.Exists(Target))
                            File.Delete(Target);

                        File.Move(Source, Target);
                        commandContext.RegisterOutput(new ObjectUrl(UrlType.File, Target), ObjectId.Empty);
                        break;
                    case Operation.Copy:
                        var sourceStream = File.OpenRead(Source);
                        var destStream = File.OpenWrite(Target);
                        await sourceStream.CopyToAsync(destStream);
                        commandContext.RegisterOutput(new ObjectUrl(UrlType.File, Target), ObjectId.Empty);
                        break;
                    case Operation.Delete:
                        File.Delete(Source);
                        break;
                }

                return ResultStatus.Successful;
            }
            catch (Exception e)
            {
                commandContext.Logger.Error(e.Message);
                return ResultStatus.Failed;
            }          
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            yield return new ObjectUrl(UrlType.File, Source);
        }

        protected override void ComputeParameterHash(Stream stream)
        {
            base.ComputeParameterHash(stream);

            var writer = new BinarySerializationWriter(stream);
            writer.Write(Source);
            writer.Write(Target);
            writer.Write(Type);
        }

        public override string ToString()
        {
            if (Type == Operation.Delete)
                return Type + " " + (Source ?? "[Source]");
            return Type + " " + (Source ?? "[Source]") + " > " + (Target ?? "[Target]");
        }
    }
}
