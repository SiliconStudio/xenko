// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The context used when compiling an asset in a Package.
    /// </summary>
    public class AssetCompilerContext : CompilerContext
    {
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

        /// <summary>
        /// The compilation context type of this compiler context
        /// </summary>
        public Type CompilationContext;
    }
}
