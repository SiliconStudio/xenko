// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// A package analysis provides methods to validate the integrity of a whole package.
    /// </summary>
    public sealed class PackageSessionAnalysis : PackageSessionAnalysisBase
    {
        private readonly PackageAnalysisParameters parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSessionAnalysis" /> class.
        /// </summary>
        /// <param name="packageSession">The package session.</param>
        /// <param name="parameters">The parameters.</param>
        public PackageSessionAnalysis(PackageSession packageSession, PackageAnalysisParameters parameters)
            : base(packageSession)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            this.parameters = (PackageAnalysisParameters)parameters.Clone();
            this.parameters.IsPackageCheckDependencies = true;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public PackageAnalysisParameters Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Performs a wide package validation analysis.
        /// </summary>
        /// <param name="log">The log to output the result of the validation.</param>
        public override void Run(ILogger log)
        {
            if (log == null) throw new ArgumentNullException("log");

            foreach (var package in Session.LocalPackages)
            {
                var analysis = new PackageAnalysis(package, parameters);
                analysis.Run(log);
            }
        }
   }
}
