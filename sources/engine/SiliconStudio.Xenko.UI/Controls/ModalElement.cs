// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a modal element that puts an overlay upon the underneath elements and freeze their input.
    /// </summary>
    [DataContract(nameof(ModalElement))]
    [DebuggerDisplay("ModalElement - Name={Name}")]
    public class ModalElement : ButtonBase
    {
        internal Color OverlayColorInternal;

        /// <summary>
        /// The key to the IsModal dependency property.
        /// </summary>
        protected readonly static PropertyKey<bool> IsModalPropertyKey = new PropertyKey<bool>("IsModalKey", typeof(ModalElement), DefaultValueMetadata.Static(true));

        /// <summary>
        /// Occurs when the element is modal and the user click outside of the modal element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<RoutedEventArgs> OutsideClick
        {
            add { AddHandler(OutsideClickEvent, value); }
            remove { RemoveHandler(OutsideClickEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="OutsideClick"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> OutsideClickEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "OutsideClick",
            RoutingStrategy.Bubble,
            typeof(ModalElement));

        public ModalElement()
        {
            OverlayColorInternal = new Color(0, 0, 0, 0.6f);
            DrawLayerNumber += 1; // (overlay)
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
        }

        /// <summary>
        /// The color of the overlay drawn upon underneath elements.
        /// </summary>
        [DataMember]
        public Color OverlayColor
        {
            get { return OverlayColorInternal; }
            set { OverlayColorInternal = value; }
        }

        /// <summary>
        /// Determine if the control should block the input of underneath elements or not.
        /// </summary>
        [DataMemberIgnore]
        public bool IsModal
        {
            get { return DependencyProperties.Get(IsModalPropertyKey); }
            set { DependencyProperties.Set(IsModalPropertyKey, value); }
        }

        protected override void OnTouchUp(TouchEventArgs args)
        {
            base.OnTouchUp(args);

            if(!IsModal || args.Source != this)
                return;

            var position = args.WorldPosition - new Vector3(WorldMatrixInternal.M41, WorldMatrixInternal.M42, WorldMatrixInternal.M43);
            if (position.X < 0 || position.X > RenderSize.X
                || position.Y < 0 || position.Y > RenderSize.Y)
            {
                var eventArgs = new RoutedEventArgs(OutsideClickEvent);
                RaiseEvent(eventArgs);
            }
        }

        protected internal override bool Intersects(ref Ray ray, out Vector3 intersectionPoint)
        {
            if (!IsModal)
                return base.Intersects(ref ray, out intersectionPoint);

            if (LayoutingContext == null)
            {
                intersectionPoint = Vector3.Zero;
                return false;
            }

            var virtualResolution = LayoutingContext.VirtualResolution;
            var worldmatrix = Matrix.Identity;
            
            return CollisionHelper.RayIntersectsRectangle(ref ray, ref worldmatrix, ref virtualResolution, 2, out intersectionPoint);
        }
    }
}
