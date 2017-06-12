// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.BuildEngine
{
    public abstract class SingleFileImportCommand : IndexFileCommand
    {
        /// <summary>
        /// This is useful if the asset binary format has changed and we want to bump the version to force re-evaluation/compilation of the command
        /// </summary>
        protected int Version;

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

            writer.Write(Version);

            writer.Write(SourcePath);
            writer.Write(Location);
        }
    }
}
