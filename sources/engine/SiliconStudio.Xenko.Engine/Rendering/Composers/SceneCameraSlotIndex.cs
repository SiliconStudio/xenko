// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Identifies a camera slotIndex in a scene composition.
    /// </summary>
    [DataContract("SceneCameraSlotIndex")]
    public class SceneCameraSlotIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlotIndex"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public SceneCameraSlotIndex(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlotIndex"/> class.
        /// </summary>
        public SceneCameraSlotIndex()
        {
        }

        /// <summary>
        /// Index of the camera in <see cref="SceneGraphicsCompositorLayers.Cameras"/>
        /// </summary>
        public int Index
        {
            get; set;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SceneCameraSlotIndex"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="slotIndex">The slotIndex.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator int(SceneCameraSlotIndex slotIndex)
        {
            return slotIndex.Index;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SceneCameraSlotIndex"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SceneCameraSlotIndex(int index)
        {
            return new SceneCameraSlotIndex(index);
        }

        public override string ToString()
        {
            return $"Cameras[{Index}]";
        }
    }
}