// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Result returned when loading a session using <see cref="PackageSession.Load(string,PackageSessionResult,System.Nullable{System.Threading.CancellationToken},bool)"/>
    /// </summary>
    public sealed class PackageSessionResult : LoggerResult
    {
        /// <summary>
        /// Gets or sets the loaded session.
        /// </summary>
        /// <value>The session.</value>
        public PackageSession Session { get; internal set; }

        /// <summary>
        /// Gets or sets whether the operation has been cancelled by user.
        /// </summary>
        public bool OperationCancelled { get; set; }

        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();
            Session = null;
        }
    }
}
