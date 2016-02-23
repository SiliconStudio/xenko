// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The context used when compiling an asset in a Package.
    /// </summary>
    public class AssetCompilerContext : CompilerContext
    {
        public AssetCompilerContext()
        {
        }

        /// <summary>
        /// Gets or sets the name of the profile being built.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets the build configuration (Debug, Release, AppStore, Testing)
        /// </summary>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the target platform for compiler is being used for.
        /// </summary>
        /// <value>The platform.</value>
        public PlatformType Platform { get; set; }
    }
}