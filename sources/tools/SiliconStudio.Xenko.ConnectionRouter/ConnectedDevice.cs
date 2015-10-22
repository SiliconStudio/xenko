// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.ConnectionRouter
{
    /// <summary>
    /// Represents a connected device that the connection router is forwarding connections to.
    /// </summary>
    class ConnectedDevice
    {
        public object Key { get; set; }
        public string Name { get; set; }
        public bool DeviceDisconnected { get; set; }
    }
}