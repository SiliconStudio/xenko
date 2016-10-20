using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [AssetPartReference(typeof(Function), typeof(Block), typeof(Link))]
    [AssetPartReference(typeof(Block), typeof(Slot), ReferenceType = typeof(BlockReference), KeepTypeInfo = false)]
    [AssetPartReference(typeof(Link))]
    [AssetPartReference(typeof(Variable))]
    [AssetPartReference(typeof(Slot))]
    public class VisualScriptAsset : AssetComposite, IProjectFileGeneratorAsset
    {
        [DataMember(0)]
        [DefaultValue(Accessibility.Public)]
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        [DataMember(10)]
        [DefaultValue(false)]
        public bool IsStatic { get; set; }
        
        /// <summary>
        /// The list of member variables (properties and fields).
        /// </summary>
        [DataMember(20)]
        public TrackingCollection<Variable> Variables { get; } = new TrackingCollection<Variable>();

        /// <summary>
        /// The list of functions.
        /// </summary>
        [DataMember(30)]
        public TrackingCollection<Function> Functions { get; } = new TrackingCollection<Function>();

        #region IProjectFileGeneratorAsset implementation

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string Generator { get; } = "XenkoVisualScriptGenerator";

        #endregion

        /// <inheritdoc/>
        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var variable in Variables)
                yield return new AssetPart(variable.Id, variable.BaseId, variable.BasePartInstanceId);
            foreach (var function in Functions)
            {
                yield return new AssetPart(function.Id, function.BaseId, function.BasePartInstanceId);
                foreach (var block in function.Blocks)
                    yield return new AssetPart(block.Id, block.BaseId, block.BasePartInstanceId);
                foreach (var link in function.Links)
                    yield return new AssetPart(link.Id, link.BaseId, link.BasePartInstanceId);
            }
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid id)
        {
            foreach (var variable in Variables)
            {
                if (variable.Id == id)
                    return true;
            }

            foreach (var function in Functions)
            {
                if (function.Id == id)
                    return true;

                if (function.Blocks.ContainsKey(id) || function.Links.ContainsKey(id))
                    return true;

                foreach (var block in function.Blocks)
                {
                    foreach (var slot in block.Slots)
                    {
                        if (slot.Id == id)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            foreach (var variable in Variables)
            {
                if (variable.Id == id)
                {
                    variable.BaseId = baseId;
                    variable.BasePartInstanceId = basePartInstanceId;
                    return;
                }
            }

            foreach (var function in Functions)
            {
                if (function.Id == id)
                {
                    function.BaseId = baseId;
                    function.BasePartInstanceId = basePartInstanceId;
                    return;
                }

                Block block;
                if (function.Blocks.TryGetValue(id, out block))
                {
                    block.BaseId = baseId;
                    block.BasePartInstanceId = basePartInstanceId;
                    return;
                }

                Link link;
                if (function.Links.TryGetValue(id, out link))
                {
                    link.BaseId = baseId;
                    link.BasePartInstanceId = basePartInstanceId;
                    return;
                }
            }
        }

        /// <inheritdoc/>
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

            var functionReference = partReference as Function;
            if (functionReference != null)
            {
                foreach (var function in Functions)
                {
                    if (function.Id == functionReference.Id)
                    {
                        return function;
                    }
                }
                return null;
            }

            var blockReference = partReference as Block;
            if (blockReference != null)
            {
                foreach (var function in Functions)
                {
                    Block realPart;
                    if (function.Blocks.TryGetValue(blockReference.Id, out realPart))
                        return realPart;
                }
                return null;
            }

            var linkReference = partReference as Link;
            if (linkReference != null)
            {
                foreach (var function in Functions)
                {
                    Link realPart;
                    if (function.Links.TryGetValue(linkReference.Id, out realPart))
                        return realPart;
                }
                return null;
            }

            var slotReference = partReference as Slot;
            if (slotReference != null)
            {
                // TODO: store slot reference as Block Id + Slot Id for faster lookup?
                foreach (var function in Functions)
                {
                    foreach (var block in function.Blocks)
                    {
                        foreach (var slot in block.Slots)
                        {
                            if (slot.Id == slotReference.Id)
                                return slot;
                        }
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