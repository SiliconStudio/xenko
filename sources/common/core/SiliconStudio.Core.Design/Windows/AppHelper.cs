// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
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
            body.AppendLine(string.Format("Current Directory: {0}", Environment.CurrentDirectory));
            body.AppendLine(string.Format("Command Line Args: {0}", string.Join(" ", GetCommandLineArgs())));
            body.AppendLine(string.Format("OS Version: {0} ({1})", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "x64" : "x86"));
            body.AppendLine(string.Format("Processor Count: {0}", Environment.ProcessorCount));
            body.AppendLine("Video configuration:");
            WriteVideoConfig(body);
            body.AppendLine(string.Format("Exception: {0}", exception.FormatFull()));
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
                    foreach (PropertyData property in managementObject.Properties)
                    {
                        writer.AppendLine(string.Format("{0}: {1}", property.Name, property.Value));
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
                int i = 0;
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");
                foreach (var managementObject in searcher.Get().OfType<ManagementObject>())
                {
                    writer.AppendLine(string.Format("GPU {0}", ++i));
                    foreach (PropertyData property in managementObject.Properties)
                    {
                        writer.AppendLine(string.Format("{0}: {1}", property.Name, property.Value));
                    }
                }
            }
            catch (Exception)
            {
                writer.AppendLine("An error occurred while trying to retrieve video configuration.");
            }
        }
    }
}