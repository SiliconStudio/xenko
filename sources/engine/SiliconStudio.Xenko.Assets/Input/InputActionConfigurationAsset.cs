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
    [AssetContentType(typeof(InputActionConfiguration))]
    [Display("Input Action Configuration")]
    public class InputActionConfigurationAsset : Asset
    {
        public const string FileExtension = ".xkinput";

        /// <summary>
        /// Lists all the actions in this configuration and the default bindings for them
        /// </summary>
        /// <remarks>Duplicate names for any of the actions are not allowed</remarks>
        [DataMember(0)]
        public List<InputAction> Actions { get; set; }
    }
}