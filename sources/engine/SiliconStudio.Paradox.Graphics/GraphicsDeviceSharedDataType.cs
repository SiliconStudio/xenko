// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Type of shared data. <see cref="GraphicsDevice.GetOrCreateSharedData{T}"/>
    /// </summary>
    public enum GraphicsDeviceSharedDataType
    {
        /// <summary>
        /// Data is shared within a <see cref="SharpDX.Direct3D11.Device"/>.
        /// </summary>
        PerDevice,

        /// <summary>
        /// Data is shared within a <see cref="SharpDX.Direct3D11.DeviceContext"/>
        /// </summary>
        PerContext,
    }
}