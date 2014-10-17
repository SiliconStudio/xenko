// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// File AUTO-GENERATED, do not edit!
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Extensions for <see cref="ILogger"/>.
    /// </summary>
    public static partial class LoggerExtensions
    {
        /// <summary>
        /// Logs the specified verbose message with an exception.
        /// </summary>
        /// <param name="message">The verbose message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Verbose(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Verbose, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified verbose message.
        /// </summary>
        /// <param name="message">The verbose message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Verbose(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Verbose(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified verbose message.
        /// </summary>
        /// <param name="messageFormat">The verbose message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Verbose(this ILogger log, string messageFormat, params object[] parameters)
        {
            Verbose(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified verbose message with an exception.
        /// </summary>
        /// <param name="messageFormat">The verbose message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Verbose(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Verbose(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified debug message with an exception.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Debug(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Debug, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified debug message.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Debug(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Debug(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified debug message.
        /// </summary>
        /// <param name="messageFormat">The debug message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Debug(this ILogger log, string messageFormat, params object[] parameters)
        {
            Debug(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified debug message with an exception.
        /// </summary>
        /// <param name="messageFormat">The debug message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Debug(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Debug(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified info message with an exception.
        /// </summary>
        /// <param name="message">The info message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Info(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Info, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified info message.
        /// </summary>
        /// <param name="message">The info message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Info(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Info(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified info message.
        /// </summary>
        /// <param name="messageFormat">The info message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Info(this ILogger log, string messageFormat, params object[] parameters)
        {
            Info(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified info message with an exception.
        /// </summary>
        /// <param name="messageFormat">The info message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Info(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Info(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified warning message with an exception.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Warning(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Warning, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Warning(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Warning(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="messageFormat">The warning message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Warning(this ILogger log, string messageFormat, params object[] parameters)
        {
            Warning(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified warning message with an exception.
        /// </summary>
        /// <param name="messageFormat">The warning message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Warning(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Warning(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified error message with an exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Error(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Error, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Error(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Error(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="messageFormat">The error message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Error(this ILogger log, string messageFormat, params object[] parameters)
        {
            Error(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified error message with an exception.
        /// </summary>
        /// <param name="messageFormat">The error message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Error(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Error(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
        /// <summary>
        /// Logs the specified fatal message with an exception.
        /// </summary>
        /// <param name="message">The fatal message.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Fatal(this ILogger log, string message, Exception exception, CallerInfo callerInfo = null)
        {
            log.Log(new LogMessage(log.Module, LogMessageType.Fatal, message, exception, callerInfo));
        }

        /// <summary>
        /// Logs the specified fatal message.
        /// </summary>
        /// <param name="message">The fatal message.</param>
        /// <param name="callerInfo">Information about the caller. Default is null, otherwise use <see cref="CallerInfo.Get"/>.</param>
        public static void Fatal(this ILogger log, string message, CallerInfo callerInfo = null)
        {
            Fatal(log, message, null, callerInfo);
        }

        /// <summary>
        /// Logs the specified fatal message.
        /// </summary>
        /// <param name="messageFormat">The fatal message to format.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        public static void Fatal(this ILogger log, string messageFormat, params object[] parameters)
        {
            Fatal(log, messageFormat, null, parameters);
        }

        /// <summary>
        /// Logs the specified fatal message with an exception.
        /// </summary>
        /// <param name="messageFormat">The fatal message to format.</param>
        /// <param name="exception">An exception to log with the message.</param>
        /// <param name="parameters">The parameters to used with the <see cref="messageFormat" />. The last parameter can be used to store <see cref="CallerInfo"/></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void Fatal(this ILogger log, string messageFormat, Exception exception, params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Fatal(log, string.Format(messageFormat, parameters), exception, Logger.ExtractCallerInfo(parameters));
        }
    }
}