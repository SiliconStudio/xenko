// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    partial class Program
    {
        private static int LocalPort = 1244;
        private static string IpOverUsbParadoxName = "ParadoxEffectCompilerServer";

        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            var windowsPhonePortMapping = false;
            int exitCode = 0;

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2015 Silicon Studio Corporation. All Rights Reserved",
                    "Effect Compiler Server - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} command [options]*", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "register-windowsphone-portmapping", "Register Windows Phone IpOverUsb port mapping", v => windowsPhonePortMapping = true },
                };

            try
            {
                var commandArgs = p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                // Make sure path exists
                if (commandArgs.Count > 0)
                    throw new OptionException("This command expect no additional arguments", "");

                if (windowsPhonePortMapping)
                {
                    RegisterWindowsPhonePortMapping();
                    return 0;
                }

                var shaderCompilerServer = new ShaderCompilerHost();

                // Start server mode
                shaderCompilerServer.Listen(LocalPort);

                // Start Android management thread
                new Thread(() => TrackAndroidDevices(shaderCompilerServer)).Start();

                // Start iOS management thread
                //TrackiOSDevices(shaderCompilerServer);

                // Start Windows Phone management thread
                new Thread(() => TrackWindowsPhoneDevice(shaderCompilerServer)).Start();

                // Forbid process to terminate (unless ctrl+c)
                while (true) Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: {1}", exeName, e);
                if (e is OptionException)
                    p.WriteOptionDescriptions(Console.Out);
                exitCode = 1;
            }

            return exitCode;
        }

        private static void ManageDevices<T>(string deviceType, Dictionary<T, string> enumeratedDevices, Dictionary<T, ConnectedDevice> currentDevices, Action<ConnectedDevice> connectDevice)
        {
            // Stop tasks used by disconnected devices
            foreach (var oldDevice in currentDevices.ToArray())
            {
                if (enumeratedDevices.ContainsKey(oldDevice.Key))
                    continue;

                oldDevice.Value.DeviceDisconnected = true;
                currentDevices.Remove(oldDevice.Key);

                Console.WriteLine("{0} Device removed: {1} ({2})", deviceType, oldDevice.Value.Name, oldDevice.Key);
            }

            // Start new devices
            int startLocalPort = 1245;
            foreach (var androidDevice in enumeratedDevices)
            {
                if (currentDevices.ContainsKey(androidDevice.Key))
                    continue;

                var connectedDevice = new ConnectedDevice
                {
                    Key = androidDevice.Key,
                    Name = androidDevice.Value,
                    Type = deviceType,
                };
                currentDevices.Add(androidDevice.Key, connectedDevice);

                connectDevice(connectedDevice);
            }
        }

        private static async Task LaunchPersistentClient(ConnectedDevice connectedDevice, ShaderCompilerHost shaderCompilerServer, string address, int localPort)
        {
            while (!connectedDevice.DeviceDisconnected)
            {
                try
                {
                    await shaderCompilerServer.TryConnect(address, localPort);
                }
                catch (Exception)
                {
                    // Mute exceptions and try to connect again
                    // TODO: Mute connection only, not message loop?
                }

                await Task.Delay(200);
            }
        }
    }
}