// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
