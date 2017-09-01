// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Assets
{
    public sealed class RawAssetImporter : RawAssetImporterBase<RawAsset>
    {
        private static readonly Guid Uid = new Guid("6f86ec95-c1ca-41e1-8adc-1449bb5ce3be");

        public RawAssetImporter()
        {
            // Raw asset is always last
            Order = int.MaxValue;
        }

        /// <inheritdoc />
        public override Guid Id => Uid;

        /// <inheritdoc />
        public override string Description => "Generic importer for raw assets";

        /// <inheritdoc />
        public override string SupportedFileExtensions => "*.*";

        /// <inheritdoc />
        public override bool IsSupportingFile(string filePath)
        {
            // Always return true
            return true;
        }

    }
}
