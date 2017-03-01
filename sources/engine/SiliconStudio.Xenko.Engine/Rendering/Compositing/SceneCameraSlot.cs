// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A camera slot used by <see cref="SceneGraphicsCompositorLayers"/>
    /// </summary>
    [DataContract("SceneCameraSlot")]
    public sealed class SceneCameraSlot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlot"/> class.
        /// </summary>
        public SceneCameraSlot()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlot"/> class.
        /// </summary>
        /// <param name="camera">The camera.</param>
        public SceneCameraSlot(CameraComponent camera)
        {
            Camera = camera;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        public string Name { get; set; }

        [DataMemberIgnore]
        public CameraComponent Camera { get; set; }

        public override string ToString()
        {
            string name = Name;
            if (name == null && Camera?.Entity != null)
            {
                name = Camera.Entity.Name;
            }

            return $"Camera [{name}]";
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="CameraComponent"/> to <see cref="SceneCameraSlot"/>.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SceneCameraSlot(CameraComponent camera)
        {
            return new SceneCameraSlot(camera);
        }
    }
}