// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Diagnostics
{
    /// <summary>
    /// Extension to <see cref="Logger"/> for loggin specific error with assets.
    /// </summary>
    public static class AssetLoggerExtensions
    {
        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, params object[] arguments)
        {
            Error(logger, package, assetReference, code, (IEnumerable<IContentReference>)null, arguments);
        }

        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IEnumerable<IContentReference> relatedGuids, params object[] arguments)
        {
            Error(logger, package, assetReference, code, relatedGuids, (Exception)null, arguments);
        }

        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IContentReference[] relatedGuids, Exception exception = null)
        {
            Error(logger, package, assetReference, code, (IEnumerable<IContentReference>)relatedGuids, exception);
        }

        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IEnumerable<IContentReference> relatedGuids, Exception exception = null)
        {
            var logMessage = new AssetLogMessage(package, assetReference, LogMessageType.Error, code) { Exception = exception };
            if (relatedGuids != null)
            {
                logMessage.Related.AddRange(relatedGuids);
            }
            logger.Log(logMessage);
        }

        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, Exception exception, params object[] arguments)
        {
            Error(logger, package, assetReference, code, null, exception, arguments);
        }

        public static void Error(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IEnumerable<IContentReference> relatedGuids, Exception exception, params object[] arguments)
        {
            var logMessage = new AssetLogMessage(package, assetReference, LogMessageType.Error, code, arguments) { Exception = exception };
            if (relatedGuids != null)
            {
                logMessage.Related.AddRange(relatedGuids);
            }
            logger.Log(logMessage);
        }

        public static void Warning(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IContentReference[] relatedGuids)
        {
            Warning(logger, package, assetReference, code, (IEnumerable<IContentReference>)null);
        }

        public static void Warning(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IEnumerable<IContentReference> relatedGuids)
        {
            var logMessage = new AssetLogMessage(package, assetReference, LogMessageType.Warning, code);
            if (relatedGuids != null)
            {
                logMessage.Related.AddRange(relatedGuids);
            }
            logger.Log(logMessage);
        }

        public static void Warning(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, params object[] arguments)
        {
            Warning(logger, package, assetReference, code, null, arguments);
        }

        public static void Warning(this ILogger logger, Package package, IContentReference assetReference, AssetMessageCode code, IEnumerable<IContentReference> relatedGuids, params object[] arguments)
        {
            var logMessage = new AssetLogMessage(package, assetReference, LogMessageType.Warning, code, arguments);
            if (relatedGuids != null)
            {
                logMessage.Related.AddRange(relatedGuids);
            }
            logger.Log(logMessage);
        }
    }
}