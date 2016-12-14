// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A component used internally to tag a Scene.
    /// </summary>
    [DataContract("SceneSettings")]
    [Display(10000, "Scene", Expand = ExpandRule.Once)]
    public sealed class SceneSettings : ComponentBase, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneSettings"/> class.
        /// </summary>
        public SceneSettings()
        {
            Id = Guid.NewGuid();
        }

        [DataMember(10)]
        [Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; }
    }
}
