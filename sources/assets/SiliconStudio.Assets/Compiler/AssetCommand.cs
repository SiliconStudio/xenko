// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A command processing an <see cref="Asset"/>.
    /// </summary>
    public abstract class AssetCommand : IndexFileCommand
    {
    }

    public abstract class AssetCommand<T> : AssetCommand
    {
        protected T asset;

        public string Url { get; set; }

        public T Asset
        {
            get { return asset; }
            set { asset = value; }
        }

        protected AssetCommand()
        {
        }

        protected AssetCommand(string url, T asset)
        {
            this.Url = url;
            this.asset = asset;
        }

        public override string Title
        {
            get
            {
                return string.Format("Asset command processing {0}", Url);
            }
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);
            
            var url = Url;
            writer.Serialize(ref asset, ArchiveMode.Serialize);
            writer.Serialize(ref url, ArchiveMode.Serialize);
        }

        public override string ToString()
        {
            // TODO provide automatic asset to string via YAML
            return asset.ToString();
        }
    }
}