// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A camera slot used by contained in a <see cref="SceneCameraSlotCollection"/> and referenceable by a <see cref="SceneCameraSlotId"/>
    /// </summary>
    [DataContract("SceneCameraSlot")]
    public sealed class SceneCameraSlot : IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlot"/> class.
        /// </summary>
        public SceneCameraSlot()
        {
            Id = Guid.NewGuid();
        }

        [NonOverridable]
        [DataMember(5)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        public string Name { get; set; } = "CameraSlot";

        [DataMemberIgnore]
        public CameraComponent Camera { get; internal set; }

        /// <summary>
        /// Generates a <see cref="SceneCameraSlotId"/> corresponding to this slot.
        /// </summary>
        /// <returns>A new instance of <see cref="SceneCameraSlotId"/>.</returns>
        public SceneCameraSlotId ToSlotId()
        {
            return new SceneCameraSlotId(Id);
        }

        public override string ToString()
        {
            string name = Name;
            if (name == null && Camera?.Entity != null)
            {
                name = Camera.Entity.Name;
            }

            return $"Camera [{name}]";
        }
    }
}
