// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.BuildEngine
{
    public abstract class SingleFileImportCommand : IndexFileCommand
    {
        protected SingleFileImportCommand()
        {
        }

        protected SingleFileImportCommand(UFile location, UFile sourcePath)
        {
            Location = location;
            SourcePath = sourcePath;
        }

        /// <summary>
        /// Gets or sets the source path of the raw asset.
        /// </summary>
        /// <value>The source path.</value>
        public UFile SourcePath { get; set; }

        /// <summary>
        /// Gets or sets the destination location in the storage.
        /// </summary>
        /// <value>The location.</value>
        public UFile Location { get; set; }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.Write(SourcePath);
            writer.Write(Location);
        }
    }
}
