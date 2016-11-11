// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A configuration that contains all actions that were configured on an InputActionMappingAsset in the GameStudio together with the default action bindings
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<InputActionConfiguration>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<InputActionConfiguration>), Profile = "Content")]
    public class InputActionConfiguration
    {
        /// <summary>
        /// Lists all the actions that this input mapping contains
        /// </summary>
        public List<InputAction> Actions { get; set; }
    }
}