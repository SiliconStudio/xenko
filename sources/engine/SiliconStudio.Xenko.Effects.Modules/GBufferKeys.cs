// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Graphics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used for GBuffer settings.
    /// </summary>
    public static class GBufferKeys
    {
        /// <summary>
        /// The GBuffer render target key.
        /// </summary>
        public static readonly ParameterResourceKey<Texture2D> Texture = ParameterKeys.Resource<Texture2D>();

        public static readonly ParameterResourceKey<Texture2D> NormalPack = ParameterKeys.Resource<Texture2D>();
    }
}
