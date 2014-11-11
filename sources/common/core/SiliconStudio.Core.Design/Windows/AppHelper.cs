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
            body.AppendFormat("OS Version: {0}\n", Environment.OSVersion);
            body.AppendFormat("Command Line Args: {0}\n", string.Join(" ", GetCommandLineArgs()));
            PrintExceptionRecursively(body, e);
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

        private static void PrintExceptionRecursively(StringBuilder builder, Exception exception, int indent = 0)
        {
            PrintException(builder, exception, indent);
            if (exception == null)
                return;

            var aggregate = exception as AggregateException;
            if (aggregate != null)
            {
                builder.AppendLine("The above exception is an aggregate exception. Printing inner exceptions:");
                // The InnerException is normally the first of the InnerExceptions.
                //if (aggregate.InnerException != null)
                //{
                //    PrintExceptionRecursively(builder, aggregate.InnerException, indent + 2);
                //}
                foreach (var innerException in aggregate.InnerExceptions)
                {
                    PrintExceptionRecursively(builder, innerException, indent + 2);
                }
            }
            else if (exception.InnerException != null)
            {
                builder.AppendLine("The above exception has an inner exception:");
                PrintExceptionRecursively(builder, exception.InnerException, indent + 2);
            }
        }

        private static void PrintException(StringBuilder builder, Exception exception, int indent)
        {
            if (exception == null)
            {
                builder.AppendFormat("{0}Exception type: (null)\n", Indent(indent));
            }
            else
            {
                builder.AppendFormat("{0}Exception type: {1}\n", Indent(indent), exception.GetType().Name);
                builder.AppendFormat("{0}Exception message: {1}\n", Indent(indent), exception.Message);
                builder.AppendFormat("{0}StackTrace: {1}\n", Indent(indent), exception.StackTrace);
            }
        }

        private static string Indent(int offset)
        {
            return "".PadLeft(offset);
        }
    }
}