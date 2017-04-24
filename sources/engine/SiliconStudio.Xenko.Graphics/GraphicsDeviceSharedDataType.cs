// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Graphics
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
