// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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