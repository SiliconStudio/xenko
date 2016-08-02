using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Entities;

namespace SiliconStudio.Xenko.Assets.Model
{
    /// <summary>
    /// The geometric primitive asset.
    /// </summary>
    [DataContract("PrefabModelAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(PrefabModelAssetCompiler))]
    [Display(185, "Prefab Model")]
    public sealed class PrefabModelAsset : Asset, IModelAsset, IAssetCompileTimeDependencies
    {
        protected override int InternalBuildOrder => 600; //make sure we build after Models

        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefabmodel";

        [DataMember]
        public List<ModelMaterial> Materials { get; } = new List<ModelMaterial>();

        [DataMember]
        public AssetReference<PrefabAsset> Prefab { get; set; }

        public IEnumerable<IReference> EnumerateCompileTimeDependencies()
        {
            yield return Prefab;
        }
    }
}
