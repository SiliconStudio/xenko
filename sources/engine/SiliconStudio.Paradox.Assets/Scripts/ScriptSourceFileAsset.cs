using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Scripts
{
    public interface IScriptTypesProvider
    {
        IReadOnlyCollection<Type> GetSourcePathTypes(string path);

        string GetNamespaceForProject(string projectName);
    }

    [DataContract("ScriptSourceFileAsset")]
    [AssetDescription(".cs", AlwaysMarkAsRoot = true)]
    [ThumbnailCompiler(PreviewerCompilerNames.ScriptSourceFileThumbnailCompilerQualifiedName)]
    [Display(95, "Script Source Code", "A C# source code file")]
    public sealed class ScriptSourceFileAsset : ProjectSourceCodeAsset
    {
        public IReadOnlyCollection<Type> GetContainedScriptTypes()
        {
            lock(ProviderLocker)
            {
                return provider?.GetSourcePathTypes(AbsoluteSourceLocation);
            }
        }

        private static readonly object ProviderLocker = new object();

        private static IScriptTypesProvider provider;

        public static void SetProvider(IScriptTypesProvider p)
        {
            lock(ProviderLocker)
            {
                provider = p;
            }
        }
    }
}
