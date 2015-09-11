using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.SpriteStudio.Runtime;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract("SpriteStudioAnimationAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioAnimationAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxss4a", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioAnimationAssetFactory))]
    [Display("Sprite Studio Animation")]
    public class SpriteStudioAnimationAsset : AssetImportTracked
    {
        public SpriteStudioAnimationAsset()
        {
            NodesData = new List<SpriteNodeData>();
            Nodes = new List<SpriteStudioNode>();
        }

        [DataMember(3)]
        public List<SpriteNodeData> NodesData { get; private set; }

        [DataMember(1)]
        public AnimationRepeatMode RepeatMode { get; set; } = AnimationRepeatMode.LoopInfinite;

        [DataMember(2)]
        public List<SpriteStudioNode> Nodes { get; private set; }

        [DataMember(4)]
        public int EndFrame;

        [DataMember(5)]
        public int Fps;

        private class SpriteStudioAnimationAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioAnimationAsset();
            }
        }
    }
}