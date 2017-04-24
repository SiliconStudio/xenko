// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets
{
    class AssetLogger : Logger
    {
        private readonly Package package;
        private readonly IReference assetReference;
        private readonly string assetFullPath;
        private readonly ILogger loggerToForward;

        public AssetLogger(Package package, IReference assetReference, string assetFullPath, ILogger loggerToForward)
        {
            this.package = package;
            this.assetReference = assetReference;
            this.assetFullPath = assetFullPath;
            this.loggerToForward = loggerToForward;
            ActivateLog(LogMessageType.Debug);
        }

        protected override void LogRaw(ILogMessage logMessage)
        {
            loggerToForward?.Log(AssetLogMessage.From(package, assetReference, logMessage, assetFullPath));
        }
    }
}
