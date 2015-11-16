// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Effect;

namespace SiliconStudio.Xenko.Assets
{
    [PackageUpgrader(XenkoConfig.PackageName, "1.0.0-beta01", "1.4.0-beta")]
    public class XenkoPackageUpgrader : PackageUpgrader
    {
        public override bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
        {
            // Paradox 1.1 projects didn't have their dependency properly updated (they might have been marked as 1.0).
            // We know they are 1.1 only because there is a .props file.
            // This check shouldn't be necessary from 1.2.
            var packagePath = dependentPackage.FullPath;
            var propsFilePath = UPath.Combine(packagePath.GetParent(), (UFile)(packagePath.GetFileName() + ".props"));
            if (!File.Exists(propsFilePath) && dependency.Version.MinVersion < new PackageVersion("1.1.0-beta"))
            {
                log.Error("Can't upgrade old projects from {0} 1.0 to 1.1", dependency.Name);
                return false;
            }

            // Nothing to do for now, most of the work is already done by individual asset upgraders
            // We can later add logic here for package-wide upgrades (i.e. GameSettingsAsset)
            if (dependency.Version.MinVersion < new PackageVersion("1.2.0-beta"))
            {
                // UIImageGroups and SpriteGroups asset have been merged into a single SpriteSheet => rename the assets and modify the tag
                var uiImageGroups = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".pdxuiimage");
                var spritesGroups = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".pdxsprite");
                RenameAndChangeTag(assetFiles, uiImageGroups, "!UIImageGroup");
                RenameAndChangeTag(assetFiles, spritesGroups, "!SpriteGroup");
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.3.0-alpha01"))
            {
                // Create GameSettingsAsset
                GameSettingsAsset.UpgraderVersion130.Upgrade(session, log, dependentPackage, dependency, dependencyPackage, assetFiles);
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.3.0-alpha02"))
            {
                // Delete EffectLogAsset
                foreach (var assetFile in assetFiles)
                {
                    if (assetFile.FilePath.GetFileName() == EffectLogAsset.DefaultFile)
                    {
                        assetFile.Deleted = true;
                    }
                }
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.4.0-beta"))
            {
                // Update file extensions with Xenko prefix
                var legacyAssets = from assetFile in assetFiles
                                   where !assetFile.Deleted
                                   let extension = assetFile.FilePath.GetFileExtension()
                                   where extension.StartsWith(".pdx")
                                   select new { AssetFile = assetFile, NewExtension = ".xk" + extension.Substring(4) };

                foreach (var legacyAsset in legacyAssets.ToArray())
                {
                    // Load asset data, so the renamed file will have it's AssetContent set
                    if (legacyAsset.AssetFile.AssetContent == null)
                        legacyAsset.AssetFile.AssetContent = File.ReadAllBytes(legacyAsset.AssetFile.FilePath);

                    // Change legacy namespaces and default effect names in all shader source files
                    // TODO: Use syntax analysis? What about shaders referenced in other assets?
                    if (legacyAsset.NewExtension == ".xksl" || legacyAsset.NewExtension == ".xkfx" || legacyAsset.NewExtension == ".xkeffectlog")
                    {
                        var sourceText = System.Text.Encoding.UTF8.GetString(legacyAsset.AssetFile.AssetContent);
                        var newSourceText = sourceText.Replace("Paradox", "Xenko");

                        if (newSourceText != sourceText)
                        {
                            legacyAsset.AssetFile.AssetContent = System.Text.Encoding.UTF8.GetBytes(newSourceText);
                        }
                    }

                    // Create asset copy with new extension
                    ChangeFileExtension(assetFiles, legacyAsset.AssetFile, legacyAsset.NewExtension);
                }

                // Force loading of user settings with old extension
                var userSettings = dependentPackage.UserSettings;

                // Change package extension
                dependentPackage.FullPath = new UFile(dependentPackage.FullPath.GetFullPathWithoutExtension(), Package.PackageFileExtension);

                // Make sure all assets are upgraded
                RunAssetUpgradersUntilVersion(log, dependentPackage, XenkoConfig.PackageName, assetFiles, PackageVersion.Parse("1.4.0-beta"));
            }

            return true;
        }

        private void RunAssetUpgradersUntilVersion(ILogger log, Package dependentPackage, string dependencyName, IList<PackageLoadingAssetFile> assetFiles, PackageVersion maxVersion)
        {
            foreach (var assetFile in assetFiles)
            {
                if (assetFile.Deleted)
                    continue;

                var context = new AssetMigrationContext(dependentPackage, log);
                AssetMigration.MigrateAssetIfNeeded(context, assetFile, dependencyName, maxVersion);
            }
        }

        /// <inheritdoc/>
        public override bool UpgradeAfterAssetsLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, PackageVersionRange dependencyVersionBeforeUpdate)
        {
            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.3.0-alpha02"))
            {
                // Add everything as root assets (since we don't know what the project was doing in the code before)
                foreach (var assetItem in dependentPackage.Assets)
                {
                    if (!AssetRegistry.IsAssetTypeAlwaysMarkAsRoot(assetItem.Asset.GetType()))
                        dependentPackage.RootAssets.Add(new AssetReference<Asset>(assetItem.Id, assetItem.Location));
                }
            }

            return true;
        }

