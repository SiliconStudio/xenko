using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Assets.Scripts
{
    public interface IScriptTypesProvider
    {
        IReadOnlyCollection<Type> GetSourcePathTypes(string path);
    }

    [DataContract("ScriptSourceFileAsset")]
    [AssetDescription(".cs")]
    [Display(95, "Script Source Code", "A C# source code file")]
    public sealed class ScriptSourceFileAsset : SourceCodeAsset
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
