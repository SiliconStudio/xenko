// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// A scene.
    /// </summary>
    [DataContract("Scene")]
    public sealed class Scene : Entity
    {
        static Scene()
        {
            PropertyContainer.AddAccessorProperty(typeof(Scene), SceneComponent.Key);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene()
        {
            // By default a scene always have a SceneComponent
            Add(new SceneComponent());
        }

        /// <summary>
        /// Gets the settings of this scene.
        /// </summary>
        /// <value>The settings.</value>
        [DataMemberIgnore]
        public SceneComponent Settings { get; internal set; }

        protected override void Destroy()
        {
            Settings.Dispose();

            base.Destroy();
        }
    }
}