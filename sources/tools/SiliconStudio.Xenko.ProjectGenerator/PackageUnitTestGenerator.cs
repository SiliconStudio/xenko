// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets;

namespace SiliconStudio.Xenko.ProjectGenerator
{
    /// <summary>
    /// Create a package.
    /// </summary>
    public class PackageUnitTestGenerator : TemplateGeneratorBase<SessionTemplateGeneratorParameters>
    {
        public static readonly PackageUnitTestGenerator Default = new PackageUnitTestGenerator();

        public static readonly Guid TemplateId = new Guid("3c4ac35f-4d63-462e-9696-974ebaa9a862");

        public override bool IsSupportingTemplate(TemplateDescription templateDescription)
        {
            if (templateDescription == null) throw new ArgumentNullException(nameof(templateDescription));
            return templateDescription.Id == TemplateId;
        }

        public override Task<bool> PrepareForRun(SessionTemplateGeneratorParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            parameters.Validate();

            return Task.FromResult(true);
        }

        public sealed override bool Run(SessionTemplateGeneratorParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            parameters.Validate();

            var name = parameters.Name;
            var outputDirectory = parameters.OutputDirectory;

            // Creates the package
            var package = NewPackage(name);
            package.Id = parameters.Id;

            // Setup the default namespace
            package.Meta.RootNamespace = parameters.Namespace;

            // Setup the path to save it
            package.FullPath = UPath.Combine(outputDirectory, new UFile(name + Package.PackageFileExtension));

            // Add it to the current session
            var session = parameters.Session;
            session.Packages.Add(package);

            // Load missing references
            session.LoadMissingReferences(parameters.Logger);
            return true;
        }

        /// <summary>
        /// Creates a new Xenko package with the specified name
        /// </summary>
        /// <param name="name">Name of the package</param>
        /// <returns>A new package instance</returns>
        public static Package NewPackage(string name)
        {
            var package = new Package
            {
                Meta =
                {
                    Name = name,
                    Version = new PackageVersion("1.0.0.0")
                },
            };

            // Add dependency to latest Xenko package
            package.Meta.Dependencies.Add(XenkoConfig.GetLatestPackageDependency());

            // Setup the assets folder by default
            package.Profiles.Add(PackageProfile.NewShared());

            return package;
        }
    }
}
