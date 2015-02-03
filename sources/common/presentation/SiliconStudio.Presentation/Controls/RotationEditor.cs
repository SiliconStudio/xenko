using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class RotationEditor : Control
    {
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;
        private Vector3 decomposedRotation;

        /// <summary>
        /// Identifies the <see cref="Rotation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(Quaternion), typeof(RotationEditor), new FrameworkPropertyMetadata(default(Quaternion), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRotationPropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAnglePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAnglePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(float), typeof(RotationEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAnglePropertyChanged));

        /// <summary>
        /// The <see cref="Vector3"/> associated to this control.
        /// </summary>
        public Quaternion Rotation { get { return (Quaternion)GetValue(RotationProperty); } set { SetValue(RotationProperty, value); } }

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

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            templateApplied = false;
            base.OnApplyTemplate();
            templateApplied = true;
        }

        /// <summary>
        /// Raised when the <see cref="Rotation"/> property is modified.
        /// </summary>
        private void OnRotationPropertyChanged()
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = RotationProperty;

            if (!interlock)
            {
                var rotation = Rotation;
                Matrix rotationMatrix;
                Matrix.RotationQuaternion(ref rotation, out rotationMatrix);
                rotationMatrix.DecomposeXYZ(out decomposedRotation);
                interlock = true;
                SetCurrentValue(XProperty, MathUtil.RadiansToDegrees(decomposedRotation.X));
                SetCurrentValue(YProperty, MathUtil.RadiansToDegrees(decomposedRotation.Y));
                SetCurrentValue(ZProperty, MathUtil.RadiansToDegrees(decomposedRotation.Z));
                interlock = false;
            }

            UpdateBinding(RotationProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the <see cref="X"/>, <see cref="Y"/> or <see cref="Z"/> properties are modified.
        /// </summary>
        /// <param name="e">The dependency property that has changed.</param>
        private void OnAnglePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;

            if (!interlock)
            {
                if (e.Property == XProperty)
                    decomposedRotation = new Vector3(MathUtil.DegreesToRadians((float)e.NewValue), decomposedRotation.Y, decomposedRotation.Z);
                else if (e.Property == YProperty)
                    decomposedRotation = new Vector3(decomposedRotation.X, MathUtil.DegreesToRadians((float)e.NewValue), decomposedRotation.Z);
                else if (e.Property == ZProperty)
                    decomposedRotation = new Vector3(decomposedRotation.X, decomposedRotation.Y, MathUtil.DegreesToRadians((float)e.NewValue));
                else
                    throw new ArgumentException("Property unsupported by method OnAnglePropertyChanged.");

                Quaternion quatX, quatY, quatZ;
                Quaternion.RotationX(decomposedRotation.X, out quatX);
                Quaternion.RotationY(decomposedRotation.Y, out quatY);
                Quaternion.RotationZ(decomposedRotation.Z, out quatZ);

                interlock = true;
                Rotation = quatX * quatY * quatZ;
                interlock = false;
            }

            UpdateBinding(e.Property);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Updates the binding of the given dependency property.
        /// </summary>
        /// <param name="dependencyProperty">The dependency property.</param>
        private void UpdateBinding(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty != initializingProperty)
            {
                BindingExpression expression = GetBindingExpression(dependencyProperty);
                if (expression != null)
                    expression.UpdateSource();
            }
        }

        /// <summary>
        /// Raised by <see cref="RotationProperty"/> when the <see cref="Rotation"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnRotationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (RotationEditor)sender;
            editor.OnRotationPropertyChanged();
        }

        /// <summary>
        /// Raised by <see cref="XProperty"/>, <see cref="YProperty"/> or <see cref="ZProperty"/> when respectively
        /// the <see cref="X"/>, <see cref="Y"/> or <see cref="Z"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnAnglePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (RotationEditor)sender;
            editor.OnAnglePropertyChanged(e);
        }
    }
}