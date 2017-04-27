// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
