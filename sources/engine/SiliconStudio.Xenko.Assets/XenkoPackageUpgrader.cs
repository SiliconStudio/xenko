// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Assets.Effect;

namespace SiliconStudio.Xenko.Assets
{
    [PackageUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "1.9.4-beta")]
    public class XenkoPackageUpgrader : PackageUpgrader
    {
        public override bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
        {
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
                            var modelReference = DynamicYamlExtensions.ConvertTo<AssetReference>(model);
                            var modelAsset = modelAssetsWithSekeleton.FirstOrDefault(x => x.Asset.AssetLocation == modelReference.Location);

                            foreach (var animation in animationComponent.Animations)
                            {
                                var animationReference = DynamicYamlExtensions.ConvertTo<AssetReference>(animation.Value);
                                var animationAsset = animAssets.FirstOrDefault(x => x.Asset.AssetLocation == animationReference.Location);

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
                        x => UPath.Combine(animationAsset.Asset.AssetLocation.GetParent(), new UFile((string)animationAsset.DynamicRootNode.Source))
                             == UPath.Combine(x.Asset.AssetLocation.GetParent(), new UFile((string)x.DynamicRootNode.Source)));
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
                        modelAsset.DynamicRootNode.Skeleton = new AssetReference(AssetId.Parse((string)skeletonAssetYaml.DynamicRootNode.Id), skeletonAsset.AssetLocation.MakeRelative(modelAsset.Asset.AssetLocation.GetParent()));
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
                    animationAsset.DynamicRootNode.Skeleton = new AssetReference(AssetId.Parse((string)skeletonAsset.DynamicRootNode.Id), skeletonAsset.Asset.AssetLocation.MakeRelative(animationAsset.Asset.AssetLocation.GetParent()));
                    animationAsset.DynamicRootNode.PreviewModel = new AssetReference(AssetId.Parse((string)modelAsset.DynamicRootNode.Id), modelAsset.Asset.AssetLocation.MakeRelative(animationAsset.Asset.AssetLocation.GetParent()));
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

            if (dependency.Version.MinVersion < new PackageVersion("1.6.0-beta"))
            {
                // Delete EffectLogAsset
                foreach (var assetFile in assetFiles)
                {
                    if (assetFile.FilePath.GetFileNameWithoutExtension() == EffectLogAsset.DefaultFile)
                    {
                        assetFile.Deleted = true;
                    }
                }
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.7.0-alpha02"))
            {
                foreach (var assetFile in assetFiles)
                {
                    if (!IsYamlAsset(assetFile))
                        continue;

                    using (var assetYaml = assetFile.AsYamlAsset())
                    {
                        if (assetYaml == null)
                            continue;

                        var sourceNode = assetYaml.DynamicRootNode.Source;
                        var sourceHashNode = assetYaml.DynamicRootNode.SourceHash;
                        if (sourceHashNode != null)
                        {
                            var source = DynamicYamlExtensions.ConvertTo<UFile>(sourceNode);
                            var sourceHash = DynamicYamlExtensions.ConvertTo<ObjectId>(sourceHashNode);
                            var dictionary = new Dictionary<UFile, ObjectId> { { source, sourceHash } };
                            var yamlDic = DynamicYamlExtensions.ConvertFrom(dictionary);
                            yamlDic.Node.Tag = null;
                            assetYaml.DynamicRootNode["~SourceHashes"] = yamlDic;
                            assetYaml.DynamicRootNode.SourceHash = DynamicYamlEmpty.Default;
                        }
                        assetYaml.DynamicRootNode.ImporterId = DynamicYamlEmpty.Default;
                        assetYaml.DynamicRootNode.SourceKeepSideBySide = DynamicYamlEmpty.Default;

                        var assetBase = assetYaml.DynamicRootNode["~Base"];
                        if (assetBase != null)
                        {
                            if (assetBase.Location == "--import--")
                                assetYaml.DynamicRootNode["~Base"] = DynamicYamlEmpty.Default;
                        }
                    }
                }
            }

            //Audio refactor
            if (dependency.Version.MinVersion < new PackageVersion("1.7.0-alpha03"))
            {
                var audioAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xksnd").Select(x => x.AsYamlAsset()).ToArray();
                foreach (var assetFile in audioAssets)
                {
                    //dispose will save back
                    using (var assetYaml = assetFile)
                    {
                        if (assetYaml == null)
                            continue;

                        if (assetYaml.RootNode.Tag == "!SoundMusic")
                        {
                            assetYaml.RootNode.Tag = "!Sound";
                            assetYaml.DynamicRootNode.Spatialized = false;
                            assetYaml.DynamicRootNode.StreamFromDisk = true;  
                        }
                        else
                        {
                            assetYaml.RootNode.Tag = "!Sound";
                            assetYaml.DynamicRootNode.Spatialized = true;
                            assetYaml.DynamicRootNode.StreamFromDisk = false;
                        }
                    }
                }
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.7.0-alpha03"))
            {
                // Delete EffectLogAsset (now, most of it is auto generated automatically by drawing one frame of the game)
                foreach (var assetFile in assetFiles)
                {
                    if (assetFile.FilePath.GetFileNameWithoutExtension() == EffectLogAsset.DefaultFile)
                    {
                        assetFile.Deleted = true;
                    }
                }
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.8.4-beta"))
            {
                // Add new generic parameter of MaterialSurfaceNormalMap to effect logs
                var regex = new Regex(@"(?<=ClassName:\s+MaterialSurfaceNormalMap\s+GenericArguments:\s+\[[^\]]*)(?=\])");
                foreach (var assetFile in assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkeffectlog"))
                {
                    var filePath = assetFile.FilePath;

                    // Load asset data, so the renamed file will have it's AssetContent set
                    if (assetFile.AssetContent == null)
                        assetFile.AssetContent = File.ReadAllBytes(filePath);

                    var sourceText = System.Text.Encoding.UTF8.GetString(assetFile.AssetContent);
                    var newSourceText = regex.Replace(sourceText, ", true");
                    var newAssetContent = System.Text.Encoding.UTF8.GetBytes(newSourceText);

                    if (newSourceText != sourceText)
                    {
                        assetFile.AssetContent = newAssetContent;
                    }

                    //File.WriteAllBytes(newFileName, newAssetContent);
                }
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.9.0-beta"))
            {
                foreach (var assetFile in assetFiles)
                {
                    if (!IsYamlAsset(assetFile))
                        continue;

                    // This upgrader will also mark every yaml asset as dirty. We want to re-save everything with the new serialization system
                    using (var assetYaml = assetFile.AsYamlAsset())
                    {
                        if (assetYaml == null)
                            continue;

                        try
                        {
                            if (assetYaml.DynamicRootNode["~Base"] != null)
                            {
                                var location = ((YamlScalarNode)assetYaml.DynamicRootNode["~Base"].Location.Node).Value;
                                if (location != "--import--")
                                {
                                    var id = ((YamlScalarNode)assetYaml.DynamicRootNode["~Base"].Asset.Id.Node).Value;
                                    var assetUrl = $"{id}:{location}";
                                    assetYaml.DynamicRootNode["Archetype"] = assetUrl;
                                }
                                assetYaml.DynamicRootNode["~Base"] = DynamicYamlEmpty.Default;
                            }
                        }
                        catch
                            (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }

            return true;
        }

        private void RunAssetUpgradersUntilVersion(ILogger log, Package dependentPackage, string dependencyName, IList<PackageLoadingAssetFile> assetFiles, PackageVersion maxVersion)
        {
            foreach (var assetFile in assetFiles)
            {
                if (assetFile.Deleted)
                    continue;

                var context = new AssetMigrationContext(dependentPackage, assetFile.ToReference(), assetFile.FilePath.ToWindowsPath(), log);
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
                        dependentPackage.RootAssets.Add(new AssetReference(assetItem.Id, assetItem.Location));
                }
            }

            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.6.0-beta"))
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

        public override bool UpgradeBeforeAssembliesLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage)
        {
            if (dependency.Version.MinVersion < new PackageVersion("1.4.0-alpha01"))
            {
                UpgradeCode(dependentPackage, log, new RenameToXenkoCodeUpgrader());
            }
            else if (dependency.Version.MinVersion < new PackageVersion("1.6.0-beta"))
            {
                UpgradeCode(dependentPackage, log, new NewComponentsCodeUpgrader());
            }

            return true;
        }

        private void UpgradeCode(Package dependentPackage, ILogger log, ICodeUpgrader codeUpgrader)
        {
            if (dependentPackage == null) throw new ArgumentNullException(nameof(dependentPackage));
            if (codeUpgrader == null) throw new ArgumentNullException(nameof(codeUpgrader));

            var csharpWorkspaceAssemblies = new[] { Assembly.Load("Microsoft.CodeAnalysis.Workspaces"), Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"), Assembly.Load("Microsoft.CodeAnalysis.Workspaces.Desktop") };
            var workspace = MSBuildWorkspace.Create(ImmutableDictionary<string, string>.Empty, MefHostServices.Create(csharpWorkspaceAssemblies));

            var tasks = dependentPackage.Profiles
                .SelectMany(profile => profile.ProjectReferences)
                .Select(projectReference => UPath.Combine(dependentPackage.RootDirectory, projectReference.Location))
                .Distinct()
                .Select(projectFullPath => Task.Run(async () =>
                {
                    if (codeUpgrader.UpgradeProject(workspace, projectFullPath))
                    {
                        // Upgrade source code
                        var f = new FileInfo(projectFullPath.ToWindowsPath());
                        if (f.Exists)
                        {
                            var project = await workspace.OpenProjectAsync(f.FullName);
                            var compilation = await project.GetCompilationAsync();
                            var subTasks = compilation.SyntaxTrees.Select(syntaxTree => Task.Run(() => codeUpgrader.UpgradeSourceFile(syntaxTree))).ToList();

                            await Task.WhenAll(subTasks);
                        }
                        else
                        {
                            log.Error($"Cannot locate project {f.FullName}.");
                        }
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private bool IsYamlAsset(PackageLoadingAssetFile assetFile)
        {
            // Determine if asset was Yaml or not
            var assetFileExtension = Path.GetExtension(assetFile.FilePath);
            assetFileExtension = assetFileExtension?.ToLowerInvariant();

            var serializer = AssetFileSerializer.FindSerializer(assetFileExtension);
            return serializer is YamlAssetSerializer;
        }
        /// <summary>
        /// Base interface for code upgrading
        /// </summary>
        private interface ICodeUpgrader
        {
            /// <summary>
            /// Upgrades the specified project file
            /// </summary>
            /// <param name="workspace">The msbuild workspace</param>
            /// <param name="projectPath">A path to a csproj file</param>
            /// <returns><c>true</c> if <see cref="UpgradeSourceFile"/> should be called for each files in the project; otherwise <c>false</c></returns>
            bool UpgradeProject(MSBuildWorkspace workspace, UFile projectPath);

            /// <summary>
            /// Upgrades the specified file 
            /// </summary>
            /// <param name="syntaxTree">The syntaxtree of the file</param>
            /// <returns>An upgrade task</returns>
            Task UpgradeSourceFile(SyntaxTree syntaxTree);
        }

        /// <summary>
        /// Code upgrader for renaming to Xenko
        /// </summary>
        private class RenameToXenkoCodeUpgrader : ICodeUpgrader
        {
            public bool UpgradeProject(MSBuildWorkspace workspace, UFile projectPath)
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
                return true;
            }

            // TODO: Reverted to simple regex, to upgrade text in .pdxfx's generated code files. Should use syntax analysis again.
            public async Task UpgradeSourceFile(SyntaxTree syntaxTree)
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
        }

        private class NewComponentsCodeUpgrader : ICodeUpgrader
        {
            private readonly Regex regexGetComponent;
            private readonly Regex regexInheritScript;

            public NewComponentsCodeUpgrader()
            {
                regexGetComponent = new Regex(@"\.Get\(([A-Za-z0-9_]*Component)\.Key\)");
                regexInheritScript = new Regex(@"class\s+(.*?):\s*Script(\W)");
            }

            public bool UpgradeProject(MSBuildWorkspace workspace, UFile projectPath)
            {
                return true;
            }

            public async Task UpgradeSourceFile(SyntaxTree syntaxTree)
            {
                var fileContents = File.ReadAllText(syntaxTree.FilePath);
                var newFileContents = fileContents;

                // Handle Scripts
                newFileContents = newFileContents.Replace("Get(ScriptComponent.Key).Scripts", "GetAll<ScriptComponent>()");
                newFileContents = newFileContents.Replace("Get<ScriptComponent>().Scripts", "GetAll<ScriptComponent>()");
                newFileContents = regexGetComponent.Replace(newFileContents, @".Get<$1>()");
                newFileContents = regexInheritScript.Replace(newFileContents, "class $1 : ScriptComponent$2");

                // Handle Physics
                newFileContents = newFileContents.Replace("Get(PhysicsComponent.Key).Elements", "GetAll<PhysicsComponent>()");
                newFileContents = newFileContents.Replace("Get<PhysicsComponent>().Elements", "GetAll<ScriptComponent>()");
                newFileContents = newFileContents.Replace("Get<PhysicsComponent>()[0]", "Get<PhysicsComponent>()");

                // Save file if there were any changes
                if (newFileContents != fileContents)
                {
                    File.WriteAllText(syntaxTree.FilePath, newFileContents);
                }
            }
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
