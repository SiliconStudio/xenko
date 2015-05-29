// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A package assets compiler.
    /// Creates the build steps necessary to produce the assets of a package.
    /// </summary>
    public class PackageCompiler : IPackageCompiler
    {
        private static readonly AssetCompilerRegistry assetCompilerRegistry = new AssetCompilerRegistry();
        private readonly List<IPackageCompiler> compilers;

        static PackageCompiler()
        {
            // Compute ParadoxSdkDir from this assembly
            // TODO Move this code to a reusable method
            var codeBase = typeof(PackageCompiler).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            SdkDirectory = Path.GetFullPath(Path.Combine(path, @"..\.."));
        }

        /// <summary>
        /// Raised when a single asset has been compiled.
        /// </summary>
        public EventHandler<AssetCompiledArgs> AssetCompiled;

        public PackageCompiler()
        {
            compilers = new List<IPackageCompiler>();
        }

        /// <summary>
        /// Gets or sets the SDK directory. Default is bound to env variable ParadoxSdkDir
        /// </summary>
        /// <value>The SDK directory.</value>
        public static string SdkDirectory { get; set; }


        /// <summary>
        /// Compile the current package session.
        /// That is generate the list of build steps to execute to create the package assets.
        /// </summary>
        public AssetCompilerResult Compile(AssetCompilerContext compilerContext)
        {
            if (compilerContext == null) throw new ArgumentNullException("compilerContext");

            if (compilerContext.Package == null)
            {
                throw new ArgumentException("Expecting a non null package", "compilerContext");
            }

            if (compilerContext.Package.Session == null)
            {
                throw new ArgumentException("Expecting a package attached to a session", "compilerContext");
            }

            var result = new AssetCompilerResult();

            // Check integrity of the packages
            var packageAnalysis = new PackageSessionAnalysis(compilerContext.Package.Session, new PackageAnalysisParameters());
            packageAnalysis.Run(result);
            if (result.HasErrors)
            {
                return result;
            }

            // Add default compilers
            compilers.Clear();

            var defaultAssetsCompiler = new DefaultAssetsCompiler();
            defaultAssetsCompiler.AssetCompiled += OnAssetCompiled;
            compilers.Add(defaultAssetsCompiler); 

            var packagesProcessed = new HashSet<Package>();
            RecursiveCompile(result, compilerContext, packagesProcessed);

            return result;
        }

        /// <summary>
        /// Compile the current package and all child package recursively by generating a list of build steps
        /// </summary>
        private void RecursiveCompile(AssetCompilerResult result, AssetCompilerContext context, HashSet<Package> processed)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (context == null) throw new ArgumentNullException("context");
            if (context.Package == null) throw new ArgumentException("context.Package cannot be null", "context");

            if (processed.Contains(context.Package))
            {
                return;
            }
            processed.Add(context.Package);

            var package = context.Package;
            var session = package.Session;

            // 1. first recursively process all store packages
            foreach (var packageDependency in package.Meta.Dependencies)
            {
                var subPackage = session.Packages.Find(packageDependency);
                if (subPackage != null)
                {
                    // Work on an immutable copy for the whole set of assets to compile
                    var contextCopy = (AssetCompilerContext)context.Clone();
                    contextCopy.Package = subPackage;
                    RecursiveCompile(result, contextCopy, processed);
                }
                else
                {
                    result.Error("Unable to find package [{0}]", packageDependency);
                }
            }

            // 2. recursively process all local packages
            foreach (var subPackageReference in package.LocalDependencies)
            {
                var subPackage = session.Packages.Find(subPackageReference.Id);
                if (subPackage != null)
                {
                    // Work on an immutable copy for the whole set of assets to compile
                    var contextCopy = (AssetCompilerContext)context.Clone();
                    contextCopy.Package = subPackage;
                    RecursiveCompile(result, contextCopy, processed);
                }
                else
                {
                    result.Error("Unable to find package [{0}]", subPackageReference);
                }
            }

            result.Info("Compiling package [{0}]", package.FullPath);

            // Compile using all PackageCompiler
            foreach (var compiler in compilers)
            {
                var compilerResult = compiler.Compile(context);
                compilerResult.CopyTo(result);
                while (compilerResult.BuildSteps.Count > 0)
                {
                    var step = compilerResult.BuildSteps[0];
                    compilerResult.BuildSteps.RemoveAt(0);
                    result.BuildSteps.Add(step);
                }
            }
        }

        private void OnAssetCompiled(object sender, AssetCompiledArgs assetCompiledArgs)
        {
            var assetsCompiled = AssetCompiled;
            if (assetsCompiled != null)
            {
                assetsCompiled(this, assetCompiledArgs);
            }
        }

        /// <summary>
        /// Internal default compiler for compiling all assets 
        /// </summary>
        private class DefaultAssetsCompiler : ItemListCompiler, IPackageCompiler
        {
            public DefaultAssetsCompiler()
                : base(assetCompilerRegistry)
            {
            }

            public AssetCompilerResult Compile(AssetCompilerContext context)
            {
                var result = new AssetCompilerResult();

                var package = context.Package;

                // Sort the items to build by build order
                var assets = package.Assets.ToList();
                assets.Sort((item1, item2) => item1.Asset != null && item2.Asset != null ? item1.Asset.InternalBuildOrder.CompareTo(item2.Asset.InternalBuildOrder) : 0);

                // Import all assets provided by this psckage
                GenerateRawImportBuildSteps(context, result);

                // generate the build steps required to build the assets via base class
                Compile(context, assets, result);

                return result;
            }

            /// <summary>
            /// Generate the build step corresponding to raw imports of the current package file.
            /// </summary>
            /// <param name="context">The compilation context</param>
            /// <param name="result">The compilation current result</param>
            private void GenerateRawImportBuildSteps(AssetCompilerContext context, AssetCompilerResult result)
            {
                if (context.Package.RootDirectory == null)
                    return;

                foreach (var profile in context.Package.Profiles)
                {
                    foreach (var sourceFolder in profile.AssetFolders)
                    {
                        var baseDirectory = Path.GetFullPath(context.Package.RootDirectory);
                        // Use sub directory
                        baseDirectory = Path.Combine(baseDirectory, sourceFolder.Path);

                        if (!Directory.Exists(baseDirectory))
                        {
                            continue;
                        }

                        var baseUDirectory = new UDirectory(baseDirectory);
                        var hashSet = new HashSet<string>();

                        // Imports explicit
                        foreach (var rawImport in sourceFolder.RawImports)
                        {
                            var sourceDirectory = baseUDirectory;
                            if (!string.IsNullOrEmpty(rawImport.SourceDirectory))
                                sourceDirectory = UPath.Combine(sourceDirectory, rawImport.SourceDirectory);

                            if (!Directory.Exists(sourceDirectory))
                            {
                                result.Error("Unable to find raw import directory [{0}]", sourceDirectory);
                                continue;
                            }

                            var files = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
                            var importRegexes = rawImport.Patterns.Select(x => new Regex(Selectors.PathSelector.TransformToRegex(x))).ToArray();
                            foreach (var file in files)
                            {
                                var pathToFileRelativeToProject = new UFile(file).MakeRelative(sourceDirectory);
                                var outputPath = pathToFileRelativeToProject;
                                if (!string.IsNullOrEmpty(rawImport.TargetLocation))
                                    outputPath = UPath.Combine(rawImport.TargetLocation, outputPath);

                                foreach (var importRegex in importRegexes)
                                {
                                    if (importRegex.Match(pathToFileRelativeToProject).Success && hashSet.Add(outputPath))
                                    {
                                        result.BuildSteps.Add(new ImportStreamCommand
                                        {
                                            SourcePath = file,
                                            Location = outputPath,
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static readonly Regex ChangeWildcardToRegex = new Regex(@"(?<!\*)\*");

            private static Regex CompileRawImport(string rawImport)
            {
                // Replace / by \
                rawImport = rawImport.Replace("\\", "/");
                // escape . by \.
                rawImport = rawImport.Replace(".", "\\.");
                // Transform * by .*?
                rawImport = ChangeWildcardToRegex.Replace(rawImport, ".*?");
                return new Regex(rawImport, RegexOptions.IgnoreCase);
            }
        }
    }
}