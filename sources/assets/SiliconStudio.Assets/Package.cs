// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Evaluation;
using SharpYaml;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    public enum PackageState
    {
        /// <summary>
        /// Package has been deserialized. References and assets are not ready.
        /// </summary>
        Raw,

        /// <summary>
        /// Dependencies have all been resolved and are also in <see cref="DependenciesReady"/> state.
        /// </summary>
        DependenciesReady,

        /// <summary>
        /// Package upgrade has been failed (either error or denied by user).
        /// Dependencies are ready, but not assets.
        /// Should be manually switched back to DependenciesReady to try upgrade again.
        /// </summary>
        UpgradeFailed,

        /// <summary>
        /// Assembly references and assets have all been loaded.
        /// </summary>
        AssetsReady,
    }

    /// <summary>
    /// A package managing assets.
    /// </summary>
    [DataContract("Package")]
    [AssetDescription(PackageFileExtensions)]
    [DebuggerDisplay("Id: {Id}, Name: {Meta.Name}, Version: {Meta.Version}, Assets [{Assets.Count}]")]
    [AssetFormatVersion("Assets", PackageFileVersion)]
    [AssetUpgrader("Assets", 0, 1, typeof(RemoveRawImports))]
    [AssetUpgrader("Assets", 1, 2, typeof(RenameSystemPackage))]
    public sealed partial class Package : Asset, IFileSynchronizable
    {
        private const int PackageFileVersion = 2;

        private readonly List<UFile> filesToDelete = new List<UFile>();

        private PackageSession session;

        private UFile packagePath;
        private UFile previousPackagePath;
        private bool isDirty;
        private readonly Lazy<PackageUserSettings> settings;

        /// <summary>
        /// Occurs when an asset dirty changed occurred.
        /// </summary>
        public event DirtyFlagChangedDelegate<Asset> AssetDirtyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class.
        /// </summary>
        public Package()
        {
            Assets = new PackageAssetCollection(this);
            Bundles = new BundleCollection(this);
            IsDirty = true;
            settings = new Lazy<PackageUserSettings>(() => new PackageUserSettings(this));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this package is a system package.
        /// </summary>
        /// <value><c>true</c> if this package is a system package; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool IsSystem { get; internal set; }

        /// <summary>
        /// Gets or sets the metadata associated with this package.
        /// </summary>
        /// <value>The meta.</value>
        [DataMember(10)]
        public PackageMeta Meta { get; set; } = new PackageMeta();

        /// <summary>
        /// Gets the local package dependencies used by this package (only valid for local references). Global dependencies
        /// are defined through the <see cref="Meta"/> property in <see cref="PackageMeta.Dependencies"/> 
        /// </summary>
        /// <value>The package local dependencies.</value>
        [DataMember(30)]
        public List<PackageReference> LocalDependencies { get; } = new List<PackageReference>();

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        [DataMember(50)]
        public PackageProfileCollection Profiles { get; } = new PackageProfileCollection();

        /// <summary>
        /// Gets or sets the list of folders that are explicitly created but contains no assets.
        /// </summary>
        [DataMember(70)]
        public List<UDirectory> ExplicitFolders { get; } = new List<UDirectory>();

        /// <summary>
        /// Gets the bundles defined for this package.
        /// </summary>
        /// <value>The bundles.</value>
        [DataMember(80)]
        public BundleCollection Bundles { get; private set; }

        /// <summary>
        /// Gets the template folders.
        /// </summary>
        /// <value>The template folders.</value>
        [DataMember(90)]
        public List<TemplateFolder> TemplateFolders { get; } = new List<TemplateFolder>();

        /// <summary>
        /// Asset references that needs to be compiled even if not directly or indirectly referenced (useful for explicit code references).
        /// </summary>
        [DataMember(100)]
        public RootAssetCollection RootAssets { get; private set; } = new RootAssetCollection();

        /// <summary>
        /// Gets the loaded templates from the <see cref="TemplateFolders"/>
        /// </summary>
        /// <value>The templates.</value>
        [DataMemberIgnore]
        public List<TemplateDescription> Templates { get; } = new List<TemplateDescription>();

        /// <summary>
        /// Gets the assets stored in this package.
        /// </summary>
        /// <value>The assets.</value>
        [DataMemberIgnore]
        public PackageAssetCollection Assets { get; }

        /// <summary>
        /// Gets the temporary assets list loaded from disk before they are going into <see cref="Assets"/>.
        /// </summary>
        /// <value>The temporary assets.</value>
        [DataMemberIgnore]
        public AssetItemCollection TemporaryAssets { get; } = new AssetItemCollection();

        /// <summary>
        /// Gets the path to the package file. May be null if the package was not loaded or saved.
        /// </summary>
        /// <value>The package path.</value>
        [DataMemberIgnore]
        public UFile FullPath
        {
            get
            {
                return packagePath;
            }
            set
            {
                SetPackagePath(value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has been modified since last saving.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                var oldValue = isDirty;
                isDirty = value;
                OnAssetDirtyChanged(this, oldValue, value);
            }
        }

        [DataMemberIgnore]
        public PackageState State { get; set; }

        /// <summary>
        /// Gets the top directory of this package on the local disk.
        /// </summary>
        /// <value>The top directory.</value>
        [DataMemberIgnore]
        public UDirectory RootDirectory => FullPath?.GetParent();

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        /// <exception cref="System.InvalidOperationException">Cannot attach a package to more than one session</exception>
        [DataMemberIgnore]
        public PackageSession Session
        {
            get
            {
                return session;
            }
            internal set
            {
                if (value != null && session != null && !ReferenceEquals(session, value))
                {
                    throw new InvalidOperationException("Cannot attach a package to more than one session");
                }
                session = value;
                IsIdLocked = (session != null);
            }
        }

        /// <summary>
        /// Gets the package user settings. Usually stored in a .user file alongside the package. Lazily loaded on first time.
        /// </summary>
        /// <value>
        /// The package user settings.
        /// </value>
        [DataMemberIgnore]
        public PackageUserSettings UserSettings => settings.Value;

        /// <summary>
        /// Gets the list of assemblies loaded by this package.
        /// </summary>
        /// <value>
        /// The loaded assemblies.
        /// </value>
        [DataMemberIgnore]
        public List<PackageLoadedAssembly> LoadedAssemblies { get; } = new List<PackageLoadedAssembly>();

        /// <summary>
        /// Adds an exiting project to this package.
        /// </summary>
        /// <param name="pathToMsproj">The path to msproj.</param>
        /// <returns>LoggerResult.</returns>
        public LoggerResult AddExitingProject(UFile pathToMsproj)
        {
            var logger = new LoggerResult();
            AddExitingProject(pathToMsproj, logger);
            return logger;
        }

        /// <summary>
        /// Adds an exiting project to this package.
        /// </summary>
        /// <param name="pathToMsproj">The path to msproj.</param>
        /// <param name="logger">The logger.</param>
        public void AddExitingProject(UFile pathToMsproj, LoggerResult logger)
        {
            if (pathToMsproj == null) throw new ArgumentNullException(nameof(pathToMsproj));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (!pathToMsproj.IsAbsolute) throw new ArgumentException(@"Expecting relative path", nameof(pathToMsproj));

            try
            {
                // Load a project without specifying a platform to make sure we get the correct platform type
                var msProject = VSProjectHelper.LoadProject(pathToMsproj, platform: "NoPlatform");
                try
                {

                    var projectType = VSProjectHelper.GetProjectTypeFromProject(msProject);
                    if (!projectType.HasValue)
                    {
                        logger.Error("This project is not a project created with the editor");
                    }
                    else
                    {
                        var platformType = VSProjectHelper.GetPlatformTypeFromProject(msProject) ?? PlatformType.Shared;

                        var projectReference = new ProjectReference()
                        {
                            Id = VSProjectHelper.GetProjectGuid(msProject),
                            Location = pathToMsproj.MakeRelative(RootDirectory),
                            Type = projectType.Value
                        };

                        // Add the ProjectReference only for the compatible profiles (same platform or no platform)
                        foreach (var profile in Profiles.Where(profile => platformType == profile.Platform))
                        {
                            profile.ProjectReferences.Add(projectReference);
                        }
                    }
                }
                finally
                {
                    msProject.ProjectCollection.UnloadAllProjects();
                    msProject.ProjectCollection.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unexpected exception while loading project [{0}]", ex, pathToMsproj);
            }
        }

        internal UDirectory GetDefaultAssetFolder()
        {
            var sharedProfile = Profiles.FindSharedProfile();
            var folder = sharedProfile?.AssetFolders.FirstOrDefault();
            return folder?.Path ?? ("Assets/" + PackageProfile.SharedName);
        }


        /// <summary>
        /// Deep clone this package.
        /// </summary>
        /// <returns>The package cloned.</returns>
        public Package Clone()
        {
            // Use a new ShadowRegistry to copy override parameters
            // Clone this asset
            var package = (Package)AssetCloner.Clone(this); 
            package.FullPath = FullPath;
            foreach (var asset in Assets)
            {
                var newAsset = asset.Asset;
                var assetItem = new AssetItem(asset.Location, newAsset)
                {
                    SourceFolder = asset.SourceFolder,
                    SourceProject = asset.SourceProject
                };
                package.Assets.Add(assetItem);
            }
            return package;
        }

        /// <summary>
        /// Sets the package path.
        /// </summary>
        /// <param name="newPath">The new path.</param>
        /// <param name="copyAssets">if set to <c>true</c> assets will be copied relatively to the new location.</param>
        public void SetPackagePath(UFile newPath, bool copyAssets = true)
        {
            var previousPath = packagePath;
            var previousRootDirectory = RootDirectory;
            packagePath = newPath;
            if (packagePath != null && !packagePath.IsAbsolute)
            {
                packagePath = UPath.Combine(Environment.CurrentDirectory, packagePath);
            }

            if (copyAssets && packagePath != previousPath)
            {
                // Update source folders
                var currentRootDirectory = RootDirectory;
                if (previousRootDirectory != null && currentRootDirectory != null)
                {
                    foreach (var profile in Profiles)
                    {
                        foreach (var sourceFolder in profile.AssetFolders)
                        {
                            if (sourceFolder.Path.IsAbsolute)
                            {
                                var relativePath = sourceFolder.Path.MakeRelative(previousRootDirectory);
                                sourceFolder.Path = UPath.Combine(currentRootDirectory, relativePath);
                            }
                        }
                    }
                }

                foreach (var asset in Assets)
                {
                    asset.IsDirty = true;
                }
                IsDirty = true;
            }
        }

        internal void OnAssetDirtyChanged(Asset asset, bool oldValue, bool newValue)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            AssetDirtyChanged?.Invoke(asset, oldValue, newValue);
        }

        /// <summary>
        /// Saves this package and all dirty assets. See remarks.
        /// </summary>
        /// <param name="saveAllAssets">if set to <c>true</c> [save all assets].</param>
        /// <returns>LoggerResult.</returns>
        /// <remarks>When calling this method directly, it does not handle moving assets between packages. 
        /// Call <see cref="PackageSession.Save"/> instead.
        /// </remarks>
        public LoggerResult Save()
        {
            var result = new LoggerResult();
            Save(result);
            return result;
        }

        /// <summary>
        /// Saves this package and all dirty assets. See remarks.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <exception cref="System.ArgumentNullException">log</exception>
        /// <remarks>When calling this method directly, it does not handle moving assets between packages.
        /// Call <see cref="PackageSession.Save" /> instead.</remarks>
        public void Save(ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            if (FullPath == null)
            {
                log.Error(this, null, AssetMessageCode.PackageCannotSave, "null");
                return;
            }

            // Use relative paths when saving
            var analysis = new PackageAnalysis(this, new PackageAnalysisParameters()
            {
                SetDirtyFlagOnAssetWhenFixingUFile = false,
                ConvertUPathTo = UPathType.Relative,
                IsProcessingUPaths = true,
                AssetTemplatingRemoveUnusedBaseParts = true,
            });
            analysis.Run(log);

            try
            {
                // Update source folders
                UpdateSourceFolders();

                if (IsDirty)
                {
                    List<UFile> filesToDeleteLocal;
                    lock (filesToDelete)
                    {
                        filesToDeleteLocal = filesToDelete.ToList();
                        filesToDelete.Clear();
                    }

                    try
                    {
                        AssetSerializer.Save(FullPath, this);

                        // Move the package if the path has changed
                        if (previousPackagePath != null && previousPackagePath != packagePath)
                        {
                            filesToDeleteLocal.Add(previousPackagePath);
                        }
                        previousPackagePath = packagePath;

                        IsDirty = false;
                    }
                    catch (Exception ex)
                    {
                        log.Error(this, null, AssetMessageCode.PackageCannotSave, ex, FullPath);
                        return;
                    }
                    
                    // Delete obsolete files
                    foreach (var file in filesToDeleteLocal)
                    {
                        if (File.Exists(file.FullPath))
                        {
                            try
                            {
                                File.Delete(file.FullPath);
                            }
                            catch (Exception ex)
                            {
                                log.Error(this, null, AssetMessageCode.AssetCannotDelete, ex, file.FullPath);
                            }
                        }
                    }
                }

                //batch projects
                var vsProjs = new Dictionary<string, Project>();

                foreach (var asset in Assets)
                {
                    if (asset.IsDirty)
                    {
                        var assetPath = asset.FullPath;

                        try
                        {
                            //Handle the ProjectSourceCodeAsset differently then regular assets in regards of Path
                            var sourceCodeAsset = asset.Asset as ProjectSourceCodeAsset;
                            if (sourceCodeAsset != null)
                            {
                                var profile = Profiles.FindSharedProfile();

                                var lib = profile?.ProjectReferences.FirstOrDefault(x => x.Type == ProjectType.Library && asset.Location.FullPath.StartsWith(x.Location.GetFileName()));
                                if (lib == null) continue;

                                var projectFullPath = UPath.Combine(RootDirectory, lib.Location);
                                var fileFullPath = UPath.Combine(RootDirectory, asset.Location);
                                var filePath = fileFullPath.MakeRelative(projectFullPath.GetFullDirectory());
                                var codeFile = new UFile(filePath + AssetRegistry.GetDefaultExtension(sourceCodeAsset.GetType()));

                                Project project;
                                if (!vsProjs.TryGetValue(projectFullPath, out project))
                                {
                                    project = VSProjectHelper.LoadProject(projectFullPath);
                                    vsProjs.Add(projectFullPath, project);
                                }

                                asset.SourceProject = projectFullPath;
                                asset.SourceFolder = RootDirectory.GetFullDirectory();
                                sourceCodeAsset.ProjectInclude = codeFile;
                                sourceCodeAsset.ProjectName = Path.GetFileNameWithoutExtension(projectFullPath.ToWindowsPath());
                                sourceCodeAsset.AbsoluteSourceLocation = UPath.Combine(projectFullPath.GetFullDirectory(), codeFile);
                                sourceCodeAsset.AbsoluteProjectLocation = projectFullPath;
                                assetPath = sourceCodeAsset.AbsoluteSourceLocation;

                                //check if the item is already there, this is possible when saving the first time when creating from a template
                                if (project.Items.All(x => x.EvaluatedInclude != codeFile.ToWindowsPath()))
                                {
                                    var generatorAsset = sourceCodeAsset as ProjectCodeGeneratorAsset;
                                    if (generatorAsset != null)
                                    {
                                        generatorAsset.GeneratedAbsolutePath = new UFile(generatorAsset.AbsoluteSourceLocation).GetFullPathWithoutExtension() + ".cs";
                                        generatorAsset.GeneratedInclude = new UFile(generatorAsset.ProjectInclude).GetFullPathWithoutExtension() + ".cs";

                                        project.AddItem("None", codeFile.ToWindowsPath(), 
                                            new List<KeyValuePair<string, string>>
                                            {
                                                new KeyValuePair<string, string>("Generator", generatorAsset.Generator),
                                                new KeyValuePair<string, string>("LastGenOutput", new UFile(generatorAsset.GeneratedInclude).GetFileNameWithExtension())
                                            });

                                        project.AddItem("Compile", new UFile(generatorAsset.GeneratedInclude).ToWindowsPath(),
                                            new List<KeyValuePair<string, string>>
                                            {
                                                new KeyValuePair<string, string>("AutoGen", "True"),
                                                new KeyValuePair<string, string>("DesignTime", "True"),
                                                new KeyValuePair<string, string>("DesignTimeSharedInput", "True"),
                                                new KeyValuePair<string, string>("DependentUpon", new UFile(generatorAsset.ProjectInclude).GetFileNameWithExtension())
                                            });
                                    }
                                    else
                                    {
                                        project.AddItem("Compile", codeFile.ToWindowsPath());
                                    }                                
                                }
                            }

                            // Inject a copy of the base into the current asset when saving
                            var assetBase = asset.Asset.Base;
                            if (assetBase != null && !assetBase.IsRootImport)
                            {
                                asset.Asset.Base = UpdateAssetBase(assetBase);
                            }

                            // Update base for BaseParts
                            if (asset.Asset.BaseParts != null)
                            {
                                var baseParts = asset.Asset.BaseParts;
                                for (int i = 0; i < baseParts.Count; i++)
                                {
                                    var basePart = baseParts[i];
                                    baseParts[i] = UpdateAssetBase(basePart);
                                }
                            }

                            AssetSerializer.Save(assetPath, asset.Asset);
                            asset.IsDirty = false;
                        }
                        catch (Exception ex)
                        {
                            log.Error(this, asset.ToReference(), AssetMessageCode.AssetCannotSave, ex, assetPath);
                        }
                    }
                }

                foreach (var project in vsProjs.Values)
                {
                    project.Save();
                    project.ProjectCollection.UnloadAllProjects();
                    project.ProjectCollection.Dispose();
                }

                Assets.IsDirty = false;

                // Save properties like the Xenko version used
                PackageSessionHelper.SaveProperties(this);
            }
            finally
            {
                // Rollback all relative UFile to absolute paths
                analysis.Parameters.ConvertUPathTo = UPathType.Absolute;
                analysis.Run();
            }
        }

        /// <summary>
        /// Finds the most recent asset base and return a new version of it.
        /// </summary>
        /// <param name="assetBase">The original asset base</param>
        /// <returns>A copy of the asset base updated with the latest base</returns>
        private AssetBase UpdateAssetBase(AssetBase assetBase)
        {
            var assetBaseItem = session != null ? session.FindAsset(assetBase.Id) : Assets.Find(assetBase.Id);
            if (assetBaseItem != null)
            {
                var newBase = (Asset)AssetCloner.Clone(assetBaseItem.Asset);
                return new AssetBase(assetBase.Location, newBase);
            }
            // TODO: If we don't find it, should we log an error instead?
            return assetBase;
        }

        /// <summary>
        /// Gets the package identifier from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Guid.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// log
        /// or
        /// filePath
        /// </exception>
        public static Guid GetPackageIdFromFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            bool hasPackage = false;
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("!Package"))
                    {
                        hasPackage = true;
                    }

                    if (hasPackage && line.StartsWith("Id:"))
                    {
                        var id = line.Substring("Id:".Length).Trim();
                        return Guid.Parse(id);
                    }
                }
            }
            throw new IOException($"File {filePath} doesn't appear to be a valid package");
        }

        /// <summary>
        /// Loads only the package description but not assets or plugins.
        /// </summary>
        /// <param name="log">The log to receive error messages.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        /// <returns>A package.</returns>
        /// <exception cref="System.ArgumentNullException">log
        /// or
        /// filePath</exception>
        public static Package Load(ILogger log, string filePath, PackageLoadParameters loadParametersArg = null)
        {
            var package = LoadRaw(log, filePath);
            if (package != null)
            {
                if (!package.LoadAssembliesAndAssets(log, loadParametersArg))
                    package = null;
            }

            return package;
        }

        /// <summary>
        /// Performs first part of the loading sequence, by deserializing the package but without processing anything yet.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// log
        /// or
        /// filePath
        /// </exception>
        internal static Package LoadRaw(ILogger log, string filePath)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            filePath = FileUtility.GetAbsolutePath(filePath);

            if (!File.Exists(filePath))
            {
                log.Error("Package file [{0}] was not found", filePath);
                return null;
            }

            try
            {
                bool aliasOccurred;
                var packageFile = new PackageLoadingAssetFile(filePath, Path.GetDirectoryName(filePath));
                var context = new AssetMigrationContext(null, log);
                AssetMigration.MigrateAssetIfNeeded(context, packageFile, "Assets");

                var package = packageFile.AssetContent != null
                    ? (Package)AssetSerializer.Load(new MemoryStream(packageFile.AssetContent), Path.GetExtension(filePath), log, out aliasOccurred)
                    : AssetSerializer.Load<Package>(filePath, log, out aliasOccurred);

                package.FullPath = filePath;
                package.previousPackagePath = package.FullPath;
                package.IsDirty = packageFile.AssetContent != null || aliasOccurred;

                return package;
            }
            catch (Exception ex)
            {
                log.Error("Error while pre-loading package [{0}]", ex, filePath);
            }

            return null;
        }

        /// <summary>
        /// Second part of the package loading process, when references, assets and package analysis is done.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        /// <returns></returns>
        internal bool LoadAssembliesAndAssets(ILogger log, PackageLoadParameters loadParametersArg)
        {
            return LoadAssemblies(log, loadParametersArg) && LoadAssets(log, loadParametersArg);
        }

        /// <summary>
        /// Load only assembly references
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        /// <returns></returns>
        internal bool LoadAssemblies(ILogger log, PackageLoadParameters loadParametersArg)
        {
            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            try
            {
                // Load assembly references
                if (loadParameters.LoadAssemblyReferences)
                {
                    LoadAssemblyReferencesForPackage(log, loadParameters);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error while pre-loading package [{0}]", ex, FullPath);

                return false;
            }
        }

        /// <summary>
        /// Load assets and perform package analysis.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        /// <returns></returns>
        internal bool LoadAssets(ILogger log, PackageLoadParameters loadParametersArg)
        {
            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            try
            {
                // Load assets
                if (loadParameters.AutoLoadTemporaryAssets)
                {
                    LoadTemporaryAssets(log, loadParameters.AssetFiles, loadParameters.CancelToken, loadParameters.TemporaryAssetsInMsbuild, loadParameters.TemporaryAssetFilter);
                }

                // Convert UPath to absolute
                if (loadParameters.ConvertUPathToAbsolute)
                {
                    var analysis = new PackageAnalysis(this, new PackageAnalysisParameters()
                    {
                        ConvertUPathTo = UPathType.Absolute,
                        IsProcessingUPaths = true, // This is done already by Package.Load
                        SetDirtyFlagOnAssetWhenFixingAbsoluteUFile = true // When loading tag attributes that have an absolute file
                    });
                    analysis.Run(log);
                }

                // Load templates
                LoadTemplates(log);

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error while pre-loading package [{0}]", ex, FullPath);

                return false;
            }
        }

        public void ValidateAssets(bool alwaysGenerateNewAssetId = false)
        {
            if (TemporaryAssets.Count == 0)
            {
                return;
            }

            try
            {
                // Make sure we are suspending notifications before updating all assets
                Assets.SuspendCollectionChanged();

                Assets.Clear();

                // Get generated output items
                var outputItems = new AssetItemCollection();

                // Create a resolver from the package
                var resolver = AssetResolver.FromPackage(this);
                resolver.AlwaysCreateNewId = alwaysGenerateNewAssetId;

                // Clean assets
                AssetCollision.Clean(this, TemporaryAssets, outputItems, resolver, true);

                // Add them back to the package
                foreach (var item in outputItems)
                {
                    Assets.Add(item);
                }

                // Don't delete SourceCodeAssets as their files are handled by the package upgrader
                var dirtyAssets = outputItems.Where(o => o.IsDirty && !(o.Asset is SourceCodeAsset))
                    .Join(TemporaryAssets, o => o.Id, t => t.Id, (o, t) => t)
                    .ToList();
                // Dirty assets (except in system package) should be mark as deleted so that are properly saved again later.
                if (!IsSystem && dirtyAssets.Count > 0)
                {
                    IsDirty = true;

                    lock (filesToDelete)
                    {
                        filesToDelete.AddRange(dirtyAssets.Select(a => a.FullPath));
                    }
                }

                TemporaryAssets.Clear();
            }
            finally
            {
                // Restore notification on assets
                Assets.ResumeCollectionChanged();
            }
        }

        /// <summary>
        /// Refreshes this package from the disk by loading or reloading all assets.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="assetFiles">The asset files (loaded from <see cref="ListAssetFiles"/> if null).</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="listAssetsInMsbuild">Specifies if we need to evaluate MSBuild files for assets.</param>
        /// <param name="filterFunc">A function that will filter assets loading</param>
        /// <returns>A logger that contains error messages while refreshing.</returns>
        /// <exception cref="System.InvalidOperationException">Package RootDirectory is null
        /// or
        /// Package RootDirectory [{0}] does not exist.ToFormat(RootDirectory)</exception>
        public void LoadTemporaryAssets(ILogger log, IList<PackageLoadingAssetFile> assetFiles = null, CancellationToken? cancelToken = null, bool listAssetsInMsbuild = true, Func<PackageLoadingAssetFile, bool> filterFunc = null)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            // If FullPath is null, then we can't load assets from disk, just return
            if (FullPath == null)
            {
                log.Warning("Fullpath not set on this package");
                return;
            }

            // Clears the assets already loaded and reload them
            TemporaryAssets.Clear();

            // List all package files on disk
            if (assetFiles == null)
                assetFiles = ListAssetFiles(log, this, listAssetsInMsbuild, cancelToken);

            var progressMessage = $"Loading Assets from Package [{FullPath.GetFileNameWithExtension()}]";

            // Display this message at least once if the logger does not log progress (And it shouldn't in this case)
            var loggerResult = log as LoggerResult;
            if (loggerResult == null || !loggerResult.IsLoggingProgressAsInfo)
            {
                log.Info(progressMessage);
            }


            var context = new AssetMigrationContext(this, log);

            // Update step counter for log progress
            var tasks = new List<System.Threading.Tasks.Task>();
            for (int i = 0; i < assetFiles.Count; i++)
            {
                var assetFile = assetFiles[i];

                if (filterFunc != null && !filterFunc(assetFile))
                {
                    continue;
                }

                // Update the loading progress
                loggerResult?.Progress(progressMessage, i, assetFiles.Count);

                var task = cancelToken.HasValue ?
                    System.Threading.Tasks.Task.Factory.StartNew(() => LoadAsset(context, assetFile, loggerResult), cancelToken.Value) : 
                    System.Threading.Tasks.Task.Factory.StartNew(() => LoadAsset(context, assetFile, loggerResult));

                tasks.Add(task);
            }

            if (cancelToken.HasValue)
            {
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), cancelToken.Value);
            }
            else
            {
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            }

            // DEBUG
            // StaticLog.Info("[{0}] Assets files loaded in {1}", assetFiles.Count, clock.ElapsedMilliseconds);

            if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
            {
                log.Warning("Skipping loading assets. PackageSession.Load cancelled");
            }
        }

        private void LoadAsset(AssetMigrationContext context, PackageLoadingAssetFile assetFile, LoggerResult loggerResult)
        {
            var fileUPath = assetFile.FilePath;
            var sourceFolder = assetFile.SourceFolder;

            // Check if asset has been deleted by an upgrader
            if (assetFile.Deleted)
            {
                IsDirty = true;

                lock (filesToDelete)
                {
                    filesToDelete.Add(assetFile.FilePath);
                }

                // Don't create temporary assets for files deleted during package upgrading
                return;
            }

            // An exception can occur here, so we make sure that loading a single asset is not going to break 
            // the loop
            try
            {
                AssetMigration.MigrateAssetIfNeeded(context, assetFile, PackageStore.Instance.DefaultPackageName);

                // Try to load only if asset is not already in the package or assetRef.Asset is null
                var assetPath = assetFile.AssetPath;

                var assetFullPath = fileUPath.FullPath;
                var assetContent = assetFile.AssetContent;

                var projectInclude = assetFile.ProjectFile != null ? fileUPath.MakeRelative(assetFile.ProjectFile.GetFullDirectory()) : null;

                bool aliasOccurred;
                var asset = LoadAsset(context.Log, assetFullPath, assetPath, assetFile.ProjectFile, projectInclude, assetContent, out aliasOccurred);

                // Create asset item
                var assetItem = new AssetItem(assetPath, asset, this)
                {
                    IsDirty = assetContent != null || aliasOccurred,
                    SourceFolder = sourceFolder.MakeRelative(RootDirectory),
                    SourceProject = asset is SourceCodeAsset && assetFile.ProjectFile != null ? assetFile.ProjectFile : null
                };

                // Set the modified time to the time loaded from disk
                if (!assetItem.IsDirty)
                    assetItem.ModifiedTime = File.GetLastWriteTime(assetFullPath);

                // TODO: Let's review that when we rework import process
                // Not fixing asset import anymore, as it was only meant for upgrade
                // However, it started to make asset dirty, for ex. when we create a new texture, choose a file and reload the scene later
                // since there was no importer id and base.
                //FixAssetImport(assetItem);

                // Add to temporary assets
                lock (TemporaryAssets)
                {
                    TemporaryAssets.Add(assetItem);
                }
            }
            catch (Exception ex)
            {
                int row = 1;
                int column = 1;
                var yamlException = ex as YamlException;
                if (yamlException != null)
                {
                    row = yamlException.Start.Line + 1;
                    column = yamlException.Start.Column;
                }

                var module = context.Log.Module;

                var assetReference = new AssetReference<Asset>(Guid.Empty, fileUPath.FullPath);

                // TODO: Change this instead of patching LoggerResult.Module, use a proper log message
                if (loggerResult != null)
                {
                    loggerResult.Module = "{0}({1},{2})".ToFormat(Path.GetFullPath(fileUPath.FullPath), row, column);
                }

                context.Log.Error(this, assetReference, AssetMessageCode.AssetLoadingFailed, ex, fileUPath, ex.Message);

                if (loggerResult != null)
                {
                    loggerResult.Module = module;
                }
            }
        }

        /// <summary>
        /// Loads the assembly references that were not loaded before.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        public void UpdateAssemblyReferences(ILogger log, PackageLoadParameters loadParametersArg = null)
        {
            if (State < PackageState.DependenciesReady)
                return;

            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();
            LoadAssemblyReferencesForPackage(log, loadParameters);
        }

        private static Asset LoadAsset(ILogger log, string assetFullPath, string assetPath, string projectFullPath, string projectInclude, byte[] assetContent, out bool assetDirty)
        {
            var asset = assetContent != null
                ? (Asset)AssetSerializer.Load(new MemoryStream(assetContent), Path.GetExtension(assetFullPath), log, out assetDirty)
                : AssetSerializer.Load<Asset>(assetFullPath, log, out assetDirty);

            // Set location on source code asset
            var sourceCodeAsset = asset as SourceCodeAsset;
            if (sourceCodeAsset != null)
            {
                // Use an id generated from the location instead of the default id
                sourceCodeAsset.Id = SourceCodeAsset.GenerateGuidFromLocation(assetPath);
                sourceCodeAsset.AbsoluteSourceLocation = assetFullPath;

                var projectSourceCodeAsset = asset as ProjectSourceCodeAsset;
                if (projectSourceCodeAsset != null)
                {
                    projectSourceCodeAsset.AbsoluteProjectLocation = projectFullPath;
                    projectSourceCodeAsset.ProjectInclude = projectInclude;
                    projectSourceCodeAsset.ProjectName = Path.GetFileNameWithoutExtension(projectFullPath);
                }

                var generatorAsset = asset as ProjectCodeGeneratorAsset;
                if (generatorAsset != null)
                {
                    generatorAsset.GeneratedAbsolutePath = new UFile(sourceCodeAsset.AbsoluteSourceLocation).GetFullPathWithoutExtension() + ".cs"; //we generate only .cs so far
                }
            }

            return asset;
        }

        private void LoadAssemblyReferencesForPackage(ILogger log, PackageLoadParameters loadParameters)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (loadParameters == null) throw new ArgumentNullException(nameof(loadParameters));
            var assemblyContainer = loadParameters.AssemblyContainer ?? AssemblyContainer.Default;
            foreach (var profile in Profiles)
            {
                foreach (var projectReference in profile.ProjectReferences.Where(projectRef => projectRef.Type == ProjectType.Plugin || projectRef.Type == ProjectType.Library))
                {
                    // Check if already loaded
                    // TODO: More advanced cases: unload removed references, etc...
                    if (LoadedAssemblies.Any(x => x.ProjectReference == projectReference))
                        continue;

                    string assemblyPath = null;
                    var fullProjectLocation = UPath.Combine(RootDirectory, projectReference.Location);

                    try
                    {
                        var forwardingLogger = new ForwardingLoggerResult(log);
                        assemblyPath = VSProjectHelper.GetOrCompileProjectAssembly(fullProjectLocation, forwardingLogger, loadParameters.AutoCompileProjects, loadParameters.BuildConfiguration, extraProperties: loadParameters.ExtraCompileProperties, onlyErrors: true);
                        if (String.IsNullOrWhiteSpace(assemblyPath))
                        {
                            log.Error("Unable to locate assembly reference for project [{0}]", fullProjectLocation);
                            continue;
                        }

                        var loadedAssembly = new PackageLoadedAssembly(projectReference, assemblyPath);
                        LoadedAssemblies.Add(loadedAssembly);

                        if (!File.Exists(assemblyPath) || forwardingLogger.HasErrors)
                        {
                            log.Error("Unable to build assembly reference [{0}]", assemblyPath);
                            continue;
                        }

                        var assembly = assemblyContainer.LoadAssemblyFromPath(assemblyPath, log);
                        if (assembly == null)
                        {
                            log.Error("Unable to load assembly reference [{0}]", assemblyPath);
                        }

                        loadedAssembly.Assembly = assembly;

                        if (assembly != null)
                        {
                            // Register assembly in the registry
                            AssemblyRegistry.Register(assembly, AssemblyCommonCategories.Assets);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unexpected error while loading project [{0}] or assembly reference [{1}]", ex, fullProjectLocation, assemblyPath);
                    }
                }
            }
        }

        private void UpdateSourceFolders()
        {
            // If there are not assets, we don't need to update or create an asset folder
            if (Assets.Count == 0)
            {
                return;
            }

            // Make sure there is a shared profile at least
            var sharedProfile = Profiles.FindSharedProfile();
            if (sharedProfile == null)
            {
                sharedProfile = PackageProfile.NewShared();
                Profiles.Add(sharedProfile);
            }

            // Use by default the first asset folders if not defined on the asset item
            var defaultFolder = sharedProfile.AssetFolders.Count > 0 ? sharedProfile.AssetFolders.First().Path : UDirectory.This;
            var assetFolders = new HashSet<UDirectory>(GetDistinctAssetFolderPaths());
            foreach (var asset in Assets)
            {
                if(asset.SourceProject != null) continue; //We don't add assets that depend on a project to the asset folders

                if (asset.SourceFolder == null)
                {
                    asset.SourceFolder = defaultFolder.IsAbsolute ? defaultFolder.MakeRelative(RootDirectory) : defaultFolder;
                    asset.IsDirty = true;
                }

                var assetFolderAbsolute = UPath.Combine(RootDirectory, asset.SourceFolder);
                if (!assetFolders.Contains(assetFolderAbsolute) && asset.SourceProject == null) //ignore assets that depend on a csproj
                {
                    assetFolders.Add(assetFolderAbsolute);
                    sharedProfile.AssetFolders.Add(new AssetFolder(assetFolderAbsolute));
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Loads the templates.
        /// </summary>
        /// <param name="log">The log result.</param>
        private void LoadTemplates(ILogger log)
        {
            foreach (var templateDir in TemplateFolders)
            {
                foreach (var filePath in templateDir.Files)
                {
                    try
                    {
                        var file = new FileInfo(filePath);
                        if (!file.Exists)
                        {
                            log.Warning("Template [{0}] does not exist ", file);
                            continue;
                        }

                        bool aliasOccurred;
                        var templateDescription = AssetSerializer.Load<TemplateDescription>(file.FullName, null, out aliasOccurred);
                        templateDescription.FullPath = file.FullName;
                        Templates.Add(templateDescription);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error while loading template from [{0}]", ex, filePath);
                    }
                }
            }
        }

        private List<UDirectory> GetDistinctAssetFolderPaths()
        {
            var existingAssetFolders = new List<UDirectory>();
            foreach (var profile in Profiles)
            {
                foreach (var folder in profile.AssetFolders)
                {
                    var folderPath = RootDirectory != null ? UPath.Combine(RootDirectory, folder.Path) : folder.Path;
                    if (!existingAssetFolders.Contains(folderPath))
                    {
                        existingAssetFolders.Add(folderPath);
                    }
                }
            }
            return existingAssetFolders;
        }

        public static List<PackageLoadingAssetFile> ListAssetFiles(ILogger log, Package package, bool listAssetsInMsbuild, CancellationToken? cancelToken)
        {
            var listFiles = new List<PackageLoadingAssetFile>();

            // TODO Check how to handle refresh correctly as a public API
            if (package.RootDirectory == null)
            {
                throw new InvalidOperationException("Package RootDirectory is null");
            }

            if (!Directory.Exists(package.RootDirectory))
            {
                return listFiles;
            }

            var sharedProfile = package.Profiles.FindSharedProfile();
            var hasProject = sharedProfile != null && sharedProfile.ProjectReferences.Count > 0;

            // Iterate on each source folders
            foreach (var sourceFolder in package.GetDistinctAssetFolderPaths())
            {
                // Lookup all files
                foreach (var directory in FileUtility.EnumerateDirectories(sourceFolder, SearchDirection.Down))
                {
                    var files = directory.GetFiles();

                    foreach (var filePath in files)
                    {
                        // Don't load package via this method
                        if (filePath.FullName.EndsWith(PackageFileExtension))
                        {
                            continue;
                        }

                        // Make an absolute path from the root of this package
                        var fileUPath = new UFile(filePath.FullName);
                        if (fileUPath.GetFileExtension() == null)
                        {
                            continue;
                        }

                        // If this kind of file an asset file?
                        var ext = fileUPath.GetFileExtension();

                        //make sure to add default shaders in this case, since we don't have a csproj for them
                        if (AssetRegistry.IsProjectCodeGeneratorAssetFileExtension(ext) && !hasProject)
                        {
                            listFiles.Add(new PackageLoadingAssetFile(fileUPath, sourceFolder));
                            continue;
                        }

                        if (!AssetRegistry.IsAssetFileExtension(ext) || AssetRegistry.IsProjectSourceCodeAssetFileExtension(ext)) //project source code assets follow the csproj pipeline
                        {
                            continue;
                        }

                        listFiles.Add(new PackageLoadingAssetFile(fileUPath, sourceFolder));
                    }
                }
            }

            //find also assets in the csproj
            if (listAssetsInMsbuild)
            {
                FindCodeAssetsInProject(listFiles, package);
            }

            return listFiles;
        }

        public static List<string> FindCodeAssetsInProject(string projectFullPath, out string nameSpace)
        {
            var realFullPath = new UFile(projectFullPath);
            var project = VSProjectHelper.LoadProject(realFullPath);
            var dir = new UDirectory(realFullPath.GetFullDirectory());

            var nameSpaceProp = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "RootNamespace");
            nameSpace = nameSpaceProp?.EvaluatedValue ?? string.Empty;

            var result = project.Items.Where(x => (x.ItemType == "Compile" || x.ItemType == "None") && string.IsNullOrEmpty(x.GetMetadataValue("AutoGen")))
                .Select(x => new UFile(x.EvaluatedInclude)).Where(x => AssetRegistry.IsProjectSourceCodeAssetFileExtension(x.GetFileExtension()))
                .Select(projectItem => UPath.Combine(dir, projectItem)).Select(csPath => (string)csPath).ToList();

            project.ProjectCollection.UnloadAllProjects();
            project.ProjectCollection.Dispose();

            return result;
        }

        private static void FindCodeAssetsInProject(ICollection<PackageLoadingAssetFile> list, Package package)
        {
            if (package.IsSystem) return;

            var profile = package.Profiles.FindSharedProfile();
            if (profile == null) return;

            foreach (var libs in profile.ProjectReferences.Where(x => x.Type == ProjectType.Library))
            {
                var realFullPath = UPath.Combine(package.RootDirectory, libs.Location);
                string defaultNamespace;
                var codePaths = FindCodeAssetsInProject(realFullPath, out defaultNamespace);
                libs.RootNamespace = defaultNamespace;
                var dir = new UDirectory(realFullPath.GetFullDirectory());
                var parentDir = dir.GetParent();

                foreach (var codePath in codePaths)
                {
                    list.Add(new PackageLoadingAssetFile(codePath, parentDir, realFullPath));
                }
            }
        }

        private class RemoveRawImports : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Profiles != null)
                {
                    var profiles = asset.Profiles;

                    foreach (var profile in profiles)
                    {
                        var folders = profile.AssetFolders;
                        if (folders != null)
                        {
                            foreach (var folder in folders)
                            {
                                if (folder.RawImports != null)
                                {
                                    folder.RemoveChild("RawImports");
                                }
                            }
                        }
                    }
                }
            }
        }

        private class RenameSystemPackage : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var dependencies = asset.Meta?.Dependencies;

                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        if (dependency.Name == "Paradox")
                            dependency.Name = "Xenko";
                    }
                }
            }
        }
    }
}
