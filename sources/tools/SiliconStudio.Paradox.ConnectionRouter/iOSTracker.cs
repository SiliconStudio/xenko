// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    static class iOSTracker
    {
        public static void TrackDevices(Router router)
        {
            var connectedDevice = new ConnectedDevice();
            
            // TODO: How to control remote server IP/port?
            DeviceHelper.LaunchPersistentClient(connectedDevice, router, "macosx-host", RouterClient.DefaultListenPort);
        }
    }
}