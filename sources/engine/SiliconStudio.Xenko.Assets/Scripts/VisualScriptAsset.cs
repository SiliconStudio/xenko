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
    [AssetPartReference(typeof(Method), typeof(Block), typeof(Link), typeof(Symbol))]
    [AssetPartReference(typeof(Block), typeof(Slot), ReferenceType = typeof(BlockReference), KeepTypeInfo = false)]
    [AssetPartReference(typeof(Link))]
    [AssetPartReference(typeof(Symbol))]
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
        public TrackingCollection<Property> Properties { get; } = new TrackingCollection<Property>();

        /// <summary>
        /// The list of functions.
        /// </summary>
        [DataMember(30)]
        public TrackingCollection<Method> Methods { get; } = new TrackingCollection<Method>();

        #region IProjectFileGeneratorAsset implementation

        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string Generator { get; } = "XenkoVisualScriptGenerator";

        #endregion

        /// <inheritdoc/>
        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var member in Properties)
                yield return new AssetPart(member.Id, member.BaseId, member.BasePartInstanceId);
            foreach (var function in Methods)
            {
                yield return new AssetPart(function.Id, function.BaseId, function.BasePartInstanceId);
                foreach (var parmeter in function.Parameters)
                    yield return new AssetPart(parmeter.Id, parmeter.BaseId, parmeter.BasePartInstanceId);
                foreach (var block in function.Blocks)
                    yield return new AssetPart(block.Id, block.BaseId, block.BasePartInstanceId);
                foreach (var link in function.Links)
                    yield return new AssetPart(link.Id, link.BaseId, link.BasePartInstanceId);
            }
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid id)
        {
            foreach (var variable in Properties)
            {
                if (variable.Id == id)
                    return true;
            }

            foreach (var method in Methods)
            {
                if (method.Id == id)
                    return true;

                if (method.Blocks.ContainsKey(id) || method.Links.ContainsKey(id))
                    return true;

                foreach (var parameter in method.Parameters)
                {
                    if (parameter.Id == id)
                        return true;
                }

                foreach (var block in method.Blocks)
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
            foreach (var property in Properties)
            {
                if (property.Id == id)
                {
                    property.BaseId = baseId;
                    property.BasePartInstanceId = basePartInstanceId;
                    return;
                }
            }

            foreach (var function in Methods)
            {
                if (function.Id == id)
                {
                    function.BaseId = baseId;
                    function.BasePartInstanceId = basePartInstanceId;
                    return;
                }

                foreach (var parameter in function.Parameters)
                {
                    if (parameter.Id == id)
                    {
                        parameter.BaseId = baseId;
                        parameter.BasePartInstanceId = basePartInstanceId;
                        return;
                    }
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
            var propertyReference = partReference as Property;
            if (propertyReference != null)
            {
                foreach (var property in Properties)
                {
                    if (property.Id == propertyReference.Id)
                    {
                        return property;
                    }
                }
                return null;
            }

            var parameterReference = partReference as Parameter;
            if (parameterReference != null)
            {
                foreach (var method in Methods)
                {
                    foreach (var parameter in method.Parameters)
                    {
                        if (parameter.Id == parameterReference.Id)
                        {
                            return method;
                        }
                    }
                }
                return null;
            }

            var methodReference = partReference as Method;
            if (methodReference != null)
            {
                foreach (var method in Methods)
                {
                    if (method.Id == methodReference.Id)
                    {
                        return method;
                    }
                }
                return null;
            }

            var blockReference = partReference as Block;
            if (blockReference != null)
            {
                foreach (var function in Methods)
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
                foreach (var function in Methods)
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
                foreach (var function in Methods)
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