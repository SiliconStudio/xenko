// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Serialization.Assets
{
    public class AssetOperationParameters
    {
        public static AssetOperationParameters Default = new AssetOperationParameters();

        public bool ProcessChunks { get; set; }

        public HashSet<Type> ChunkTypes { get; set; }

        public AssetOperationParameters()
        {
            ProcessChunks = true;
        }
    }
}