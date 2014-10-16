// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Base factory for <see cref="SamplerState"/>.
    /// </summary>
    public class SamplerStateFactory : GraphicsResourceFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateFactory"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal SamplerStateFactory(GraphicsDevice device) : base(device)
        {
            PointWrap = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Point, TextureAddressMode.Wrap)).KeepAliveBy(this);
            PointWrap.Name = "SamplerState.PointWrap";

            PointClamp = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Point, TextureAddressMode.Clamp)).KeepAliveBy(this);
            PointClamp.Name = "SamplerState.PointClamp";

            LinearWrap = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Wrap)).KeepAliveBy(this);
            LinearWrap.Name = "SamplerState.LinearWrap";

            LinearClamp = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Clamp)).KeepAliveBy(this);
            LinearClamp.Name = "SamplerState.LinearClamp";

            AnisotropicWrap = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Anisotropic, TextureAddressMode.Wrap)).KeepAliveBy(this);
            AnisotropicWrap.Name = "SamplerState.AnisotropicWrap";

            AnisotropicClamp = SamplerState.New(device, new SamplerStateDescription(TextureFilter.Anisotropic, TextureAddressMode.Clamp)).KeepAliveBy(this);
            AnisotropicClamp.Name = "SamplerState.AnisotropicClamp";
        }

        /// <summary>
        /// Default state for point filtering with texture coordinate wrapping.
        /// </summary>
        public readonly SamplerState PointWrap;

        /// <summary>
        /// Default state for point filtering with texture coordinate clamping.
        /// </summary>
        public readonly SamplerState PointClamp;

        /// <summary>
        /// Default state for linear filtering with texture coordinate wrapping.
        /// </summary>
        public readonly SamplerState LinearWrap;

        /// <summary>
        /// Default state for linear filtering with texture coordinate clamping.
        /// </summary>
        public readonly SamplerState LinearClamp;

        /// <summary>
        /// Default state for anisotropic filtering with texture coordinate wrapping.
        /// </summary>
        public readonly SamplerState AnisotropicWrap;

        /// <summary>
        /// Default state for anisotropic filtering with texture coordinate clamping.
        /// </summary>
        public readonly SamplerState AnisotropicClamp;
    }
}

