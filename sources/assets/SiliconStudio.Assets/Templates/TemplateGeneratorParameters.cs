// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// Parameters used by <see cref="ITemplateGenerator.PrepareForRun"/>
    /// </summary>
    public sealed class TemplateGeneratorParameters
    {
        /// <summary>
        /// Gets or sets the project name used to generate the template.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default namespace of this project.
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public UDirectory OutputDirectory { get; set; }

        /// <summary>
        /// The actual template description.
        /// </summary>
        public TemplateDescription Description { get; set; }

        /// <summary>
        /// Gets or sets the window handle.
        /// </summary>
        /// <value>The window handle.</value>
        public IntPtr WindowHandle { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the current session.
        /// </summary>
        /// <value>The session.</value>
        public PackageSession Session { get; set; }

        /// <summary>
        /// Gets or sets the current package (may be null)
        /// </summary>
        /// <value>The package.</value>
        public Package Package { get; set; }

        /// <summary>
        /// Contains extra properties that can be consumed by template generators.
        /// </summary>
        public PropertyContainer Tags;

        /// <summary>
        /// Validates this instance (all fields must be setup)
        /// </summary>
        public void Validate()
        {
            if (Name == null)
            {
                throw new InvalidOperationException("[Name] cannot be null in TemplateGeneratorParameters");
            }
            if (OutputDirectory == null && Description.Scope != TemplateScope.Package)
            {
                throw new InvalidOperationException("[OutputDirectory] cannot be null in TemplateGeneratorParameters for a template that is not generated within a Package");
            }
            if (Description == null)
            {
                throw new InvalidOperationException("[Description] cannot be null in TemplateGeneratorParameters");
            }
            if (Logger == null)
            {
                throw new InvalidOperationException("[Logger] cannot be null in TemplateGeneratorParameters");
            }

            if (Description.Scope == TemplateScope.Session && Session == null)
            {
                throw new InvalidOperationException("[Session] cannot be null in for a template expecting a PackageSession");
            }
            if (Description.Scope == TemplateScope.Package && Package == null)
            {
                throw new InvalidOperationException("[Package] cannot be null in for a template expecting a Package");
            }
        }
    }
}