        private void ChangeFileExtension(IList<PackageLoadingAssetFile> assetFiles, PackageLoadingAssetFile file, string newExtension)
        {
            // Create the new file
            var newFileName = new UFile(file.FilePath.FullPath.Replace(file.FilePath.GetFileExtension(), newExtension));
            var newFile = new PackageLoadingAssetFile(newFileName, file.SourceFolder) { AssetContent = file.AssetContent };

            // Add the new file
            assetFiles.Add(newFile);

            // Mark the old file as "To Delete"
            file.Deleted = true;
        }

        private void RenameAndChangeTag( IList<PackageLoadingAssetFile> assetFiles, IEnumerable<PackageLoadingAssetFile> groupFiles, string oldTag)
        {
            var oldTagLength = System.Text.Encoding.UTF8.GetBytes(oldTag).Length;
            var newTagBuffer = System.Text.Encoding.UTF8.GetBytes("!SpriteSheet");
            
            foreach (var file in groupFiles.ToArray())
            {
                // set the content of the new asset (replace the tags)
                using (var stream = new FileStream(file.FilePath.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    file.AssetContent = new byte[stream.Length + newTagBuffer.Length - oldTagLength];
                    using (var memoryStream = new MemoryStream(file.AssetContent))
                    {
                        memoryStream.Write(newTagBuffer, 0, newTagBuffer.Length);
                        stream.Position = oldTagLength;
                        stream.CopyTo(memoryStream);
                    }
                }

                // rename the file
                ChangeFileExtension(assetFiles, file, ".pdxsheet");
            }
        }

        public override bool UpgradeBeforeAssembliesLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage)
        {
            if (dependency.Version.MinVersion < new PackageVersion("1.4.0-alpha01"))
            {
                // Only load workspace for C# assemblies (default includes VB but not added as a NuGet package)
                var csharpWorkspaceAssemblies = new[] { Assembly.Load("Microsoft.CodeAnalysis.Workspaces"), Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"), Assembly.Load("Microsoft.CodeAnalysis.Workspaces.Desktop") };
                var workspace = MSBuildWorkspace.Create(ImmutableDictionary<string, string>.Empty, MefHostServices.Create(csharpWorkspaceAssemblies));

                var tasks = dependentPackage.Profiles
                    .SelectMany(profile => profile.ProjectReferences)
                    .Select(projectReference => UPath.Combine(dependentPackage.RootDirectory, projectReference.Location))
                    .Distinct()
                    .Select(projectFullPath => Task.Run(() => UpgradeProject(workspace, projectFullPath)))
                    .ToArray();

                Task.WaitAll(tasks);
            }

            return true;
        }

        private async Task UpgradeProject(MSBuildWorkspace workspace, UFile projectPath)
        {
            // Upgrade .csproj file
            // TODO: Use parsed file?
            var fileContents = File.ReadAllText(projectPath);

            // Rename referenced to the package, shaders and effects
            var newFileContents = fileContents.Replace(".pdx", ".xk");

            // Rename variables
            newFileContents = newFileContents.Replace("Paradox", "Xenko");

            // Create fallback for old environment variable
            var index = newFileContents.IndexOf("<SiliconStudioCurrentPackagePath>", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                newFileContents = newFileContents.Insert(index, "<SiliconStudioXenkoDir Condition=\"'$(SiliconStudioXenkoDir)' == ''\">$(SiliconStudioParadoxDir)</SiliconStudioXenkoDir>\n    ");
            }

            // Save file if there were any changes
            if (newFileContents != fileContents)
            {
                File.WriteAllText(projectPath, newFileContents);
            }

            // Upgrade source code
            var project = await workspace.OpenProjectAsync(projectPath.ToWindowsPath());
            var compilation = await project.GetCompilationAsync();
            var tasks = compilation.SyntaxTrees.Select(syntaxTree => Task.Run(() => UpgradeSourceFile(syntaxTree))).ToList();

            await Task.WhenAll(tasks);
        }

        // TODO: Reverted to simple regex, to upgrade text in .pdxfx's generated code files. Should use syntax analysis again.
        private async Task UpgradeSourceFile(SyntaxTree syntaxTree)
        {
            var fileContents = File.ReadAllText(syntaxTree.FilePath);

            // Rename referenced to the package, shaders and effects
            var newFileContents = fileContents.Replace(".pdx", ".xk");

            // Rename variables
            newFileContents = newFileContents.Replace("Paradox", "Xenko");

            // Save file if there were any changes
            if (newFileContents != fileContents)
            {
                File.WriteAllText(syntaxTree.FilePath, newFileContents);
            }

            //var root = await syntaxTree.GetRootAsync();
            //var rewriter = new RenamingRewriter();
            //var newRoot = rewriter.Visit(root);

            //if (newRoot != root)
            //{
            //    var newSyntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
            //    var sourceText = await newSyntaxTree.GetTextAsync();

            //    using (var textWriter = new StreamWriter(syntaxTree.FilePath))
            //    {
            //        sourceText.Write(textWriter);
            //    }
            //}
        }

        private class RenamingRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                var identifier = node.Identifier;

                if (node.Identifier.ValueText.Contains("Paradox"))
                {
                    var newName = node.Identifier.ValueText.Replace("Paradox", "Xenko");
                    return node.WithIdentifier(SyntaxFactory.Identifier(identifier.LeadingTrivia, newName, identifier.TrailingTrivia));
                }

                return base.VisitIdentifierName(node);
            }
        }
    }
}