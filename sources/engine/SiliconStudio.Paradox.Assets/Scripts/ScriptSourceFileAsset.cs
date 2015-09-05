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

    [DataContract("EffectCompositorAsset")]
    [AssetDescription(".cs")]
    [Display(95, "Script Source Code", "A C# source code file")]
    public sealed class ScriptSourceFileAsset : SourceCodeAsset
    {
        public ScriptSourceFileAsset()
        {
        }

        public IReadOnlyCollection<Type> GetContainedScriptTypes()
        {
            lock(providerLocker)
            {
                if(provider != null)
                {
                    return provider.GetSourcePathTypes(AbsoluteSourceLocation);
                }
                else
                {
                    return null;
                }
            }
        }

        private static object providerLocker = new object();

        private static IScriptTypesProvider provider;

        public static void SetProvider(IScriptTypesProvider p)
        {
            lock(providerLocker)
            {
                provider = p;
            }
        }
    }
}
