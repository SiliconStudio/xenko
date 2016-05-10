// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class RotationEditor : VectorEditorBase<Quaternion>
    {
        private Vector3 decomposedRotation;

        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// The X component (in Cartesian coordinate system) of the <see cref="Vector3"/> associated to this control.
        /// </summary>
        public float X { get { return (float)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector3"/> associated to this control.
        /// </summary>
        public float Y { get { return (float)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector3"/> associated to this control.
        /// </summary>
        public float Z { get { return (float)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        public override void ResetValue()
        {
            Value = Quaternion.Identity;
        }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Quaternion value)
        {
            // This allows iterating on the euler angles when resulting rotation are equivalent (see PDX-1779).
            var current = Recompose(ref decomposedRotation);
            if (current == value)
                return;

            var rotationMatrix = Matrix.RotationQuaternion(value);
            rotationMatrix.Decompose(out decomposedRotation.Y, out decomposedRotation.X, out decomposedRotation.Z);
            SetCurrentValue(XProperty, MathUtil.RadiansToDegrees(decomposedRotation.X));
            SetCurrentValue(YProperty, MathUtil.RadiansToDegrees(decomposedRotation.Y));
            SetCurrentValue(ZProperty, MathUtil.RadiansToDegrees(decomposedRotation.Z));
        }

        /// <inheritdoc/>
        protected override Quaternion UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == XProperty)
                decomposedRotation = new Vector3(MathUtil.DegreesToRadians(X), decomposedRotation.Y, decomposedRotation.Z);
            else if (property == YProperty)
                decomposedRotation = new Vector3(decomposedRotation.X, MathUtil.DegreesToRadians(Y), decomposedRotation.Z);
            else if (property == ZProperty)
                decomposedRotation = new Vector3(decomposedRotation.X, decomposedRotation.Y, MathUtil.DegreesToRadians(Z));
            else
                throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
            
            return Recompose(ref decomposedRotation);
        }

        /// <inheritdoc/>
        protected override Quaternion UpateValueFromFloat(float value)
        {
            var radian = MathUtil.DegreesToRadians(value);
            decomposedRotation = new Vector3(radian);
            return Recompose(ref decomposedRotation);
        }

        private static Quaternion Recompose(ref Vector3 vector)
        {
            return Quaternion.RotationYawPitchRoll(vector.Y, vector.X, vector.Z);
        }
    }
}
