// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Gyroscope. It measures the rotation speed of device along the x/y/z axis.
    /// </summary>
    public interface IGyroscopeSensor : ISensorDevice
    {
        /// <summary>
        /// Gets the current rotation speed of the device along x/y/z axis.
        /// </summary>
        Vector3 RotationRate { get; }
    }
}