// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderGeneratorContext
    {
        public MaterialShaderGeneratorContext()
        {
            PendingOperations = new Stack<ShaderSource>();
            Operations = new List<ShaderSource>();
            Streams = new Stack<HashSet<string>>();
            Streams.Push(new HashSet<string>());
        }

        public MaterialAsset FindMaterial(AssetReference<MaterialAsset> materialReference)
        {
            throw new NotImplementedException();
        }

        public Stack<ShaderSource> PendingOperations { get; private set; }

        public Stack<HashSet<string>> Streams { get; private set; } 

        public List<ShaderSource> Operations { get; private set; }

        public bool HasStream(string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Streams.Peek().Contains(stream);
        }
        public void PushStream(string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Streams.Peek().Add(stream);
        }

        public bool ExploreGenerics = false;

        public LoggerResult Log;
    }
}
