// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.EffectCompilerServer
{
    partial class Program
    {
        private static void TrackiOSDevices(ShaderCompilerHost shaderCompilerServer)
        {
            var connectedDevice = new ConnectedDevice();
            LaunchPersistentClient(connectedDevice, shaderCompilerServer, "192.168.2.146", 1245);
        }
    }
}