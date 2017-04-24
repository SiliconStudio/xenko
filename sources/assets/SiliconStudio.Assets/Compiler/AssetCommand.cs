// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A command processing an <see cref="Asset"/>.
    /// </summary>
    public abstract class AssetCommand : IndexFileCommand
    {
        public string Url { get; set; }

        protected AssetCommand(string url)
        {
            Url = url;
        }
    }

    public abstract class AssetCommand<T> : AssetCommand
    {
        protected readonly Package Package;

        /// <summary>
        /// This is useful if the asset binary format has changed and we want to bump the version to force re-evaluation/compilation of the command
        /// </summary>
        protected int Version;

        protected AssetCommand(string url, T parameters, Package package)
            : base (url)
        {
            Parameters = parameters;
            Package = package;
        }

        public T Parameters { get; set; }
        
        public override string Title => $"Asset command processing {Url}";

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.Serialize(ref Version);
            
            var url = Url;
            var assetParameters = Parameters;
            writer.SerializeExtended(ref assetParameters, ArchiveMode.Serialize);
            writer.Serialize(ref url, ArchiveMode.Serialize);
        }

        public override string ToString()
        {
            // TODO provide automatic asset to string via YAML
            return Parameters.ToString();
        }
    }
}
