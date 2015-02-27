// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public sealed class PackageArchive
    {
        public static void Build(Package package, string outputDirectory = null)
        {
            if (package == null) throw new ArgumentNullException("package");

            var meta = new NuGet.ManifestMetadata();
            package.Meta.ToNugetManifest(meta);

            var builder = new NuGet.PackageBuilder();
            builder.Populate(meta);

            // TODO this is not working 
            var files = new List<NuGet.ManifestFile>()
                {
                    NewFile(@"Bin\**\*.exe", "Bin"),
                    NewFile(@"Bin\**\*.vsix", "Bin"),
                    NewFile(@"Bin\**\*.so", "Bin"),
                    NewFile(@"Bin\**\*.a", "Bin"),
                    NewFile(@"Bin\**\*.md", "Bin"),
                    NewFile(@"Bin\**\*.html", "Bin"),
                    NewFile(@"Bin\**\*.config", "Bin"),
                    NewFile(@"Bin\**\*.dll", "Bin"),
                    NewFile(@"Bin\**\*.xml", "Bin"),
                    NewFile(@"Bin\**\*.winmd", "Bin"),
                    NewFile(@"Targets\*.targets", "Targets"),
                };

            // Handle Assets
            var rootDir = package.RootDirectory;

            var newPackage = new Package { Meta = package.Meta };

            foreach (var profile in package.Profiles)
            {
                var target = "Assets/" + profile.Name;
                foreach (var assetFolder in profile.AssetFolders)
                {
                    // TODO: handle exclude in asset folders
                    //files.Add(NewFile(source, target, @"**\*.cs;**\*.hlsl;**\*.csproj;**\*.csproj.user;**\obj\**"));
                    files.Add(NewFile(assetFolder.Path.MakeRelative(rootDir) + "/**/*.pdxsl", target));
                    files.Add(NewFile(assetFolder.Path.MakeRelative(rootDir) + "/**/*.pdxfx", target));
                }

                var targetProfile = new PackageProfile(profile.Name);
                targetProfile.AssetFolders.Add(new AssetFolder(target));
                newPackage.Profiles.Add(targetProfile);
            }

            // Handle templates
            var targetFolder = new TemplateFolder("Templates");

            foreach (var templateFolder in package.TemplateFolders)
            {
                var source = templateFolder.Path.MakeRelative(rootDir) + "/**";
                UDirectory target = targetFolder.Path;
                if (templateFolder.Group != null)
                {
                    target = UPath.Combine(target, templateFolder.Group);
                }

                var excludeFiles = templateFolder.Exclude;
                files.Add(NewFile(source, target, excludeFiles));

                // Add template files
                foreach (var templateFile in templateFolder.Files)
                {
                    var newTemplateFile = templateFile.MakeRelative(templateFolder.Path);
                    if (templateFolder.Group != null)
                    {
                        newTemplateFile = UPath.Combine(templateFolder.Group, newTemplateFile);
                    }

                    newTemplateFile = UPath.Combine(targetFolder.Path, newTemplateFile);
                    targetFolder.Files.Add(newTemplateFile);
                }
            }

            // Create temp package for archive
            newPackage.TemplateFolders.Add(targetFolder);
            var newPackageFileName = "temp" + Guid.NewGuid() + ".pdxpkg";
            newPackage.FullPath = package.RootDirectory + "/" + newPackageFileName;
            var result = newPackage.Save();
            if (result.HasErrors)
            {
                throw new InvalidOperationException(result.ToText());
                // TODO throw error
            }
            files.Add(NewFile(newPackageFileName, package.Meta.Name + Package.PackageFileExtension));

            // Add files
            builder.PopulateFiles(package.RootDirectory, files);

            outputDirectory = outputDirectory ?? Environment.CurrentDirectory;

            // Save the nupkg
            var outputPath = GetOutputPath(builder,  outputDirectory);
            bool isExistingPackage = File.Exists(outputPath);
            if (isExistingPackage)
            {
                File.Delete(outputPath);
            }
            try
            {
                using (Stream stream = File.Create(outputPath))
                {
                    builder.Save(stream);
                }
            }
            catch
            {
                if (!isExistingPackage && File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                throw;
            }

            File.Delete(newPackage.FullPath);
        }

        private static NuGet.ManifestFile NewFile(string source, string target, string exclude = null)
        {
            return new NuGet.ManifestFile()
                {
                    Source = source.Replace('/', '\\'),
                    Target = target.Replace('/', '\\'),
                    Exclude = exclude
                };
        }

        private static string GetOutputPath(NuGet.PackageBuilder builder, string outputDirectory)
        {
            string version = builder.Version.ToString();

            // Output file is {id}.{version}
            string outputFile = builder.Id + "." + version;
            outputFile += NuGet.Constants.PackageExtension;

            return Path.Combine(outputDirectory, outputFile);
        }
    }
}