// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Scene as exported by third-party exporters (FBX, Assimp, etc...)
    /// </summary>
    [DataConverter(AutoGenerate = true, ContentReference = true)]
    [ContentSerializer(typeof(DataContentConverterSerializer<Scene>))]
    public class Scene
    {
        [DataMemberConvert]
        public Model Model { get; set; }

        [DataMemberConvert]
        public AnimationClip Animation { get; set; }

        [DataMemberConvert]
        public List<LightComponent> Lights { get; set; }

        [DataMemberConvert]
        public List<CameraComponent> Cameras { get; set; }
    }
}