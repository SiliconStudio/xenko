// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Settings;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The context used when compiling an asset in a Package.
    /// </summary>
    public class CompilerContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerContext"/> class.
        /// </summary>
        public CompilerContext()
        {
            Properties = new PropertyCollection();
            PackageProperties = PackageProfile.SettingsContainer.CreateSettingsProfile(false);
        }

        /// <summary>
        /// Properties passed on the command line.
        /// </summary>
        public Dictionary<string, string> OptionProperties { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the attributes attached to this context.
        /// </summary>
        /// <value>The attributes.</value>
        public PropertyCollection Properties { get; private set; }

        public SettingsProfile PackageProperties { get; private set; }

        public CompilerContext Clone()
        {
            var context = (CompilerContext)MemberwiseClone();
            return context;
        }

        public void Dispose()
        {
            PackageProfile.SettingsContainer.UnloadSettingsProfile(PackageProperties);
            PackageProperties.Dispose();
        }
    }
}