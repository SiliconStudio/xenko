using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Assets.Navigation;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Assets.Input
{
    [DataContract("DefaultInputMapping")]
    [ObjectFactory(typeof(InputMappingAssetEntryFactory))]
    public class InputMappingAssetEntry
    {
        [Display("Mapping Name")]
        [DataMember(0)]
        public string MappingName;
        [DataMember(0)]
        public List<VirtualButton> DefaultMappings { get; set; }
    }

    public class InputMappingAssetEntryFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new InputMappingAssetEntry
            {
                DefaultMappings = new List<VirtualButton>()
            };
        }
    }

    [DataContract("InputMappingAsset")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true)]
    [Display("Input Mapping")]
    public class InputMappingAsset : Asset
    {
        public const string FileExtension = ".xkimap";

        [DataMember(0)]
        public List<InputMappingAssetEntry> DefaultBindings { get; set; }
    }
}