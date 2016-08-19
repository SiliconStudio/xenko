using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [AssetPartReference(typeof(Block), ReferenceType = typeof(BlockReference), KeepTypeInfo = false)]
    [AssetPartReference(typeof(Link))]
    public class VisualScriptAsset : AssetComposite, IProjectFileGeneratorAsset
    {
        public AssetPartCollection<Block> Blocks { get; } = new AssetPartCollection<Block>();

        public AssetPartCollection<Link> Links { get; } = new AssetPartCollection<Link>();

        #region IProjectFileGeneratorAsset implementation

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string AbsoluteSourceLocation { get; set; }

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string AbsoluteProjectLocation { get; set; }

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string ProjectInclude { get; set; }

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string ProjectName { get; set; }

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string Generator { get; } = "XenkoVisualScriptGenerator";

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string GeneratedAbsolutePath { get; set; }

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string GeneratedInclude { get; set; }

        #endregion

        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var block in Blocks)
                yield return new AssetPart(block.Id, block.BaseId, block.BasePartInstanceId);
            foreach (var link in Links)
                yield return new AssetPart(link.Id, link.BaseId, link.BasePartInstanceId);
        }

        public override bool ContainsPart(Guid id)
        {
            return Blocks.ContainsKey(id) || Links.ContainsKey(id);
        }

        public override void FixupPartReferences()
        {
            AssetCompositeAnalysis.FixupAssetPartReferences(this, ResolveReference);
        }

        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            Block block;
            if (Blocks.TryGetValue(id, out block))
            {
                block.BaseId = baseId;
                block.BasePartInstanceId = basePartInstanceId;
            }

            Link link;
            if (Links.TryGetValue(id, out link))
            {
                link.BaseId = baseId;
                link.BasePartInstanceId = basePartInstanceId;
            }
        }

        protected virtual object ResolveReference(object partReference)
        {
            var blockReference = partReference as Block;
            if (blockReference != null)
            {
                Block realPart;
                Blocks.TryGetValue(blockReference.Id, out realPart);
                return realPart;
            }

            var linkReference = partReference as Link;
            if (linkReference != null)
            {
                Link realPart;
                Links.TryGetValue(linkReference.Id, out realPart);
                return realPart;
            }

            return null;
        }

        public void SaveGeneratedAsset()
        {
            var compilerOptions = new VisualScriptCompilerOptions
            {
                Namespace = "Namespace",
                Class = Path.GetFileNameWithoutExtension(GeneratedAbsolutePath),
                BaseClass = "AsyncScript",
                UsingDirectives =
                {
                    typeof(SiliconStudio.Xenko.Engine.Entity).Namespace,
                },
            };
            var compilerResult = VisualScriptCompiler.Generate(this, compilerOptions);
            File.WriteAllText(GeneratedAbsolutePath, compilerResult.GeneratedSource);
        }
    }
}