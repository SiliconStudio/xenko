// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Assets.Input
{
    [DataContract]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true)]
    [Display("Input Mapping")]
    public class InputMappingAsset : Asset
    {
        public const string FileExtension = ".xkimap";

        /// <summary>
        /// Lists all the actions that this input mapping wil define
        /// </summary>
        [DataMember(0)]
        public List<InputAction> Actions { get; set; }
    }
}