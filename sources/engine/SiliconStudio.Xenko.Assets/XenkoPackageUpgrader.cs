// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
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
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Effect;

namespace SiliconStudio.Xenko.Assets
{
    [PackageUpgrader(XenkoConfig.PackageName, "1.0.0-beta01", "1.5.0-alpha03")]
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

            if (dependency.Version.MinVersion < new PackageVersion("1.5.0-alpha01"))
            {
                RunAssetUpgradersUntilVersion(log, dependentPackage, XenkoConfig.PackageName, assetFiles, PackageVersion.Parse("1.5.0-alpha01"));
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.5.0-alpha02"))
            {
                // Ideally, this should be part of asset upgrader but we can't upgrade multiple assets at once yet

                var modelAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkm3d").Select(x => x.AsYamlAsset()).ToArray();
                var animAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkanim").Select(x => x.AsYamlAsset()).ToArray();
                var sceneAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkscene").Select(x => x.AsYamlAsset()).ToArray();

                // Select models with at least two nodes
                var modelAssetsWithSekeleton = modelAssets
                    .Where(model => ((IEnumerable)model.DynamicRootNode.Nodes).Cast<object>().Count() > 1).ToArray();

                var animToModelMapping = new Dictionary<PackageLoadingAssetFile.YamlAsset, PackageLoadingAssetFile.YamlAsset>();

                // Find associations in scene
                foreach (var sceneAsset in sceneAssets)
                {
                    var hierarchy = sceneAsset.DynamicRootNode.Hierarchy;
                    foreach (dynamic entity in hierarchy.Entities)
                    {
                        var components = entity.Entity.Components;
                        var animationComponent = components["AnimationComponent.Key"];
                        var model = components["ModelComponent.Key"]?.Model;
                        if (animationComponent != null && model != null)
                        {
                            var modelReference = DynamicYamlExtensions.ConvertTo<AssetReference<Asset>>(model);
                            var modelAsset = modelAssetsWithSekeleton.FirstOrDefault(x => x.Asset.AssetPath == modelReference.Location);

                            foreach (var animation in animationComponent.Animations)
                            {
                                var animationReference = DynamicYamlExtensions.ConvertTo<AssetReference<Asset>>(animation.Value);
                                var animationAsset = animAssets.FirstOrDefault(x => x.Asset.AssetPath == animationReference.Location);

                                if (modelAsset != null && animationAsset != null)
                                {
                                    animToModelMapping[animationAsset] = modelAsset;
                                }
                            }
                        }
                    }
                }

                // Find associations when sharing same source file
                foreach (var animationAsset in animAssets)
                {
                    // Comparing absolute path of assets
                    var modelAsset = modelAssetsWithSekeleton.FirstOrDefault(
                        x => UPath.Combine(animationAsset.Asset.AssetPath.GetParent(), new UFile((string)animationAsset.DynamicRootNode.Source))
                             == UPath.Combine(x.Asset.AssetPath.GetParent(), new UFile((string)x.DynamicRootNode.Source)));
                    if (modelAsset != null)
                    {
                        animToModelMapping[animationAsset] = modelAsset;
                    }
                }

                var modelToSkeletonMapping = new Dictionary<PackageLoadingAssetFile.YamlAsset, PackageLoadingAssetFile.YamlAsset>();

                // For each model asset, create skeleton assets
                foreach (var modelAsset in modelAssetsWithSekeleton)
                {
                    var skeletonAsset = new PackageLoadingAssetFile(modelAsset.Asset.FilePath.GetFullPathWithoutExtension() + " Skeleton.xkskel", modelAsset.Asset.SourceFolder)
                    {
                        AssetContent = System.Text.Encoding.UTF8.GetBytes("!Skeleton\r\nId: " + Guid.NewGuid())
                    };

                    using (var skeletonAssetYaml = skeletonAsset.AsYamlAsset())
                    {
                        // Set source
                        skeletonAssetYaml.DynamicRootNode.Source = modelAsset.DynamicRootNode.Source;
                        skeletonAssetYaml.DynamicRootNode.SourceHash = modelAsset.DynamicRootNode.SourceHash;

                        // To be on the safe side, mark everything as preserved
                        var nodes = modelAsset.DynamicRootNode.Nodes;
                        foreach (var node in nodes)
                        {
                            node.Preserve = true;
                        }

                        skeletonAssetYaml.DynamicRootNode.Nodes = nodes;
                        skeletonAssetYaml.DynamicRootNode.ScaleImport = modelAsset.DynamicRootNode.ScaleImport;

                        // Update model to point to this skeleton
                        modelAsset.DynamicRootNode.Skeleton = new AssetReference<Asset>(Guid.Parse((string)skeletonAssetYaml.DynamicRootNode.Id), skeletonAsset.AssetPath.MakeRelative(modelAsset.Asset.AssetPath.GetParent()));
                        modelToSkeletonMapping.Add(modelAsset, skeletonAssetYaml);
                    }

                    assetFiles.Add(skeletonAsset);
                }

                // Update animation to point to skeleton, and set preview default model
                foreach (var animToModelEntry in animToModelMapping)
                {
                    var animationAsset = animToModelEntry.Key;
                    var modelAsset = animToModelEntry.Value;

                    var skeletonAsset = modelToSkeletonMapping[modelAsset];
                    animationAsset.DynamicRootNode.Skeleton = new AssetReference<Asset>(Guid.Parse((string)skeletonAsset.DynamicRootNode.Id), skeletonAsset.Asset.AssetPath.MakeRelative(animationAsset.Asset.AssetPath.GetParent()));
                    animationAsset.DynamicRootNode.PreviewModel = new AssetReference<Asset>(Guid.Parse((string)modelAsset.DynamicRootNode.Id), modelAsset.Asset.AssetPath.MakeRelative(animationAsset.Asset.AssetPath.GetParent()));
                }

                // Remove Nodes from models
                foreach (var modelAsset in modelAssets)
                {
                    modelAsset.DynamicRootNode.Nodes = DynamicYamlEmpty.Default;
                    modelAsset.DynamicRootNode["~Base"].Asset.Nodes = DynamicYamlEmpty.Default;
                }

                // Save back
                foreach (var modelAsset in modelAssets)
                    modelAsset.Dispose();
                foreach (var animAsset in animAssets)
                    animAsset.Dispose();
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

            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.5.0-alpha03"))
            {
                // Mark all assets dirty to force a resave
                foreach (var assetItem in dependentPackage.Assets)
                {
                    if (!(assetItem.Asset is SourceCodeAsset))
                    {
                        assetItem.IsDirty = true;
                    }
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