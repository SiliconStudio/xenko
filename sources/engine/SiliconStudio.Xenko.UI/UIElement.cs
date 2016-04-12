// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Provides a base class for all the User Interface elements in Xenko applications.
    /// </summary>
    [DataContract(Inherited = true)]
    [DebuggerDisplay("UIElement: {Name}")]
    public abstract class UIElement : IUIElementUpdate, IIdentifiable
    {
        /// <summary>
        /// The key to the height dependency property.
        /// </summary>
        public readonly static PropertyKey<float> DefaultWidthPropertyKey =
            new PropertyKey<float>("DefaultWidthKey", typeof(UIElement), DefaultValueMetadata.Static(0f), ValidateValueMetadata.New<float>(DefaultSizeValidator), ObjectInvalidationMetadata.New<float>(DefaultSizeInvalidation));
        /// <summary>
        /// The key to the height dependency property.
        /// </summary>
        public readonly static PropertyKey<float> DefaultHeightPropertyKey =
            new PropertyKey<float>("DefaultHeightKey", typeof(UIElement), DefaultValueMetadata.Static(0f), ValidateValueMetadata.New<float>(DefaultSizeValidator), ObjectInvalidationMetadata.New<float>(DefaultSizeInvalidation));
        /// <summary>
        /// The key to the height dependency property.
        /// </summary>
        public readonly static PropertyKey<float> DefaultDepthPropertyKey =
            new PropertyKey<float>("DefaultDepthKey", typeof(UIElement), DefaultValueMetadata.Static(0f), ValidateValueMetadata.New<float>(DefaultSizeValidator), ObjectInvalidationMetadata.New<float>(DefaultSizeInvalidation));
        /// <summary>
        /// The key to the name dependency property.
        /// </summary>
        public readonly static PropertyKey<string> NamePropertyKey =
            new PropertyKey<string>("NameKey", typeof(UIElement), DefaultValueMetadata.Static<string>(null), ObjectInvalidationMetadata.New<string>(NameInvalidationCallback));
        /// <summary>
        /// The key to the Background color dependency property.
        /// </summary>
        private readonly static PropertyKey<Color> BackgroundColorPropertyKey =
            new PropertyKey<Color>("BackgroundColorKey", typeof(Color), DefaultValueMetadata.Static(new Color(0, 0, 0, 0)));
        
        private static readonly RoutedEvent<TouchEventArgs> PreviewTouchDownEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("PreviewTouchDown", RoutingStrategy.Tunnel, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> PreviewTouchMoveEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("PreviewTouchMove", RoutingStrategy.Tunnel, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> PreviewTouchUpEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("PreviewTouchUp", RoutingStrategy.Tunnel, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> TouchDownEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("TouchDown", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> TouchEnterEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("TouchEnter", RoutingStrategy.Direct, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> TouchLeaveEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("TouchLeave", RoutingStrategy.Direct, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> TouchMoveEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("TouchMove", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> TouchUpEvent =
            EventManager.RegisterRoutedEvent<TouchEventArgs>("TouchUp", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> KeyPressedEvent =
            EventManager.RegisterRoutedEvent<KeyEventArgs>("KeyPressed", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> KeyDownEvent =
            EventManager.RegisterRoutedEvent<KeyEventArgs>("KeyDown", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> KeyReleasedEvent =
            EventManager.RegisterRoutedEvent<KeyEventArgs>("KeyReleased", RoutingStrategy.Bubble, typeof(UIElement));

        private static readonly Queue<List<RoutedEventHandlerInfo>> RoutedEventHandlerInfoListPool = new Queue<List<RoutedEventHandlerInfo>>();
        
        internal bool HierarchyDisablePicking;
        internal Vector3 RenderSizeInternal;
        internal Matrix WorldMatrixInternal;
        internal protected Thickness MarginInternal = Thickness.UniformCuboid(0f);

        private Visibility visibility = Visibility.Visible;
        private float opacity = 1.0f;
        private bool isEnabled = true;
        private bool isHierarchyEnabled = true;
        private float width = float.NaN;
        private float height = float.NaN;
        private float depth = float.NaN;
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment verticalAlignment = VerticalAlignment.Stretch;
        private DepthAlignment depthAlignment = DepthAlignment.Center;
        private float maximumWidth = float.PositiveInfinity;
        private float maximumHeight = float.PositiveInfinity;
        private float maximumDepth = float.PositiveInfinity;
        private float minimumWidth;
        private float minimumHeight;
        private float minimumDepth;
        private Matrix localMatrix = Matrix.Identity;
        private MouseOverState mouseOverState;
        private LayoutingContext layoutingContext;
        private Style style;
        private ResourceDictionary resourceDictionary;

        protected bool ArrangeChanged;
        protected bool LocalMatrixChanged;

        private Vector3 previousProvidedMeasureSize = new Vector3(-1,-1,-1);
        private Vector3 previousProvidedArrangeSize = new Vector3(-1,-1,-1);
        private bool previousIsParentCollapsed;

        static UIElement()
        {
            // register the class handlers
            EventManager.RegisterClassHandler(typeof(UIElement), PreviewTouchDownEvent, PreviewTouchDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), PreviewTouchMoveEvent, PreviewTouchMoveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), PreviewTouchUpEvent, PreviewTouchUpClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), TouchDownEvent, TouchDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), TouchEnterEvent, TouchEnterClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), TouchLeaveEvent, TouchLeaveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), TouchMoveEvent, TouchMoveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), TouchUpEvent, TouchUpClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), KeyPressedEvent, KeyPressedClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), KeyDownEvent, KeyDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), KeyReleasedEvent, KeyReleasedClassHandler);
        }

        /// <summary>
        /// Creates a new instance of <see cref="UIElement"/>.
        /// </summary>
        protected UIElement()
        {
            Id = Guid.NewGuid();
            DependencyProperties = new PropertyContainer(this);
            VisualChildrenCollection = new UIElementCollection();
            DrawLayerNumber = 1; // one layer for BackgroundColor/Clipping
        }
        
        /// <summary>
        /// The <see cref="UIElement"/> that currently has the focus.
        /// </summary>
        internal static UIElement FocusedElement { get; set; }

        /// <summary>
        /// A unique ID defining the UI element.
        /// </summary>
        [DataMember]
        [Display(Browsable = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// List of the dependency properties attached to the object.
        /// </summary>
        [DataMember]
        public PropertyContainer DependencyProperties;

        /// <summary>
        /// Gets the size that this element computed during the measure pass of the layout process.
        /// </summary>
        /// <remarks>This value does not contain possible <see cref="Margin"/></remarks>
        [DataMemberIgnore]
        public Vector3 DesiredSize { get; private set; }

        /// <summary>
        /// Gets the size that this element computed during the measure pass of the layout process.
        /// </summary>
        /// <remarks>This value contains possible <see cref="Margin"/></remarks>
        [DataMemberIgnore]
        public Vector3 DesiredSizeWithMargins { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the computed size and position of child elements in this element's layout are valid.
        /// </summary>
        [DataMemberIgnore]
        public bool IsArrangeValid { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current size returned by layout measure is valid.
        /// </summary>
        [DataMemberIgnore]
        public bool IsMeasureValid { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UIElement"/> is currently touched by the user.
        /// </summary>
        [DataMemberIgnore]
        public bool IsTouched { get; internal set; }

        /// <summary>
        /// The world matrix of the UIElement.
        /// The origin of the element is the center of the object's bounding box defined by <see cref="RenderSize"/>.
        /// </summary>
        [DataMemberIgnore]
        public Matrix WorldMatrix
        {
            get { return WorldMatrixInternal; }
            private set { WorldMatrixInternal = value; }
        }

        /// <summary>
        /// The final depth bias value of the element resulting from the parent/children z order update.
        /// </summary>
        internal int DepthBias { get; private set; }

        /// <summary>
        /// The maximum depth bias value among the children of the element resulting from the parent/children z order update.
        /// </summary>
        internal int MaxChildrenDepthBias { get; private set; }

        /// <summary>
        /// The number of layers used to draw this element. 
        /// This value has to be modified by the user when he redefines the default element renderer,
        /// so that <see cref="DepthBias"/> values of the relatives keeps enough spaces to draw the different layers.
        /// </summary>
        [DataMember]
        public int DrawLayerNumber { get; set; }

        internal bool ForceNextMeasure = true;
        internal bool ForceNextArrange = true;

        /// <summary>
        /// Gets or sets the style of this UI element.
        /// </summary>
        internal Style Style
        {
            get { return style; }
            set
            {
                // Style is same, skip
                if (style == value)
                    return;

                // Check if we already had a style
                if (style != null)
                    throw new InvalidOperationException("Style can't be changed once it has been applied.");

                // Run each setter for undefined values
                var currentStyle = value;
                while (currentStyle != null)
                {
                    foreach (var setter in currentStyle.Setters)
                    {
                        setter.ApplyIfNotSet(ref DependencyProperties);
                    }
                    currentStyle = currentStyle.BasedOn;
                }

                // Set it as current style
                style = value;
            }
        }

        /// <summary>
        /// Gets or sets the resource dictionary associated to this element.
        /// </summary>
        internal ResourceDictionary ResourceDictionary
        {
            get { return resourceDictionary; }
            set
            {
                if (value == resourceDictionary)
                    return;

                resourceDictionary = value;

                if (resourceDictionary != null)
                {
                    // Try to find matching style
                    object matchingStyle;
                    if (resourceDictionary.TryGetValue(GetType(), out matchingStyle))
                    {
                        // Apply style
                        Style = (Style)matchingStyle;
                    }
                }

                foreach (var child in VisualChildren)
                    child.ResourceDictionary = value;
            }
        }

        /// <summary>
        /// The ratio between the element real size on the screen and the element virtual size.
        /// </summary>
        protected internal LayoutingContext LayoutingContext
        {
            get { return layoutingContext; }
            set
            {
                if (value == null)
                    return;

                if (layoutingContext != null && layoutingContext.Equals(value))
                    return;

                ForceMeasure();
                layoutingContext = value;
                foreach (var child in VisualChildren)
                    child.LayoutingContext = value;
            }
        }

        /// <summary>
        /// The visual children of this element. 
        /// </summary>
        /// <remarks>If the class is inherited it is the responsibility of the descendant class to correctly update this collection</remarks>
        [DataMemberIgnore]
        internal protected UIElementCollection VisualChildrenCollection { get; }

        private static void DefaultSizeInvalidation(object propertyOwner, PropertyKey<float> propertyKey, float propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.InvalidateMeasure();
        }

        /// <summary>
        /// Invalidates the arrange state (layout) for the element. 
        /// </summary>
        protected internal void InvalidateArrange()
        {
            ForceArrange(); // force arrange on top hierarchy

            PropagateArrangeInvalidationToChildren(); // propagate weak invalidation on children
        }

        private void PropagateArrangeInvalidationToChildren()
        {
            foreach (var child in VisualChildrenCollection)
            {
                if (child.IsArrangeValid)
                {
                    child.IsArrangeValid = false;
                    child.PropagateArrangeInvalidationToChildren();
                }
            }
        }

        private void ForceArrange()
        {
            if(ForceNextArrange) // no need to propagate arrange force if it's already done
                return;

            IsArrangeValid = false;
            ForceNextArrange = true;

            VisualParent?.ForceArrange();
        }

        /// <summary>
        /// Invalidates the measurement state (layout) for the element.
        /// </summary>
        protected internal void InvalidateMeasure()
        {
            ForceMeasure(); // force measure on top hierarchy

            PropagateMeasureInvalidationToChildren(); // propagate weak invalidation on children
        }

        private void PropagateMeasureInvalidationToChildren()
        {
            foreach (var child in VisualChildrenCollection)
            {
                if (child.IsMeasureValid)
                {
                    child.IsMeasureValid = false;
                    child.IsArrangeValid = false;
                    child.PropagateMeasureInvalidationToChildren();
                }
            }
        }

        private void ForceMeasure()
        {
            if (ForceNextMeasure && ForceNextArrange) // no need to propagate arrange force if it's already done
                return;

            ForceNextMeasure = true;
            ForceNextArrange = true;

            IsMeasureValid = false;
            IsArrangeValid = false;

            VisualParent?.ForceMeasure();
        }

        private static void NameInvalidationCallback(object propertyOwner, PropertyKey<string> propertyKey, string propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.OnNameChanged();
        }

        /// <summary>
        /// This method is call when the name of the UIElement changes.
        /// This method can be overridden in inherited classes to perform class specific actions on <see cref="Name"/> changes.
        /// </summary>
        protected virtual void OnNameChanged()
        {
        }

        /// <summary>
        /// Indicate if the UIElement can be hit by the user. 
        /// If this property is true, the UI system performs hit test on the UIElement.
        /// </summary>
        [DataMember]
        public bool CanBeHitByUser { get; set; }

        /// <summary>
        /// This property can be set to <value>true</value> to disable all touch events on the element's children.
        /// </summary>
        [DataMember]
        public bool PreventChildrenFromBeingHit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this element is enabled in the user interface (UI).
        /// </summary>
        [DataMember]
        public virtual bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;

                MouseOverState = MouseOverState.MouseOverNone;
            }
        }

        /// <summary>
        /// Gets the value indicating whether this element and all its upper hierarchy are enabled or not.
        /// </summary>
        public bool IsHierarchyEnabled => isHierarchyEnabled;

        /// <summary>
        /// Gets a value indicating whether this element is visible in the user interface (UI).
        /// </summary>
        public bool IsVisible => Visibility == Visibility.Visible;

        /// <summary>
        /// Gets a value indicating whether this element takes some place in the user interface.
        /// </summary>
        public bool IsCollapsed => Visibility == Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the opacity factor applied to the entire UIElement when it is rendered in the user interface (UI). This is a dependency property.
        /// </summary>
        /// <remarks>Value is clamped between [0,1].</remarks>
        [DataMember]
        public float Opacity
        {
            get { return opacity; }
            set { opacity = Math.Max(0, Math.Min(1, value)); }
        }

        /// <summary>
        /// Gets or sets the user interface (UI) visibility of this element. This is a dependency property.
        /// </summary>
        [DataMember]
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if(value == visibility)
                    return; 

                visibility = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the current state of the mouse over the UI element.
        /// </summary>
        /// <remarks>Only elements that can be clicked by user can have the <cref>MouseOverState.MouseOverElement</cref> value. 
        /// That is element that have <see cref="CanBeHitByUser"/> set to <value>true</value></remarks>
        [DataMemberIgnore]
        public MouseOverState MouseOverState
        {
            get { return mouseOverState; }
            internal set
            {
                var oldValue = mouseOverState;
                if(oldValue == value)
                    return;

                mouseOverState = value;

                MouseOverStateChanged?.Invoke(this, new PropertyChangedArgs<MouseOverState> { NewValue = value, OldValue = oldValue });
            }
        }

        /// <summary>
        /// Gets or sets the default height of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be a finite positive real number.</exception>
        [DataMemberIgnore]
        public float DefaultHeight
        {
            get { return DependencyProperties.Get(DefaultHeightPropertyKey); }
            set { DependencyProperties.Set(DefaultHeightPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the default width of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be a finite positive real number.</exception>
        [DataMemberIgnore]
        public float DefaultWidth
        {
            get { return DependencyProperties.Get(DefaultWidthPropertyKey); }
            set { DependencyProperties.Set(DefaultWidthPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the default width of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be a finite positive real number.</exception>
        [DataMemberIgnore]
        public float DefaultDepth
        {
            get { return DependencyProperties.Get(DefaultDepthPropertyKey); }
            set { DependencyProperties.Set(DefaultDepthPropertyKey, value); }
        }
        
        private static void DefaultSizeValidator(ref float size)
        {
            if (size < 0 || float.IsInfinity(size) || float.IsNaN(size))
                throw new ArgumentOutOfRangeException(nameof(size));
        }

        /// <summary>
        /// Gets or sets the user suggested height of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite  or undefined.</exception>
        [DataMember]
        public float Height
        {
            get { return height; }
            set
            {
                if (value < 0 || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                height = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the user suggested width of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite  or undefined.</exception>
        [DataMember]
        public float Width
        {
            get { return width; }
            set
            {
                if (value < 0 || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                width = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the user suggested width of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite or undefined.</exception>
        [DataMember]
        public float Depth
        {
            get { return depth; }
            set
            {
                if (value < 0 || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                depth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the size of the element. Same as setting separately <see cref="Width"/>, <see cref="Height"/>, and <see cref="Depth"/>
        /// </summary>
        [DataMemberIgnore]
        public Vector3 Size
        {
            get { return new Vector3(Width, Height, Depth); }
            set
            {
                Width = value.X;
                Height = value.Y;
                Depth = value.Z;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Set one component of the size of the element.
        /// </summary>
        /// <param name="dimensionIndex">Index indicating which component to set</param>
        /// <param name="value">The value to give to the size</param>
        internal void SetSize(int dimensionIndex, float value)
        {
            if (dimensionIndex == 0)
                Width = value;
            else if (dimensionIndex == 1)
                Height = value;
            else
                Depth = value;

            InvalidateMeasure();
        }

        /// <summary>
        /// Gets or sets the minimum width of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite.</exception>
        [DataMember]
        public float MinimumWidth
        {
            get { return minimumWidth; }
            set
            {
                if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                minimumWidth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the minimum height of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite.</exception>
        [DataMember]
        public float MinimumHeight
        {
            get { return minimumHeight; }
            set
            {
                if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                minimumHeight = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the minimum height of this element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite.</exception>
        [DataMember]
        public float MinimumDepth
        {
            get { return minimumDepth; }
            set
            {
                if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                minimumDepth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to clip the content of this element (or content coming from the child elements of this element) 
        /// to fit into the size of the containing element. This is a dependency property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive and finite.</exception>
        [DataMember]
        public bool ClipToBounds { get; set; }

        /// <summary>
        /// Gets or sets the maximum width of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        [DataMember]
        public float MaximumWidth
        {
            get { return maximumWidth; }
            set
            {
                if (value < 0 || float.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                maximumWidth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the maximum height of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        [DataMember]
        public float MaximumHeight
        {
            get { return maximumHeight; }
            set
            {
                if (value < 0 || float.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                maximumHeight = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the maximum height of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        [DataMember]
        public float MaximumDepth
        {
            get { return maximumDepth; }
            set
            {
                if (value < 0 || float.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                maximumDepth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of this element.
        /// </summary>
        [DataMember]
        public HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set
            {
                horizontalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of this element.
        /// </summary>
        [DataMember]
        public VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set
            {
                verticalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Gets or sets the depth alignment of this element.
        /// </summary>
        [DataMember]
        public DepthAlignment DepthAlignment
        {
            get { return depthAlignment; }
            set
            {
                depthAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Gets or sets the name of this element. This is a dependency property.
        /// </summary>
        [DataMemberIgnore]
        public string Name
        {
            get { return DependencyProperties.Get(NamePropertyKey); }
            set { DependencyProperties.Set(NamePropertyKey, value); }
        }

        /// <summary>
        /// Gets the logical parent of this element. This is a dependency property.
        /// </summary>
        [DataMemberIgnore]
        public UIElement Parent { get; protected set; }

        /// <summary>
        /// Gets the visual parent of this element. This is a dependency property.
        /// </summary>
        [DataMemberIgnore]
        public UIElement VisualParent { get; protected set; }

        /// <summary>
        /// Get a enumerable to the visual children of the <see cref="UIElement"/>.
        /// </summary>
        /// <remarks>Inherited classes are in charge of overriding this method to return their children.</remarks>
        [DataMemberIgnore]
        public IEnumerable<UIElement> VisualChildren => VisualChildrenCollection;

        /// <summary>
        /// The list of the children of the element that can be hit by the user.
        /// </summary>
        protected internal virtual FastCollection<UIElement> HitableChildren => VisualChildrenCollection;

        /// <summary>
        /// Gets or sets the margins of this element.
        /// </summary>
        [DataMember]
        public Thickness Margin
        {
            get { return MarginInternal; }
            set 
            { 
                MarginInternal = value; 
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the LocalMatrix of this element.
        /// </summary>
        /// <remarks>The local transform is not taken is account during the layering. The transformation is purely for rendering effects.</remarks>
        [DataMember]
        [Display(Browsable = false)]
        public Matrix LocalMatrix
        {
            get { return localMatrix; }
            set
            {
                localMatrix = value;
                LocalMatrixChanged = true;
            }
        }

        /// <summary>
        /// The opacity used to render element. 
        /// </summary>
        [DataMemberIgnore]
        public float RenderOpacity { get; private set; }

        /// <summary>
        /// Gets (or sets, but see Remarks) the final render size of this element.
        /// </summary>
        [DataMemberIgnore]
        public Vector3 RenderSize
        {
            get { return RenderSizeInternal; }
            private set { RenderSizeInternal = value; }
        }

        /// <summary>
        /// The rendering offsets caused by the UIElement margins and alignments.
        /// </summary>
        [DataMemberIgnore]
        public Vector3 RenderOffsets { get; private set; }

        /// <summary>
        /// Gets the rendered width of this element.
        /// </summary>
        public float ActualWidth => RenderSize.X;

        /// <summary>
        /// Gets the rendered height of this element.
        /// </summary>
        public float ActualHeight => RenderSize.Y;

        /// <summary>
        /// Gets the rendered depth of this element.
        /// </summary>
        public float ActualDepth => RenderSize.Z;

        /// <summary>
        /// The background color of the element.
        /// </summary>
        [DataMemberIgnore]
        public Color BackgroundColor
        {
            get { return DependencyProperties.Get(BackgroundColorPropertyKey); }
            set { DependencyProperties.Set(BackgroundColorPropertyKey, value); }
        }

        private unsafe bool Vector3BinaryEqual(ref Vector3 left, ref Vector3 right)
        {
            fixed (Vector3* pVector3Left = &left)
            fixed (Vector3* pVector3Right = &right)
            {
                var pLeft = (int*)pVector3Left;
                var pRight = (int*)pVector3Right;

                return pLeft[0] == pRight[0] && pLeft[1] == pRight[1] && pLeft[2] == pRight[2];
            }
        }

        /// <summary>
        /// Updates the <see cref="DesiredSize"/> of a <see cref="UIElement"/>. 
        /// Parent elements call this method from their own implementations to form a recursive layout update. 
        /// Calling this method constitutes the first pass (the "Measure" pass) of a layout update.
        /// </summary>
        /// <param name="availableSizeWithMargins">The available space that a parent element can allocate a child element with its margins.
        /// A child element can request a larger space than what is available;  the provided size might be accommodated if scrolling is possible in the content model for the current element.</param>
        public void Measure(Vector3 availableSizeWithMargins)
        {
            if (!ForceNextMeasure && Vector3BinaryEqual(ref availableSizeWithMargins, ref previousProvidedMeasureSize))
            {
                IsMeasureValid = true;
                ValidateChildrenMeasure();
                return;
            }

            ForceNextMeasure = false;
            IsMeasureValid = true;
            IsArrangeValid = false;
            previousProvidedMeasureSize = availableSizeWithMargins;

            // avoid useless computation if the element is collapsed
            if (IsCollapsed)
            {
                DesiredSize = DesiredSizeWithMargins = Vector3.Zero;
                return;
            }

            // variable containing the temporary desired size 
            var desiredSize = new Vector3(Width, Height, Depth);

            // override the size if not set by the user
            if (float.IsNaN(desiredSize.X) || float.IsNaN(desiredSize.Y) || float.IsNaN(desiredSize.Z))
            {
                // either the width, height or the depth of the UIElement is not fixed
                // -> compute the desired size of the children to determine it

                // removes the size required for the margins in the available size
                var availableSizeWithoutMargins = CalculateSizeWithoutThickness(ref availableSizeWithMargins, ref MarginInternal);

                // trunk the available size for the element between the maximum and minimum width/height of the UIElement
                availableSizeWithoutMargins = new Vector3(
                Math.Max(MinimumWidth, Math.Min(MaximumWidth, !float.IsNaN(desiredSize.X)? desiredSize.X: availableSizeWithoutMargins.X)),
                Math.Max(MinimumHeight, Math.Min(MaximumHeight, !float.IsNaN(desiredSize.Y) ? desiredSize.Y : availableSizeWithoutMargins.Y)),
                Math.Max(MinimumDepth, Math.Min(MaximumDepth, !float.IsNaN(desiredSize.Z) ? desiredSize.Z : availableSizeWithoutMargins.Z)));

                // compute the desired size for the children
                var childrenDesiredSize = MeasureOverride(availableSizeWithoutMargins);

                // replace the undetermined size by the desired size for the children
                if (float.IsNaN(desiredSize.X))
                    desiredSize.X = childrenDesiredSize.X;
                if (float.IsNaN(desiredSize.Y))
                    desiredSize.Y = childrenDesiredSize.Y;
                if (float.IsNaN(desiredSize.Z))
                    desiredSize.Z = childrenDesiredSize.Z;

                // override the element size by the default size if still unspecified
                if (float.IsNaN(desiredSize.X))
                    desiredSize.X = DefaultWidth;
                if (float.IsNaN(desiredSize.Y))
                    desiredSize.Y = DefaultHeight;
                if (float.IsNaN(desiredSize.Z))
                    desiredSize.Z = DefaultDepth;
            }

            // trunk the desired size between the maximum and minimum width/height of the UIElement
            desiredSize = new Vector3(
                Math.Max(MinimumWidth, Math.Min(MaximumWidth, desiredSize.X)),
                Math.Max(MinimumHeight, Math.Min(MaximumHeight, desiredSize.Y)),
                Math.Max(MinimumDepth, Math.Min(MaximumDepth, desiredSize.Z)));

            // compute the desired size with margin
            var desiredSizeWithMargins = CalculateSizeWithThickness(ref desiredSize, ref MarginInternal);
            
            // update Element state variables
            DesiredSize = desiredSize;
            DesiredSizeWithMargins = desiredSizeWithMargins;
        }

        private void ValidateChildrenMeasure()
        {
            foreach (var child in VisualChildrenCollection)
            {
                if (!child.IsMeasureValid)
                {
                    child.IsMeasureValid = true;
                    child.ValidateChildrenMeasure();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for possible child elements and determines a size for the <see cref="UIElement"/>-derived class.
        /// </summary>
        /// <param name="availableSizeWithoutMargins">The available size that this element can give to child elements. 
        /// Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size desired by the children</returns>
        protected virtual Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return Vector3.Zero;
        }

        /// <summary>
        /// Positions child elements and determines the size of the UIElement. 
        /// This method constitutes the second pass of a layout update.
        /// </summary>
        /// <param name="finalSizeWithMargins">The final size that the parent computes for the child element with the margins.</param>
        /// <param name="isParentCollapsed">Boolean indicating if one of the parents of the element is currently collapsed.</param>
        public void Arrange(Vector3 finalSizeWithMargins, bool isParentCollapsed)
        {
            if (!ForceNextArrange && Vector3BinaryEqual(ref finalSizeWithMargins, ref previousProvidedArrangeSize) && isParentCollapsed == previousIsParentCollapsed)
            {
                IsArrangeValid = true;
                ValidateChildrenArrange();
                return;
            }

            ForceNextArrange = false;
            IsArrangeValid = true;
            ArrangeChanged = true;
            previousIsParentCollapsed = isParentCollapsed;
            previousProvidedArrangeSize = finalSizeWithMargins;

            // special to avoid useless computation if the element is collapsed
            if (IsCollapsed || isParentCollapsed)
            {
                CollapseOverride();
                return;
            }

            // initialize the element size with the user suggested size (maybe NaN if not set)
            var elementSize = new Vector3(Width, Height, Depth);

            // stretch the element if the user size is unspecified and alignment constraints requires it
            var finalSizeWithoutMargins = CalculateSizeWithoutThickness(ref finalSizeWithMargins, ref MarginInternal);
            if (float.IsNaN(elementSize.X) && HorizontalAlignment == HorizontalAlignment.Stretch)
                elementSize.X = finalSizeWithoutMargins.X;
            if (float.IsNaN(elementSize.Y) && VerticalAlignment == VerticalAlignment.Stretch)
                elementSize.Y = finalSizeWithoutMargins.Y;
            if (float.IsNaN(elementSize.Z) && DepthAlignment == DepthAlignment.Stretch)
                elementSize.Z = finalSizeWithoutMargins.Z;

            // override the element size by the desired size if still unspecified
            if (float.IsNaN(elementSize.X))
                elementSize.X = Math.Min(DesiredSize.X, finalSizeWithoutMargins.X);
            if (float.IsNaN(elementSize.Y))
                elementSize.Y = Math.Min(DesiredSize.Y, finalSizeWithoutMargins.Y);
            if (float.IsNaN(elementSize.Z))
                elementSize.Z = Math.Min(DesiredSize.Z, finalSizeWithoutMargins.Z);

            // trunk the element size between the maximum and minimum width/height of the UIElement
            elementSize = new Vector3(
                Math.Max(MinimumWidth, Math.Min(MaximumWidth, elementSize.X)),
                Math.Max(MinimumHeight, Math.Min(MaximumHeight, elementSize.Y)),
                Math.Max(MinimumDepth, Math.Min(MaximumDepth, elementSize.Z)));

            // let ArrangeOverride decide of the final taken size 
            elementSize = ArrangeOverride(elementSize);
            
            // compute the rendering offsets
            var renderOffsets = CalculateAdjustmentOffsets(ref MarginInternal, ref finalSizeWithMargins, ref elementSize);

            // update UIElement internal variables
            RenderSize = elementSize;
            RenderOffsets = renderOffsets;
        }

        private void ValidateChildrenArrange()
        {
            foreach (var child in VisualChildrenCollection)
            {
                if (!child.IsArrangeValid)
                {
                    child.IsArrangeValid = true;
                    child.ValidateChildrenArrange();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, positions possible child elements and determines a size for a <see cref="UIElement"/> derived class.
        /// </summary>
        /// <param name="finalSizeWithoutMargins">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected virtual Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            return finalSizeWithoutMargins;
        }

        /// <summary>
        /// When overridden in a derived class, collapse possible child elements and derived class.
        /// </summary>
        protected virtual void CollapseOverride()
        {
            DesiredSize = Vector3.Zero;
            DesiredSizeWithMargins = Vector3.Zero;
            RenderSize = Vector3.Zero;
            RenderOffsets = Vector3.Zero;

            foreach (var child in VisualChildrenCollection)
                PropagateCollapseToChild(child);
        }

        /// <summary>
        /// Propagate the collapsing to a child element <paramref name="element"/>. 
        /// </summary>
        /// <param name="element">A child element to which propagate the collapse.</param>
        /// <exception cref="InvalidOperationException"><paramref name="element"/> is not a child of this element.</exception>
        protected void PropagateCollapseToChild(UIElement element)
        {
            if (element.VisualParent != this)
                throw new InvalidOperationException("Element is not a child of this element.");

            element.InvalidateMeasure();
            element.CollapseOverride();
        }
        
        /// <summary>
        /// Finds an element that has the provided identifier name in the element children.
        /// </summary>
        /// <param name="name">The name of the requested element.</param>
        /// <returns>The requested element. This can be null if no matching element was found.</returns>
        /// <remarks>If several elements with the same name exist return the first found</remarks>
        public UIElement FindName(string name)
        {
            if (Name == name)
                return this;

            return VisualChildren.Select(child => child.FindName(name)).FirstOrDefault(elt => elt != null);
        }

        /// <summary>
        /// Set the parent to a child.
        /// </summary>
        /// <param name="child">The child to which set the parent.</param>
        /// <param name="parent">The parent of the child.</param>
        protected static void SetParent(UIElement child, UIElement parent)
        {
            if (parent != null && child.Parent != null && parent != child.Parent)
                throw new InvalidOperationException("The UI element 'Name="+child.Name+"' has already as parent the element 'Name="+child.Parent.Name+"'.");

            child.Parent = parent;
        }

        /// <summary>
        /// Set the visual parent to a child.
        /// </summary>
        /// <param name="child">The child to which set the visual parent.</param>
        /// <param name="parent">The parent of the child.</param>
        protected static void SetVisualParent(UIElement child, UIElement parent)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (parent != null && child.VisualParent != null && parent != child.VisualParent)
                throw new InvalidOperationException("The UI element 'Name=" + child.Name + "' has already as visual parent the element 'Name=" + child.VisualParent.Name + "'.");

            child.VisualParent?.VisualChildrenCollection.Remove(child);

            child.VisualParent = parent;

            if (parent != null)
            {
                child.ResourceDictionary = parent.ResourceDictionary;
                child.LayoutingContext = parent.layoutingContext;
                parent.VisualChildrenCollection.Add(child);
            }
        }

        /// <summary>
        /// Calculate the intersection of the UI element and the ray.
        /// </summary>
        /// <param name="ray">The ray in world space coordinate</param>
        /// <param name="intersectionPoint">The intersection point in world space coordinate</param>
        /// <returns><value>true</value> if the two elements intersects, <value>false</value> otherwise</returns>
        internal protected virtual bool Intersects(ref Ray ray, out Vector3 intersectionPoint)
        {
            // does ray intersect element Oxy face?
            var intersects = CollisionHelper.RayIntersectsRectangle(ref ray, ref WorldMatrixInternal, ref RenderSizeInternal, 2, out intersectionPoint);

            // if element has depth also test other faces
            if (ActualDepth > MathUtil.ZeroTolerance)
            {
                Vector3 intersection;
                if (CollisionHelper.RayIntersectsRectangle(ref ray, ref WorldMatrixInternal, ref RenderSizeInternal, 0, out intersection))
                {
                    intersects = true;
                    if (intersection.Z > intersectionPoint.Z)
                        intersectionPoint = intersection;
                }
                if (CollisionHelper.RayIntersectsRectangle(ref ray, ref WorldMatrixInternal, ref RenderSizeInternal, 1, out intersection))
                {
                    intersects = true;
                    if (intersection.Z > intersectionPoint.Z)
                        intersectionPoint = intersection;
                }
            }

            return intersects;
        }

        void IUIElementUpdate.Update(GameTime time)
        {
            if(!IsEnabled)
                return;

            Update(time);

            foreach (var child in VisualChildrenCollection)
                ((IUIElementUpdate)child).Update(time);
        }

        void IUIElementUpdate.UpdateWorldMatrix(ref Matrix parentWorldMatrix, bool parentWorldChanged)
        {
            UpdateWorldMatrix(ref parentWorldMatrix, parentWorldChanged);
        }

        void IUIElementUpdate.UpdateElementState(int elementBias)
        {
            var parent = VisualParent;
            var parentRenderOpacity = 1f;
            var parentIsHierarchyEnabled = true;
            var parentHierarchyDisablePicking = false;

            if (parent != null)
            {
                parentRenderOpacity = parent.RenderOpacity;
                parentIsHierarchyEnabled = parent.IsHierarchyEnabled;
                parentHierarchyDisablePicking = parent.HierarchyDisablePicking;
            }

            RenderOpacity = parentRenderOpacity * Opacity;
            isHierarchyEnabled = parentIsHierarchyEnabled && isEnabled;
            HierarchyDisablePicking = parentHierarchyDisablePicking || PreventChildrenFromBeingHit;
            DepthBias = elementBias;

            var currentElementDepthBias = DepthBias + DrawLayerNumber;

            foreach (var visualChild in VisualChildrenCollection)
            {
                ((IUIElementUpdate)visualChild).UpdateElementState(currentElementDepthBias);

                currentElementDepthBias = visualChild.MaxChildrenDepthBias + (visualChild.ClipToBounds ? visualChild.DrawLayerNumber : 0);
            }

            MaxChildrenDepthBias = currentElementDepthBias;
        }

        /// <summary>
        /// Method called by <see cref="IUIElementUpdate.Update"/>.
        /// This method can be overridden by inherited classes to perform time-based actions.
        /// This method is not in charge to recursively call the update on children elements, this is automatically done.
        /// </summary>
        /// <param name="time">The current time of the game</param>
        protected virtual void Update(GameTime time)
        {
        }

        /// <summary>
        /// Method called by <see cref="IUIElementUpdate.UpdateWorldMatrix"/>.
        /// Parents are in charge of recursively calling this function on their children.
        /// </summary>
        /// <param name="parentWorldMatrix">The world matrix of the parent.</param>
        /// <param name="parentWorldChanged">Boolean indicating if the world matrix provided by the parent changed</param>
        protected virtual void UpdateWorldMatrix(ref Matrix parentWorldMatrix, bool parentWorldChanged)
        {
            if (parentWorldChanged || LocalMatrixChanged || ArrangeChanged)
            {
                var localMatrixCopy = localMatrix;

                // include rendering offsets into the local matrix.
                localMatrixCopy.TranslationVector += RenderOffsets + RenderSize / 2;

                // calculate the world matrix of UIelement
                Matrix worldMatrix;
                Matrix.Multiply(ref localMatrixCopy, ref parentWorldMatrix, out worldMatrix);
                WorldMatrix = worldMatrix;

                LocalMatrixChanged = false;
                ArrangeChanged = false;
            }
        }

        /// <summary>
        /// Add the thickness values into the size calculation of a UI element.
        /// </summary>
        /// <param name="sizeWithoutMargins">The size without the thickness included</param>
        /// <param name="thickness">The thickness to add to the space</param>
        /// <returns>The size with the margins included</returns>
        protected static Vector3 CalculateSizeWithThickness(ref Vector3 sizeWithoutMargins, ref Thickness thickness)
        {
            var negativeThickness = -thickness;
            return CalculateSizeWithoutThickness(ref sizeWithoutMargins, ref negativeThickness);
        }

        /// <summary>
        /// Remove the thickness values into the size calculation of a UI element.
        /// </summary>
        /// <param name="sizeWithMargins">The size with the thickness included</param>
        /// <param name="thickness">The thickness to remove in the space</param>
        /// <returns>The size with the margins not included</returns>
        protected static Vector3 CalculateSizeWithoutThickness(ref Vector3 sizeWithMargins, ref Thickness thickness)
        {
            return new Vector3(
                    Math.Max(0, sizeWithMargins.X - thickness.Left - thickness.Right),
                    Math.Max(0, sizeWithMargins.Y - thickness.Top - thickness.Bottom),
                    Math.Max(0, sizeWithMargins.Z - thickness.Front - thickness.Back));
        }
        
        /// <summary>
        /// Computes the (X,Y,Z) offsets to position correctly the UI element given the total provided space to it.
        /// </summary>
        /// <param name="thickness">The thickness around the element to position.</param>
        /// <param name="providedSpace">The total space given to the child element by the parent</param>
        /// <param name="usedSpaceWithoutThickness">The space used by the child element without the thickness included in it.</param>
        /// <returns>The offsets</returns>
        protected Vector3 CalculateAdjustmentOffsets(ref Thickness thickness, ref Vector3 providedSpace, ref Vector3 usedSpaceWithoutThickness)
        {
            // compute the size of the element with the thickness included 
            var usedSpaceWithThickness = CalculateSizeWithThickness(ref usedSpaceWithoutThickness, ref thickness);

            // set offset for left and stretch alignments
            var offsets = new Vector3(thickness.Left, thickness.Top, thickness.Front);

            // align the element horizontally
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    offsets.X += (providedSpace.X - usedSpaceWithThickness.X) / 2;
                    break;
                case HorizontalAlignment.Right:
                    offsets.X += providedSpace.X - usedSpaceWithThickness.X;
                    break;
            }

            // align the element vertically
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    offsets.Y += (providedSpace.Y - usedSpaceWithThickness.Y) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    offsets.Y += providedSpace.Y - usedSpaceWithThickness.Y;
                    break;
            }

            // align the element vertically
            switch (DepthAlignment)
            {
                case DepthAlignment.Center:
                    offsets.Z += (providedSpace.Z - usedSpaceWithThickness.Z) / 2;
                    break;
                case DepthAlignment.Back:
                    offsets.Z += providedSpace.Z - usedSpaceWithThickness.Z;
                    break;
            }

            return offsets;
        }

        internal void PropagateRoutedEvent(RoutedEventArgs e)
        {
            var routedEvent = e.RoutedEvent;

            // propagate first if tunneling
            if (routedEvent.RoutingStrategy == RoutingStrategy.Tunnel)
                VisualParent?.PropagateRoutedEvent(e);

            // Trigger the class handler
            var classHandler = EventManager.GetClassHandler(GetType(), routedEvent);
            if (classHandler != null && (classHandler.HandledEventToo || !e.Handled))
                classHandler.Invoke(this, e);

            // Trigger instance handlers
            if (eventsToHandlers.ContainsKey(routedEvent))
            {
                // get a list of handler from the pool where we can copy the handler to trigger
                if (RoutedEventHandlerInfoListPool.Count == 0)
                    RoutedEventHandlerInfoListPool.Enqueue(new List<RoutedEventHandlerInfo>());
                var pooledList = RoutedEventHandlerInfoListPool.Dequeue();

                // copy the RoutedEventHandlerEventInfo list into a list of the pool in order to be able to modify the handler list in the handler itself
                pooledList.AddRange(eventsToHandlers[routedEvent]);

                // iterate on the pooled list to invoke handlers
                foreach (var handlerInfo in pooledList)
                {
                    if (handlerInfo.HandledEventToo || !e.Handled)
                        handlerInfo.Invoke(this, e);
                }

                // add the pooled list back to the pool.
                pooledList.Clear(); // avoid to keep dead references
                RoutedEventHandlerInfoListPool.Enqueue(pooledList);
            }

            // propagate afterwards if bubbling
            if (routedEvent.RoutingStrategy == RoutingStrategy.Bubble)
                VisualParent?.PropagateRoutedEvent(e);
        }

        /// <summary>
        /// Raises a specific routed event. The <see cref="RoutedEvent"/> to be raised is identified within the <see cref="RoutedEventArgs"/> instance 
        /// that is provided (as the <see cref="RoutedEvent"/> property of that event data).
        /// </summary>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data and also identifies the event to raise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The type of the routed event argument <paramref name="e"/> does not match the event handler second argument type.</exception>
        public void RaiseEvent(RoutedEventArgs e)
        {
            if (e == null) 
                throw new ArgumentNullException(nameof(e));

            if (e.RoutedEvent == null)
                return;

            if(!e.RoutedEvent.HandlerSecondArgumentType.GetTypeInfo().IsAssignableFrom(e.GetType().GetTypeInfo()))
                throw new InvalidOperationException("The type of second parameter of the handler (" + e.RoutedEvent.HandlerSecondArgumentType 
                                                        + ") is not assignable from the parameter 'e' type (" + e.GetType() + ").");

            var sourceWasNull = e.Source == null;
            if (sourceWasNull)  // set the source to default if needed
                e.Source = this;

            e.StartEventRouting();

            PropagateRoutedEvent(e);

            e.EndEventRouting();

            if (sourceWasNull) // reset the source if it was not explicitly set (event might be reused again for other sources)
                e.Source = null;
        }

        /// <summary>
        /// Adds a routed event handler for a specified routed event, adding the handler to the handler collection on the current element. 
        /// Specify handledEventsToo as true to have the provided handler be invoked for routed event that had already been marked as handled by another element along the event route.
        /// </summary>
        /// <param name="routedEvent">An identifier for the routed event to be handled.</param>
        /// <param name="handler">A reference to the handler implementation.</param>
        /// <param name="handledEventsToo">true to register the handler such that it is invoked even when the routed event is marked handled in its event data; 
        /// false to register the handler with the default condition that it will not be invoked if the routed event is already marked handled.</param>
        /// <exception cref="ArgumentNullException">Provided handler or routed event is null.</exception>
        public void AddHandler<T>(RoutedEvent<T> routedEvent, EventHandler<T> handler, bool handledEventsToo = false) where T : RoutedEventArgs
        {
            if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if(!eventsToHandlers.ContainsKey(routedEvent))
                eventsToHandlers[routedEvent] = new List<RoutedEventHandlerInfo>();

            eventsToHandlers[routedEvent].Add(new RoutedEventHandlerInfo<T>(handler, handledEventsToo));
        }

        /// <summary>
        /// Removes the specified routed event handler from this element.
        /// </summary>
        /// <param name="routedEvent">The identifier of the routed event for which the handler is attached.</param>
        /// <param name="handler">The specific handler implementation to remove from the event handler collection on this element.</param>
        /// <exception cref="ArgumentNullException">Provided handler or routed event is null.</exception>
        public void RemoveHandler<T>(RoutedEvent<T> routedEvent, EventHandler<T> handler) where T : RoutedEventArgs
        {
            if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if(!eventsToHandlers.ContainsKey(routedEvent))
                return;

            eventsToHandlers[routedEvent].Remove(new RoutedEventHandlerInfo<T>(handler));
        }

        private readonly Dictionary<RoutedEvent, List<RoutedEventHandlerInfo>> eventsToHandlers = new Dictionary<RoutedEvent, List<RoutedEventHandlerInfo>>();

        /// <summary>
        /// Occurs when the value of the <see cref="MouseOverState"/> property changed.
        /// </summary>
        /// <remarks>This event is not a routed event</remarks>
        public event PropertyChangedHandler<MouseOverState> MouseOverStateChanged;

        /// <summary>
        /// Occurs when the user starts touching the <see cref="UIElement"/>. That is when he moves its finger down from the element.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchDown
        {
            add { AddHandler(PreviewTouchDownEvent, value); }
            remove { RemoveHandler(PreviewTouchDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the user moves its finger on the <see cref="UIElement"/>. 
        /// That is when his finger was already on the element and moved from its previous position.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchMove
        {
            add { AddHandler(PreviewTouchMoveEvent, value); }
            remove { RemoveHandler(PreviewTouchMoveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user stops touching the <see cref="UIElement"/>. That is when he moves its finger up from the element.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchUp
        {
            add { AddHandler(PreviewTouchUpEvent, value); }
            remove { RemoveHandler(PreviewTouchUpEvent, value); }
        }

        /// <summary>
        /// Occurs when the user starts touching the <see cref="UIElement"/>. That is when he moves its finger down from the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchDown
        {
            add { AddHandler(TouchDownEvent, value); }
            remove { RemoveHandler(TouchDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the user enters its finger into <see cref="UIElement"/>. 
        /// That is when his finger was on the screen outside of the element and moved inside the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchEnter
        {
            add { AddHandler(TouchEnterEvent, value); }
            remove { RemoveHandler(TouchEnterEvent, value); }
        }

        /// <summary>
        /// Occurs when the user leaves its finger from the <see cref="UIElement"/>. 
        /// That is when his finger was inside of the element and moved on the screen outside of the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchLeave
        {
            add { AddHandler(TouchLeaveEvent, value); }
            remove { RemoveHandler(TouchLeaveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user move its finger inside the <see cref="UIElement"/>.
        /// That is when his finger was already on the element and moved from its previous position.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchMove
        {
            add { AddHandler(TouchMoveEvent, value); }
            remove { RemoveHandler(TouchMoveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user stops touching the <see cref="UIElement"/>. That is when he moves its finger up from the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchUp
        {
            add { AddHandler(TouchUpEvent, value); }
            remove { RemoveHandler(TouchUpEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user press a key on the keyboard.
        /// </summary>
        /// <remarks>A key pressed event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyPressed
        {
            add { AddHandler(KeyPressedEvent, value); }
            remove { RemoveHandler(KeyPressedEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user maintains a key pressed on the keyboard.
        /// </summary>
        /// <remarks>A key down event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyDown
        {
            add { AddHandler(KeyDownEvent, value); }
            remove { RemoveHandler(KeyDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user release a key on the keyboard.
        /// </summary>
        /// <remarks>A key released event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyReleased
        {
            add { AddHandler(KeyReleasedEvent, value); }
            remove { RemoveHandler(KeyReleasedEvent, value); }
        }

        internal void RaiseTouchDownEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = PreviewTouchDownEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = TouchDownEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchEnterEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = TouchEnterEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchLeaveEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = TouchLeaveEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchMoveEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = PreviewTouchMoveEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = TouchMoveEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchUpEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = PreviewTouchUpEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = TouchUpEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseKeyPressedEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = KeyPressedEvent;
            RaiseEvent(keyEventArgs);
        }

        internal void RaiseKeyDownEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = KeyDownEvent;
            RaiseEvent(keyEventArgs);
        }

        internal void RaiseKeyReleasedEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = KeyReleasedEvent;
            RaiseEvent(keyEventArgs);
        }

        private static void PreviewTouchDownClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if(uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchDown(TouchEventArgs args)
        {
            IsTouched = true;
        }

        private static void PreviewTouchMoveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchMove(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchMove"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchMove(TouchEventArgs args)
        {
        }

        private static void PreviewTouchUpClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchUp(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchUp"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchUp(TouchEventArgs args)
        {
            IsTouched = false;
        }

        private static void TouchDownClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchDown(TouchEventArgs args)
        {
        }

        private static void TouchEnterClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchEnter(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchEnter"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchEnter(TouchEventArgs args)
        {
            IsTouched = true;
        }

        private static void TouchLeaveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchLeave(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchLeave"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchLeave(TouchEventArgs args)
        {
            IsTouched = false;
        }

        private static void TouchMoveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchMove(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchMove"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchMove(TouchEventArgs args)
        {
        }

        private static void TouchUpClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchUp(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchUp"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchUp(TouchEventArgs args)
        {
        }

        private static void KeyPressedClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyPressed(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyPressed"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyPressed(KeyEventArgs args)
        {
        }

        private static void KeyDownClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyDown(KeyEventArgs args)
        {
        }

        private static void KeyReleasedClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyReleased(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyReleased"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyReleased(KeyEventArgs args)
        {
        }
    }
}
