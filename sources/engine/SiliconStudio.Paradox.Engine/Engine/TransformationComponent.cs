// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Defines Position, Rotation and Scale of its <see cref="Entity"/>.
    /// </summary>
    [DataContract("TransformationComponent")]
    [DataSerializerGlobal(null, typeof(TrackingCollection<TransformationComponent>))]
    [Display(10, "Transform")]
    public sealed class TransformationComponent : EntityComponent //, IEnumerable<TransformationComponent> Check why this is not working
    {
        public static PropertyKey<TransformationComponent> Key = new PropertyKey<TransformationComponent>("Key", typeof(TransformationComponent),
            new AccessorMetadata((ref PropertyContainer props) => ((Entity)props.Owner).Transform, (ref PropertyContainer props, object value) => ((Entity)props.Owner).Transform = (TransformationComponent)value));

        // When false, transformation should be computed in TransformationProcessor (no dependencies).
        // When true, transformation is computed later by another system.
        // This is useful for scenario such as binding a node to a bone, where it first need to run TransformationProcessor for the hierarchy,
        // run MeshProcessor to update ModelViewHierarchy, copy Node/Bone transformation to another Entity with special root and then update its children transformations.
        internal bool isSpecialRoot = false;
        private bool useTRS = true;
        private TransformationComponent parent;

        /// <summary>
        /// The world matrix.
        /// Use <see cref="UpdateWorldMatrix"/> to ensure it is updated.
        /// </summary>
        [DataMemberIgnore]
        public Matrix WorldMatrix = Matrix.Identity;

        /// <summary>
        /// The local matrix.
        /// Use <see cref="UpdateLocalMatrix"/> to ensure it is updated.
        /// </summary>
        [DataMemberIgnore]
        public Matrix LocalMatrix = Matrix.Identity;

        /// <summary>
        /// The translation relative to the parent transformation.
        /// </summary>
        public Vector3 Translation;

        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// The scaling relative to the parent transformation.
        /// </summary>
        public Vector3 Scaling;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationComponent" /> class.
        /// </summary>
        public TransformationComponent()
        {
            var children = new TrackingCollection<TransformationComponent>();
            children.CollectionChanged += ChildrenCollectionChanged;

            Children = children;

            UseTRS = true;
            Scaling = Vector3.One;
            Rotation = Quaternion.Identity;
        }

        public bool UseTRS
        {
            get { return useTRS; }
            set { useTRS = value; }
        }
        
        /// <summary>
        /// Gets the children of this <see cref="TransformationComponent"/>.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public FastCollection<TransformationComponent> Children { get; private set; }

        /// <summary>
        /// Gets or sets the euler rotation, with XYZ order.
        /// Not stable: setting value and getting it again might return different value as it is internally encoded as a <see cref="Quaternion"/> in <see cref="Rotation"/>.
        /// </summary>
        /// <value>
        /// The euler rotation.
        /// </value>
        [DataMemberIgnore]
        public Vector3 RotationEulerXYZ
        {
            get
            {
                var rotation = Rotation;
                Vector3 rotationEuler;

                // Equivalent to:
                //  Matrix rotationMatrix;
                //  Matrix.Rotation(ref cachedRotation, out rotationMatrix);
                //  rotationMatrix.DecomposeXYZ(out rotationEuler);

                float xx = rotation.X * rotation.X;
                float yy = rotation.Y * rotation.Y;
                float zz = rotation.Z * rotation.Z;
                float xy = rotation.X * rotation.Y;
                float zw = rotation.Z * rotation.W;
                float zx = rotation.Z * rotation.X;
                float yw = rotation.Y * rotation.W;
                float yz = rotation.Y * rotation.Z;
                float xw = rotation.X * rotation.W;

                rotationEuler.Y = (float)Math.Asin(2.0f * (yw - zx));
                double test = Math.Cos(rotationEuler.Y);
                if (test > 1e-6f)
                {
                    rotationEuler.Z = (float)Math.Atan2(2.0f * (xy + zw), 1.0f - (2.0f * (yy + zz)));
                    rotationEuler.X = (float)Math.Atan2(2.0f * (yz + xw), 1.0f - (2.0f * (yy + xx)));
                }
                else
                {
                    rotationEuler.Z = (float)Math.Atan2(2.0f * (zw - xy), 2.0f * (zx + yw));
                    rotationEuler.X = 0.0f;
                }
                return rotationEuler;
            }
            set
            {
                // Equilvalent to:
                //  Quaternion quatX, quatY, quatZ;
                //  
                //  Quaternion.RotationX(value.X, out quatX);
                //  Quaternion.RotationY(value.Y, out quatY);
                //  Quaternion.RotationZ(value.Z, out quatZ);
                //  
                //  rotation = quatX * quatY * quatZ;

                var halfAngles = value * 0.5f;

                var fSinX = (float)Math.Sin(halfAngles.X);
                var fCosX = (float)Math.Cos(halfAngles.X);
                var fSinY = (float)Math.Sin(halfAngles.Y);
                var fCosY = (float)Math.Cos(halfAngles.Y);
                var fSinZ = (float)Math.Sin(halfAngles.Z);
                var fCosZ = (float)Math.Cos(halfAngles.Z);

                var fCosXY = fCosX * fCosY;
                var fSinXY = fSinX * fSinY;

                Rotation.X = fSinX * fCosY * fCosZ - fSinZ * fSinY * fCosX;
                Rotation.Y = fSinY * fCosX * fCosZ + fSinZ * fSinX * fCosY;
                Rotation.Z = fSinZ * fCosXY - fSinXY * fCosZ;
                Rotation.W = fCosZ * fCosXY + fSinXY * fSinZ;
            }
        }

        /// <summary>
        /// Gets or sets the parent of this <see cref="TransformationComponent"/>.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        [DataMemberIgnore]
        public TransformationComponent Parent
        {
            get { return parent; }
            set
            {
                var oldParent = Parent;
                if (oldParent == value)
                    return;

                if (oldParent != null)
                    oldParent.Children.Remove(this);
                if (value != null)
                    value.Children.Add(this);
            }
        }

        /// <summary>
        /// Updates the local matrix.
        /// If <see cref="UseTRS"/> is true, <see cref="LocalMatrix"/> will be updated from <see cref="Translation"/>, <see cref="Rotation"/> and <see cref="Scaling"/>.
        /// </summary>
        public void UpdateLocalMatrix()
        {
            if (UseTRS)
            {
                CreateMatrixTRS(ref Translation, ref Rotation, ref Scaling, out LocalMatrix);
            }
        }

        /// <summary>
        /// Updates the world matrix.
        /// It will first call <see cref="UpdateLocalMatrix"/> on self, and <see cref="UpdateWorldMatrix"/> on <see cref="Parent"/> if not null.
        /// Then <see cref="WorldMatrix"/> will be updated by multiplying <see cref="LocalMatrix"/> and parent <see cref="WorldMatrix"/> (if any).
        /// </summary>
        public void UpdateWorldMatrix()
        {
            UpdateLocalMatrix();

            if (Parent != null && !isSpecialRoot)
            {
                Parent.UpdateWorldMatrix();
                Matrix.Multiply(ref LocalMatrix, ref Parent.WorldMatrix, out WorldMatrix);
            }
            else
            {
                WorldMatrix = LocalMatrix;
            }
        }

        internal void UpdateWorldMatrixNonRecursive()
        {
            if (Parent != null && !isSpecialRoot)
            {
                Matrix.Multiply(ref LocalMatrix, ref Parent.WorldMatrix, out WorldMatrix);
            }
            else
            {
                WorldMatrix = LocalMatrix;
            }
        }

        private void AddItem(TransformationComponent item)
        {
            if (item.Parent != null)
                throw new InvalidOperationException("This TransformationComponent already has a Parent, detach it first.");

            item.parent = this;
        }

        private void RemoveItem(TransformationComponent item)
        {
            if (item.Parent != this)
                throw new InvalidOperationException("This TransformationComponent's parent is not the expected value.");

            item.parent = null;
        }

        private void ChildrenCollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItem((TransformationComponent)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItem((TransformationComponent)e.Item);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a matrix that contains both the X, Y and Z rotation, as well as scaling and translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="rotation">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="scaling">The scaling.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void CreateMatrixTRS(ref Vector3 translation, ref Quaternion rotation, ref Vector3 scaling, out Matrix result)
        {
            // Equivalent to:
            //result =
            //    Matrix.Scaling(scaling)
            //    *Matrix.RotationX(rotation.X)
            //    *Matrix.RotationY(rotation.Y)
            //    *Matrix.RotationZ(rotation.Z)
            //    *Matrix.Translation(translation);

            // Rotation
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));

            // Translation
            result.M41 = translation.X;
            result.M42 = translation.Y;
            result.M43 = translation.Z;

            // Scaling
            if (scaling.X != 1.0f)
            {
                result.M11 *= scaling.X;
                result.M12 *= scaling.X;
                result.M13 *= scaling.X;
            }
            if (scaling.Y != 1.0f)
            {
                result.M21 *= scaling.Y;
                result.M22 *= scaling.Y;
                result.M23 *= scaling.Y;
            }
            if (scaling.Z != 1.0f)
            {
                result.M31 *= scaling.Z;
                result.M32 *= scaling.Z;
                result.M33 *= scaling.Z;
            }

            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = 1.0f;
        }

        protected internal override PropertyKey DefaultKey
        {
            get { return Key; }
        }

        private static readonly Type[] DefaultProcessors = new Type[] { typeof(HierarchicalProcessor), typeof(TransformationProcessor),  };
        protected internal override IEnumerable<Type> GetDefaultProcessors()
        {
            return DefaultProcessors;
        }

        //public IEnumerator<TransformationComponent> GetEnumerator()
        //{
        //    return Children.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return ((IEnumerable)Children).GetEnumerator();
        //}
    }
}