// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A scene.
    /// </summary>
    [DataContract("Scene")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Scene>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Scene>), Profile = "Content")]
    public sealed class Scene : PrefabBase
    {
        public override string ToString()
        {
            return $"Scene {Name}";
        }
    }
}
