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
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene() : this(new SceneSettings())
        {
        }

        public Scene(SceneSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets the settings of this scene.
        /// </summary>
        /// <value>The settings.</value>
        public SceneSettings Settings { get; }

        protected override void Destroy()
        {
            Settings.Dispose();

            base.Destroy();
        }

        public override string ToString()
        {
            return string.Format("Scene {0}", Name);
        }
    }
}
