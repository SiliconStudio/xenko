// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    partial class Program
    {
        private static void TrackAndroidDevices(ShaderCompilerHost shaderCompilerServer)
        {
            var currentAndroidDevices = new Dictionary<string, ConnectedDevice>();

            // Start new devices
            int startLocalPort = 51153;

            // Wait and process android device changes events
            while (true)
            {
                // Fill list of android devices
                var newAndroidDevices = new Dictionary<string, string>();
                foreach (var device in AndroidDeviceEnumerator.ListAndroidDevices())
                {
                    newAndroidDevices.Add(device.Serial, string.Format("{0} ({1})", device.Name, device.Serial));
                }

                ManageDevices("Android", newAndroidDevices, currentAndroidDevices, (connectedDevice) =>
                {
                    // First, try adb reverse port mapping (supported on newest adb+device)
                    // This is the best solution, as nothing specific needs to be done.
                    //var output = ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} reverse tcp:{1} tcp:{2}", newAndroidDevice, LocalPort, LocalPort));
                    //if (output.ExitCode == 0)
                    //    continue;

                    // Setup adb port forward (tries up to 5 times for open ports)
                    int localPort = 0;
                    int firstTestedLocalPort = startLocalPort;
                    int remotePort = 1245;
                    for (int i = 0; i < 4; ++i)
                    {
                        int testedLocalPort = startLocalPort++;
                        var output = ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} forward tcp:{1} tcp:{2}", connectedDevice.Key, startLocalPort, remotePort));

                        if (output.ExitCode == 0)
                        {
                            localPort = testedLocalPort;
                            Console.WriteLine("{0} Device connected: {1}; successfully mapped port {2}:{3}", connectedDevice.Type, connectedDevice.Name, testedLocalPort, remotePort);
                            break;
                        }
                    }

                    if (localPort == 0)
                    {
                        int lastTestedLocalPort = startLocalPort;
                        Console.WriteLine("{0} Device connected: {1}; error when mapping port [{2}-{3}]:{4}", connectedDevice.Type, connectedDevice.Name, firstTestedLocalPort, lastTestedLocalPort - 1, remotePort);
                        return;
                    }

                    // Launch a client thread that will automatically tries to connect to this port
                    Task.Run(() => LaunchPersistentClient(connectedDevice, shaderCompilerServer, "localhost", localPort));
                });

                Thread.Sleep(1000); // Detect new devices every 1000 msec
            }
        }
    }
}