// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Identifies a camera slotIndex in a scene composition.
    /// </summary>
    [DataContract("SceneCameraSlotId")]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(SceneCameraSlotIdDataSerializer))]
    public struct SceneCameraSlotId : IEquatable<SceneCameraSlotId>
    {
        /// <summary>
        /// The Guid matching a <see cref="SceneCameraSlot"/>.
        /// </summary>
        public readonly Guid Id;

        /// <summary>
        /// The <see cref="GraphicsCompositor"/> on which the camera containing this <see cref="SceneCameraSlotId"/> is currently attached.
        /// </summary>
        internal GraphicsCompositor AttachedCompositor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraSlotId"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public SceneCameraSlotId(Guid id)
        {
            Id = id;
            AttachedCompositor = null;
        }

        /// <summary>
        /// Gets whether this <see cref="SceneCameraSlotId"/> is empty.
        /// </summary>
        public bool IsEmpty => Id == Guid.Empty;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Cameras[{Id}]";
        }

        /// <inheritdoc/>
        public bool Equals(SceneCameraSlotId other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SceneCameraSlotId && Equals((SceneCameraSlotId)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(SceneCameraSlotId left, SceneCameraSlotId right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(SceneCameraSlotId left, SceneCameraSlotId right)
        {
            return !left.Equals(right);
        }

        public sealed class SceneCameraSlotIdDataSerializer : DataSerializer<SceneCameraSlotId>
        {
            /// <inheritdoc/>
            public override void Serialize(ref SceneCameraSlotId assetReference, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(assetReference.Id);
                }
                else
                {
                    var id = stream.Read<Guid>();
                    assetReference = new SceneCameraSlotId(id);
                }
            }
        }
    }
}
