// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A scene.
    /// </summary>
    [DataContract("Scene")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Scene>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Scene>), Profile = "Asset")]
    public sealed class Scene : ComponentBase
    {
        private SceneSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene() : this(new SceneSettings())
        {
        }

        public Scene(SceneSettings settings)
        {
            Entities = new TrackingCollection<Entity>();
            this.settings = settings;
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public TrackingCollection<Entity> Entities { get; private set; }

        /// <summary>
        /// Gets the settings of this scene.
        /// </summary>
        /// <value>The settings.</value>
        public SceneSettings Settings { get { return settings; } }

        // Note: Added for compatibility with previous code
        [Obsolete]
        public void AddChild(Entity entity)
        {
            Entities.Add(entity);
        }

        [Obsolete]
        public void RemoveChild(Entity entity)
        {
            Entities.Remove(entity);
        }

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
