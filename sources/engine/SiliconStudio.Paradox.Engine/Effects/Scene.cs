// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Scene as exported by third-party exporters (FBX, Assimp, etc...)
    /// </summary>
    //[ContentSerializer(typeof(DataContentConverterSerializer<Scene>))]
    public class Scene
    {
        public Model Model { get; set; }

        public AnimationClip Animation { get; set; }

        public List<LightComponent> Lights { get; set; }

        public List<CameraComponent> Cameras { get; set; }
    }
}