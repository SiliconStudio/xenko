using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Assets.Navigation;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Data;

namespace SiliconStudio.Xenko.Assets.Input
{
    [DataContract("InputMappingBinding")]
    [ObjectFactory(typeof(InputMappingBindingFactory))]
    public class InputMappingBinding
    {
        [DataMember(0)]
        public string MappingName;
        [DataMember(0)]
        public List<IVirtualButtonDesc> DefaultMappings { get; } = new List<IVirtualButtonDesc>();
    }

    public class InputMappingBindingFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new InputMappingBinding
            {
            };
        }
    }

    [DataContract("InputMappingAsset")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true)]
    [AssetCompiler(typeof(InputMappingAssetCompiler))]
    [Display("Input Mapping")]
    public class InputMappingAsset : Asset
    {
        public const string FileExtension = ".xkimap";

        [DataMember(0)]
        public string EnumType;

        [DataMember(0)]
        public List<InputMappingBinding> Bindings { get; } = new List<InputMappingBinding>();
    }
}