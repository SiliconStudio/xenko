// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Core.Windows
{
    public static class AppHelper
    {
        public static string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }

        public static string BuildErrorMessage(Exception exception, string header = null)
        {
            var body = new StringBuilder();

            if (header != null)
            {
                body.Append(header);
            }
            body.AppendLine($"Current Directory: {Environment.CurrentDirectory}");
            body.AppendLine($"Command Line Args: {string.Join(" ", GetCommandLineArgs())}");
            body.AppendLine($"OS Version: {Environment.OSVersion} ({(Environment.Is64BitOperatingSystem ? "x64" : "x86")})");
            body.AppendLine($"Processor Count: {Environment.ProcessorCount}");
            body.AppendLine("Video configuration:");
            WriteVideoConfig(body);
            body.AppendLine($"Exception: {exception.FormatFull()}");
            return body.ToString();
        }

        public static string BuildErrorToClipboard(Exception exception, string header = null)
        {
            var errorMessage = BuildErrorMessage(exception, header);
            try
            {
                Clipboard.SetText(errorMessage);
            }
            catch (Exception e)
            {
                e.Ignore();
            }

            return errorMessage;
        }

        internal static void WriteMemoryInfo(StringBuilder writer)
        {
            // Not used yet, but we might want to include some of these info
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM CIM_OperatingSystem");

                foreach (var managementObject in searcher.Get().OfType<ManagementObject>())
                {
                    foreach (var property in managementObject.Properties)
                    {
                        writer.AppendLine($"{property.Name}: {property.Value}");
                    }
                }
            }
            catch (Exception)
            {
                writer.AppendLine("An error occurred while trying to retrieve memory information.");
            }
    }

        public static void WriteVideoConfig(StringBuilder writer)
        {
            try
            {
                var i = 0;
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (var managementObject in searcher.Get().OfType<ManagementObject>())
                {
                    writer.AppendLine($"GPU {++i}");
                    foreach (var property in managementObject.Properties)
                    {
                        writer.AppendLine(string.Format("  {0}: {1}", property.Name, property.Value));
                    }
                }
            }
            catch (Exception)
            {
                writer.AppendLine("An error occurred while trying to retrieve video configuration.");
            }
        }

        public static Dictionary<string, string> GetVideoConfig()
        {
            var result = new Dictionary<string, string>();

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                int deviceId = 0;
                foreach (var managementObject in searcher.Get().OfType<ManagementObject>())
                {
                    foreach (var property in managementObject.Properties)
                    {
                        if(property.Value == null) continue;
                        
                        result.Add($"GPU{deviceId}.{property.Name}", property.Value.ToString());
                    }
                    deviceId++;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        } 
    }
}