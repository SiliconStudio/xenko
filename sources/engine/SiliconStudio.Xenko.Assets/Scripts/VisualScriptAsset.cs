using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [AssetPartReference(typeof(Block), typeof(Slot), ReferenceType = typeof(BlockReference), KeepTypeInfo = false)]
    [AssetPartReference(typeof(Link))]
    [AssetPartReference(typeof(Variable))]
    [AssetPartReference(typeof(Slot))]
    public class VisualScriptAsset : AssetComposite, IProjectFileGeneratorAsset
    {
        [DataMember(0)]
        public TrackingCollection<Variable> Variables { get; } = new TrackingCollection<Variable>();

        [DataMember(10)]
        public AssetPartCollection<Block> Blocks { get; } = new AssetPartCollection<Block>();

        [DataMember(20)]
        public AssetPartCollection<Link> Links { get; } = new AssetPartCollection<Link>();

        #region IProjectFileGeneratorAsset implementation

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string Generator { get; } = "XenkoVisualScriptGenerator";

        #endregion

        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var variable in Variables)
                yield return new AssetPart(variable.Id, variable.BaseId, variable.BasePartInstanceId);
            foreach (var block in Blocks)
                yield return new AssetPart(block.Id, block.BaseId, block.BasePartInstanceId);
            foreach (var link in Links)
                yield return new AssetPart(link.Id, link.BaseId, link.BasePartInstanceId);
        }

        public override bool ContainsPart(Guid id)
        {
            return Blocks.ContainsKey(id) || Links.ContainsKey(id);
        }

        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            foreach (var variable in Variables)
            {
                if (variable.Id == id)
                {
                    variable.BaseId = baseId;
                    variable.BasePartInstanceId = basePartInstanceId;
                    break;
                }
            }

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

        protected override object ResolvePartReference(object partReference)
        {
            var variableReference = partReference as Variable;
            if (variableReference != null)
            {
                foreach (var variable in Variables)
                {
                    if (variable.Id == variableReference.Id)
                    {
                        return variable;
                    }
                }
                return null;
            }

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

            var slotReference = partReference as Slot;
            if (slotReference != null)
            {
                // TODO: store slot reference as Block Id + Slot Id for faster lookup
                foreach (var block in Blocks)
                {
                    foreach (var slot in block.Slots)
                    {
                        if (slot.Id == slotReference.Id)
                            return slot;
                    }
                }

                return null;
            }

            return null;
        }

        public void SaveGeneratedAsset(AssetItem assetItem)
        {
            var generatedAbsolutePath = assetItem.GetGeneratedAbsolutePath();

            var compilerResult = Compile(assetItem);
            File.WriteAllText(generatedAbsolutePath, compilerResult.GeneratedSource);
        }

        public VisualScriptCompilerResult Compile(AssetItem assetItem)
        {
            var generatedAbsolutePath = assetItem.GetGeneratedAbsolutePath();

            var compilerOptions = new VisualScriptCompilerOptions
            {
                Namespace = "Namespace",
                Class = Path.GetFileNameWithoutExtension(generatedAbsolutePath),
                BaseClass = "AsyncScript",
                UsingDirectives =
                {
                    typeof(SiliconStudio.Xenko.Engine.Entity).Namespace,
                },
            };
            var compilerResult = VisualScriptCompiler.Generate(this, compilerOptions);
            return compilerResult;
        }
    }
}