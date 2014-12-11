// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public static class ImageEffectStepKeys
    {
        public static readonly ParameterKey<Texture> InputTexture = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> NullTexture = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> OutputTexture = ParameterKeys.New<Texture>();
    }
}