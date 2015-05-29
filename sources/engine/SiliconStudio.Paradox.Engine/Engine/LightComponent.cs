// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Rendering.Lights;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a light to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("LightComponent")]
    [Display(120, "Light")]
    [DefaultEntityComponentRenderer(typeof(LightComponentRenderer), -10)]
    [DefaultEntityComponentProcessor(typeof(LightProcessor))]
    public sealed class LightComponent : EntityComponent
    {
        public static PropertyKey<LightComponent> Key = new PropertyKey<LightComponent>("Key", typeof(LightComponent));

        /// <summary>
        /// The default direction of a light vector is (x,y,z) = (0,0,-1)
        /// </summary>
        public static readonly Vector3 DefaultDirection = new Vector3(0, 0, -1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponent"/> class.
        /// </summary>
        public LightComponent()
        {
            Type = new LightDirectional();
            Intensity = 1.0f;
            CullingMask = EntityGroupMask.All;
        }

        /// <summary>
        /// Gets or sets the type of the light.
        /// </summary>
        /// <value>The type of the light.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Light", AlwaysExpand = true)]
        public ILight Type { get; set; }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        /// <value>The light intensity.</value>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        public float Intensity { get; set; }

        /// <summary>
        /// Get or sets the layers that the light influences
        /// </summary>
        /// <value>
        /// The layer mask.
        /// </value>
        [DataMember(40)]
        [DefaultValue(EntityGroupMask.All)]
        public EntityGroupMask CullingMask { get; set; }

        /// <summary>
        /// Gets the light position in World-Space (computed by the <see cref="LightProcessor"/>) (readonly field). See remarks.
        /// </summary>
        /// <value>The position.</value>
        /// <remarks>This property should only be used inside a renderer and not from a script as it is updated after scripts</remarks>
        [DataMemberIgnore]
        public Vector3 Position;

        /// <summary>
        /// Gets the light direction in World-Space (computed by the <see cref="LightProcessor"/>) (readonly field).
        /// </summary>
        /// <value>The direction.</value>
        /// <remarks>This property should only be used inside a renderer and not from a script as it is updated after scripts</remarks>
        [DataMemberIgnore]
        public Vector3 Direction;

        [DataMemberIgnore]
        public Color3 Color;

        /// <summary>
        /// The bounding box of this light in WS after the <see cref="LightProcessor"/> has been applied (readonly field).
        /// </summary>
        [DataMemberIgnore]
        public BoundingBox BoundingBox;

        /// <summary>
        /// The bounding box extents of this light in WS after the <see cref="LightProcessor"/> has been applied (readonly field).
        /// </summary>
        [DataMemberIgnore]
        public BoundingBoxExt BoundingBoxExt;

        /// <summary>
        /// The determines whether this instance has a valid bounding box (readonly field).
        /// </summary>
        [DataMemberIgnore]
        public bool HasBoundingBox;

        /// <summary>
        /// Updates this instance( <see cref="Position"/>, <see cref="Direction"/>, <see cref="HasBoundingBox"/>, <see cref="BoundingBox"/>, <see cref="BoundingBoxExt"/>
        /// </summary>
        public bool Update()
        {
            if (Type == null || !Enabled || !Type.Update(this))
            {
                return false;
            }

            // Compute light direction and position
            Vector3 lightDirection;
            var lightDir = DefaultDirection;
            Vector3.TransformNormal(ref lightDir, ref Entity.Transform.WorldMatrix, out lightDirection);
            lightDirection.Normalize();

            Position = Entity.Transform.WorldMatrix.TranslationVector;
            Direction = lightDirection;

            // Color
            var colorLight = Type as IColorLight;
            Color = (colorLight != null) ? colorLight.ComputeColor(Intensity) : new Color3();

            // Compute bounding boxes
            HasBoundingBox = false;
            BoundingBox = new BoundingBox();
            BoundingBoxExt = new BoundingBoxExt();

            var directLight = Type as IDirectLight;
            if (directLight != null && directLight.HasBoundingBox)
            {
                // Computes the bounding boxes
                BoundingBox = directLight.ComputeBounds(Position, Direction);
                BoundingBoxExt = new BoundingBoxExt(BoundingBox);
            }

            return true;
        }


        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}