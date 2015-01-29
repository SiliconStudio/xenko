// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialComputeColorKeys
    {
        public MaterialComputeColorKeys(ParameterKey<Texture> textureBaseKey, ParameterKey valueBaseKey)
        {
            //if (textureBaseKey == null) throw new ArgumentNullException("textureBaseKey");
            //if (valueBaseKey == null) throw new ArgumentNullException("valueBaseKey");
            TextureBaseKey = textureBaseKey;
            ValueBaseKey = valueBaseKey;
        }

        public ParameterKey<Texture> TextureBaseKey { get; private set; }

        public ParameterKey ValueBaseKey { get; private set; }
    }
}