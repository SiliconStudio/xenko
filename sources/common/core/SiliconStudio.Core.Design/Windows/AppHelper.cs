// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiliconStudio.Core.Windows
{
    public static class AppHelper
    {
        public static string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs().Skip(1).ToArray();
        }

        public static string BuildErrorToClipboard(Exception e, string header = null)
        {
            var body = new StringBuilder();

            if (header != null)
            {
                body.Append(header);
            }
            body.AppendFormat("User: {0}\n", Environment.UserName);
            body.AppendFormat("Current Directory: {0}\n", Environment.CurrentDirectory);
            body.AppendFormat("Command Line Args: {0}\n", string.Join(" ", GetCommandLineArgs()));
            if (e == null)
            {
                body.AppendFormat("Exception type: (null)\n");
            }
            else
            {
                body.AppendFormat("Exception type: {0}\n", e.GetType().Name);
                body.AppendFormat("Exception message: {0}\n", e.Message);
                body.AppendFormat("Inner exception type: {0}\n", e.InnerException != null ? e.InnerException.GetType().Name : null);
                body.AppendFormat("Inner exception message: {0}\n", e.InnerException != null ? e.InnerException.Message : null);
                body.AppendFormat("StackTrace: {0}\n", e.StackTrace);
            }
            var errorMessage = body.ToString();
            try
            {
                Clipboard.SetText(errorMessage);
            }
            catch (Exception)
            {
            }

            return errorMessage;
        }
    }
}