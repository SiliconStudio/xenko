// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio
{
    public class AndroidDeviceEnumerator
    {
        /// <summary>
        /// Lists all the Android devices accessible from the computer.
        /// </summary>
        /// <returns>The list of all the available Android devices.</returns>
        public static AndroidDeviceDescription[] ListAndroidDevices()
        {
            var devices = new List<AndroidDeviceDescription>();

            ProcessOutputs devicesOutputs;

            try
            {
                devicesOutputs = ShellHelper.RunProcessAndGetOutput(@"adb", @"devices");
            }
            catch (Exception)
            {
                return new AndroidDeviceDescription[0];
            }

            var whitespace = new[] { ' ', '\t' };
            for (var i = 1; i < devicesOutputs.OutputLines.Count; ++i) // from the second line
            {
                var line = devicesOutputs.OutputLines[i];
                if (line != null)
                {
                    var res = line.Split(whitespace);
                    if (res.Length == 2)
                    {
                        AndroidDeviceDescription device;
                        device.Serial = res[0];
                        device.Name = res[1];
                        devices.Add(device);
                    }
                }
            }

            // Set the real name of the Android device.
            for (var i = 0; i < devices.Count; ++i)
            {
                var device = devices[i];
                //TODO: doing a grep instead will be better
                var deviceNameOutputs = ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} shell cat /system/build.prop", device.Serial));
                foreach (var line in deviceNameOutputs.OutputLines)
                {
                    if (line != null && line.StartsWith(@"ro.product.model")) // correct line
                    {
                        var parts = line.Split('=');

                        if (parts.Length > 1)
                        {
                            device.Name = parts[1];
                            devices[i] = device;
                        }

                        break; // no need to search further
                    }
                }
            }

            return devices.ToArray();
        }

        public struct AndroidDeviceDescription
        {
            public string Serial;
            public string Name;
        }
    }
}