// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Base class for all <see cref="Session"/> and <see cref="Asset"/> integrity analysis.
    /// </summary>
    [AssemblyScan]
    public abstract class PackageSessionAnalysisBase
    {
        private PackageSession packageSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSessionAnalysis" /> class.
        /// </summary>
        /// <param name="packageSession">The package session.</param>
        /// <exception cref="System.ArgumentNullException">packageSession</exception>
        protected PackageSessionAnalysisBase(PackageSession packageSession)
        {
            if (packageSession == null) throw new ArgumentNullException("packageSession");
            this.packageSession = packageSession;
        }

        protected PackageSessionAnalysisBase()
        {
            
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        public PackageSession Session
        {
            get
            {
                return packageSession;
            }
            set
            {
                packageSession = value;
            }
        }

        /// <summary>
        /// Performs a wide package validation analysis.
        /// </summary>
        /// <returns>Result of the validation.</returns>
        public LoggerResult Run()
        {
            if (packageSession == null) throw new InvalidOperationException("packageSession is null");
            var results = new LoggerResult();
            Run(results);
            return results;
        }

        /// <summary>
        /// Performs a wide package validation analysis.
        /// </summary>
        /// <param name="log">The log to output the result of the validation.</param>
        public abstract void Run(ILogger log);
    }
}