// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    public class MaterialComputeColorKeys
    {
        public MaterialComputeColorKeys(ParameterKey<Texture> textureBaseKey, ParameterKey valueBaseKey, Color? defaultTextureValue = null)
        {
            //if (textureBaseKey == null) throw new ArgumentNullException("textureBaseKey");
            //if (valueBaseKey == null) throw new ArgumentNullException("valueBaseKey");
            TextureBaseKey = textureBaseKey;
            ValueBaseKey = valueBaseKey;
            DefaultTextureValue = defaultTextureValue;
        }

        public ParameterKey<Texture> TextureBaseKey { get; private set; }

        public ParameterKey ValueBaseKey { get; private set; }

        public Color? DefaultTextureValue { get; private set; }
    }
}