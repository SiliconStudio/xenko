using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public interface IScriptTypesProvider
    {
        IReadOnlyCollection<Type> GetSourcePathTypes(string path);

        string GetNamespaceForProject(string projectName);
    }

    [DataContract("ScriptSourceFileAsset")]
    [AssetDescription(Extension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [Display(95, "Script Source Code")]
    public sealed class ScriptSourceFileAsset : ProjectSourceCodeAsset
    {
        public const string Extension = ".cs";

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
