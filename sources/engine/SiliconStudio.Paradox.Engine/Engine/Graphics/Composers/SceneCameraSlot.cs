// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Identifies a camera slot in a scene composition.
    /// </summary>
    [DataContract("SceneCameraSlot")]
    public struct SceneCameraSlot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlot"/> struct.
        /// </summary>
        /// <param name="index">The index.</param>
        public SceneCameraSlot(int index)
            : this()
        {
            Index = index;
        }

        /// <summary>
        /// Index of the camera in <see cref="SceneGraphicsCompositorLayers.Cameras"/>
        /// </summary>
        public int Index
        {
            get; set;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SiliconStudio.Paradox.Engine.Graphics.Composers.SceneCameraSlot"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator int(SceneCameraSlot slot)
        {
            return slot.Index;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SiliconStudio.Paradox.Engine.Graphics.Composers.SceneCameraSlot"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SceneCameraSlot(int index)
        {
            return new SceneCameraSlot(index);
        }

        public override string ToString()
        {
            return string.Format("Cameras[{0}]", Index);
        }
    }
}