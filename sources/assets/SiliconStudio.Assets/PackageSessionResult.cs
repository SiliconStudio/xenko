// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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

        public override void Clear()
        {
            base.Clear();
            Session = null;
        }
    }
}