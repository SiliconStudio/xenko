// Copyright (c) 2012-2016 Silicon Studio Corporation (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// File AUTO-GENERATED, do not edit!
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Extensions for <see cref="ILogger"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs the specified verbose message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The verbose message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Verbose(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Verbose, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified verbose message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The verbose message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Verbose(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Verbose(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified verbose message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The verbose message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Verbose(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Verbose(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified verbose message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The verbose message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Verbose(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Verbose(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified debug message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The debug message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Debug(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Debug, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified debug message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The debug message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Debug(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Debug(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified debug message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The debug message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Debug(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Debug(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified debug message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The debug message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Debug(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Debug(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified info message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The info message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Info(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Info, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified info message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The info message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Info(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Info(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified info message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The info message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Info(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Info(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified info message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The info message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Info(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Info(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified warning message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The warning message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Warning(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Warning, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The warning message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Warning(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Warning(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The warning message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Warning(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Warning(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified warning message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The warning message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Warning(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Warning(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified error message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The error message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Error(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Error, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The error message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Error(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Error(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The error message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Error(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Error(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified error message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The error message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Error(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Error(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified fatal message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The fatal message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Fatal(this ILogger logger, string message, Exception exception, CallerInfo callerInfo = null)
        {
            logger.Log(new LogMessage(logger.Module, LogMessageType.Fatal, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified fatal message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The fatal message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Fatal(this ILogger logger, string message, CallerInfo callerInfo = null)
        {
            Fatal(logger, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified fatal message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The fatal message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Fatal(this ILogger logger, string messageFormat, params object[] parameters)
        {
            Fatal(logger, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified fatal message with an exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFormat">The fatal message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete("This method will be removed in next release. You can use string interpolation to inline the formatting of your string.")]
        public static void Fatal(this ILogger logger, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Fatal(logger, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
    }
}
