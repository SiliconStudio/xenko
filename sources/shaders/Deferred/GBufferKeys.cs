// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Keys used for GBuffer settings.
    /// </summary>
    public static partial class GBufferKeys
    {
        /// <summary>
        /// The GBuffer render target key.
        /// </summary>
        public static readonly ParameterKey<Texture> Texture = ParameterKeys.New<Texture>();

        public static readonly ParameterKey<Texture> NormalPack = ParameterKeys.New<Texture>();
    }
}
