// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Core.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Represents the maximum number of lines to include in the stack trace when formatting a exception to be displayed in a dialog.
        /// </summary>
        public const int MaxStackTraceLines = 30;

        /// <summary>
        /// Explicitly ignores the exception. This method does nothing but suppress warnings related to a catch block doing nothing.
        /// </summary>
        /// <param name="exception">The exception to ignore.</param>
        public static void Ignore(this Exception exception)
        {
            // Intentionally does nothing.
        }

        /// <summary>
        /// Formats the exception to be displayed in a dialog message. This methods will limit the number of lines to the value of <see cref="MaxStackTraceLines"/>.
        /// </summary>
        /// <param name="exception">The exception to format</param>
        /// <param name="startWithNewLine">Indicate whether a <see cref="Environment.NewLine"/> symbol should be included at the beginning of the resulting string.</param>
        /// <returns>A string representing the exception formatted for dialog message.</returns>
        public static string FormatForDialog(this Exception exception, bool startWithNewLine)
        {
            // Get the innermost exception.
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            var stackTrace = ExtractStackTrace(exception, MaxStackTraceLines);
            return string.Format("{0}{1}{2}{3}", startWithNewLine ? Environment.NewLine : "", exception.Message, Environment.NewLine, stackTrace);
        }

        /// <summary>
        /// Formats the exception to be displayed in a log or report. This method will process <see cref="AggregateException"/>,
        /// expand <see cref="Exception.InnerException"/>, and does not limit the number of line of the resulting string.
        /// </summary>
        /// <param name="exception">The exception to format</param>
        /// <returns>A string representing the exception formatted for log or report.</returns>
        public static string FormatForReport(this Exception exception)
        {
            var message = string.Format("{0}{1}{2}{3}", exception.Message, Environment.NewLine, ExtractStackTrace(exception), Environment.NewLine);
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                message += "AggregateException - InnerExceptions:" + Environment.NewLine;
                message += aggregateException.InnerExceptions.Aggregate(message, (current, innerException) => current + FormatForReport(exception));
            }

            if (exception.InnerException != null)
            {
                message += "InnerException:" + Environment.NewLine;
                message += FormatForReport(exception.InnerException);
            }
            return message;
        }

        /// <summary>
        /// Extracts the stack trace from an exception, formatting it correctly and limiting the number of lines if needed.
        /// </summary>
        /// <param name="exception">The exception from which to extract the stack trace.</param>
        /// <param name="maxLines">The maximum number of lines to return in the resulting string. Zero or negative numbers mean no limit.</param>
        /// <returns>A properly formated string containing the stack trace.</returns>
        public static string ExtractStackTrace(this Exception exception, int maxLines = -1)
        {
            var stackTraceArray = exception.StackTrace.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, maxLines > 0 ? stackTraceArray.Take(maxLines) : stackTraceArray);
        }
    }
}
