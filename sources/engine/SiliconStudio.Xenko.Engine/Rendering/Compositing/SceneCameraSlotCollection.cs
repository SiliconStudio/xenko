// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A collection of <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("SceneCameraSlotCollection")]
    public sealed class SceneCameraSlotCollection : FastTrackingCollection<SceneCameraSlot>
    {
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneCameraSlotCollection> Current = new PropertyKey<SceneCameraSlotCollection>("SceneCameraSlotCollection.Current", typeof(SceneCameraSlotCollection));
    }
}