// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
