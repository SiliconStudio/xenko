// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private readonly IPackageCompilerSource packageCompilerSource;

        static PackageCompiler()
        {
            // Compute XenkoSdkDir from this assembly
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

        public PackageCompiler(IPackageCompilerSource packageCompilerSource)
        {
            this.packageCompilerSource = packageCompilerSource;
            compilers = new List<IPackageCompiler>();
        }

        /// <summary>
        /// Gets or sets the SDK directory. Default is bound to env variable XenkoSdkDir
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

            var result = new AssetCompilerResult();

            var assets = packageCompilerSource.GetAssets(result).ToList();
            if (result.HasErrors)
            {
                return result;
            }

            var defaultAssetsCompiler = new DefaultAssetsCompiler(assets);
            defaultAssetsCompiler.AssetCompiled += OnAssetCompiled;

            // Add default compilers
            compilers.Clear();
            compilers.Add(defaultAssetsCompiler);

            // Compile using all PackageCompiler
            foreach (var compiler in compilers)
            {
                var compilerResult = compiler.Compile(compilerContext);
                compilerResult.CopyTo(result);
                while (compilerResult.BuildSteps.Count > 0)
                {
                    var step = compilerResult.BuildSteps[0];
                    compilerResult.BuildSteps.RemoveAt(0);
                    result.BuildSteps.Add(step);
                }
            }

            return result;
        }

        private void OnAssetCompiled(object sender, AssetCompiledArgs assetCompiledArgs)
        {
            AssetCompiled?.Invoke(this, assetCompiledArgs);
        }

        /// <summary>
        /// Internal default compiler for compiling all assets 
        /// </summary>
        private class DefaultAssetsCompiler : ItemListCompiler, IPackageCompiler
        {
            private readonly IList<AssetItem> assets;

            public DefaultAssetsCompiler(IList<AssetItem> assets)
                : base(assetCompilerRegistry)
            {
                this.assets = assets;
            }

            public AssetCompilerResult Compile(AssetCompilerContext context)
            {
                var result = new AssetCompilerResult();

                // generate the build steps required to build the assets via base class
                Prepare(context, assets, result);

                return result;
            }
        }
    }
}