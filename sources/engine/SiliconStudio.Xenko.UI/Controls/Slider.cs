// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a slider element.
    /// </summary>
    [DataContract(nameof(Slider))]
    public class Slider : UIElement
    {
        private float value;

        private bool shouldSnapToTicks;

        private Orientation orientation = Orientation.Horizontal;
        private float tickFrequency = 10.0f;
        private float minimum;
        private float maximum = 1.0f;
        private ISpriteProvider trackBackgroundImage;

        private void OnSizeChanged(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        static Slider()
        {
            EventManager.RegisterClassHandler(typeof(Slider), ValueChangedEvent, ValueChangedClassHandler);
        }

        /// <summary>
        /// Create a new instance of slider.
        /// </summary>
        public Slider()
        {
            CanBeHitByUser = true;
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            DrawLayerNumber += 4; // track background, track foreground, ticks, thumb
        }

        /// <summary>
        /// Gets or sets the image to display as Track background.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider TrackBackgroundImage
        {
            get { return trackBackgroundImage; }
            set
            {
                if (trackBackgroundImage != null)
                    trackBackgroundImage.GetSprite().SizeChanged -= OnSizeChanged;

                trackBackgroundImage = value;
                InvalidateMeasure();

                if (trackBackgroundImage != null)
                    trackBackgroundImage.GetSprite().SizeChanged += OnSizeChanged;
            }
        }

        /// <summary>
        /// Gets or sets the image to display as Track foreground.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider TrackForegroundImage { get; set; }

        /// <summary>
        /// Gets or sets the image to display as slider thumb (button).
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider ThumbImage { get; set; }

        /// <summary>
        /// Gets or sets the image to display as slider thumb (button) when the mouse is over the slider.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider MouseOverThumbImage { get; set; }

        /// <summary>
        /// Gets or sets the image to display as tick.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider TickImage { get; set; }

        /// <summary>
        /// Gets or sets the smallest possible value of the slider.
        /// </summary>
        [DataMember]
        [DataMemberRange(0, float.MaxValue)]
        [DefaultValue(0.0f)]
        public float Minimum
        {
            get { return minimum; }
            set
            {
                if (value > Maximum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(Minimum)} should be lesser than or equal to {nameof(Maximum)}.");
                }

                minimum = value;
            }
        }

        /// <summary>
        /// Gets or sets the greatest possible value of the slider.
        /// </summary>
        [DataMember]
        [DataMemberRange(0, float.MaxValue)]
        [DefaultValue(1.0f)]
        public float Maximum
        {
            get { return maximum; }
            set
            {
                if (value < Minimum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(Maximum)} should be greater than or equal to {nameof(Minimum)}.");
                }

                maximum = value;
            }
        }

        /// <summary>
        /// Gets or sets the step of a <see cref="Value"/> change.
        /// </summary>
        [DataMember]
        [DataMemberRange(0, float.MaxValue)]
        [DefaultValue(0.1f)]
        public float Step { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets the current value of the slider.
        /// </summary>
        /// <remarks>value is truncated between <see cref="Minimum"/> and <see cref="Maximum"/></remarks>
        [DataMember]
        [DataMemberRange(0, float.MaxValue)]
        [DefaultValue(0.0f)]
        public float Value
        {
            get { return value; }
            set
            {
                var oldValue = Value;

                this.value = MathUtil.Clamp(value, Minimum, Maximum);
                if(ShouldSnapToTicks)
                    this.value = CalculateClosestTick(this.value);

                if(Math.Abs(oldValue - this.value) > MathUtil.ZeroTolerance)
                    RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
            }
        }

        /// <summary>
        /// Gets or sets the frequency of the ticks on the slider track.
        /// </summary>
        /// <remarks>Provided value is truncated to be greater or equal the 1</remarks>
        [DataMember]
        [DataMemberRange(1, float.MaxValue)]
        [Display(category: AppearanceCategory)]
        [DefaultValue(10.0f)]
        public float TickFrequency
        {
            get { return tickFrequency; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                tickFrequency = value;
                Value = this.value; // snap to tick if enabled
            }
        }

        /// <summary>
        /// Gets or sets the offset in virtual pixels between the center of the track and center of the ticks (for an not-stretched slider).
        /// </summary>
        [DataMember]
        [DataMemberRange(0, float.MaxValue)]
        [Display(category: AppearanceCategory)]
        [DefaultValue(10.0f)]
        public float TickOffset { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets the left/right offsets specifying where the track region starts. 
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        public Vector2 TrackStartingOffsets { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if the default direction of the slider should reversed or not.
        /// </summary>
        [DataMember]
        [Display(category: BehaviorCategory)]
        [DefaultValue(false)]
        public bool IsDirectionReversed { get; set; } = false;

        /// <summary>
        /// Gets or sets the value indicating if the ticks should be displayed or not.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(false)]
        public bool AreTicksDisplayed { get; set; } = false;

        /// <summary>
        /// Gets or sets the value indicating if the slider <see cref="Value"/> should be snapped to the ticks or not.
        /// </summary>
        [DataMember]
        [Display(category: BehaviorCategory)]
        [DefaultValue(false)]
        public bool ShouldSnapToTicks
        {
            get { return shouldSnapToTicks; }
            set 
            { 
                shouldSnapToTicks = value;
                Value = Value; // snap if enabled
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the slider.
        /// </summary>
        [DataMember]
        [Display(category: LayoutCategory)]
        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Snap the current <see cref="Value"/> to the closest tick.
        /// </summary>
        public void SnapToClosestTick()
        {
            Value = CalculateClosestTick(Value);
        }

        /// <summary>
        /// Calculate the value of the closest tick to the provided value.
        /// </summary>
        /// <param name="rawValue">The current raw value</param>
        /// <returns>The value adjusted to the closest tick</returns>
        protected float CalculateClosestTick(float rawValue)
        {
            var absoluteValue = rawValue - Minimum;
            var step = (Maximum - Minimum) / TickFrequency;
            var times = (float)Math.Round(absoluteValue / step);
            return times * step;
        }

        /// <summary>
        /// Increase the <see cref="Value"/> by <see cref="Step"/>.
        /// </summary>
        /// <remarks>If <see cref="ShouldSnapToTicks"/> is <value>True</value> then it increases of at least one tick.</remarks>
        public void Increase()
        {
            Value += CalculateIncreamentValue();
        }

        /// <summary>
        /// Decrease the <see cref="Value"/> by <see cref="Step"/>.
        /// </summary>
        /// <remarks>If <see cref="ShouldSnapToTicks"/> is <value>True</value> then it decreases of at least one tick.</remarks>
        public void Decrease()
        {
            Value -= CalculateIncreamentValue();
        }

        private float CalculateIncreamentValue()
        {
            return shouldSnapToTicks? Math.Max(Step, (Maximum - Minimum) / TickFrequency): Step;
        }
        
        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            var image = TrackBackgroundImage;
            if (image == null)
                return base.MeasureOverride(availableSizeWithoutMargins);

            var idealSize = image.GetSprite().SizeInPixels.Y;
            var desiredSize = new Vector3(idealSize, idealSize, 0)
            {
                [(int)Orientation] = availableSizeWithoutMargins[(int)Orientation]
            };

            return desiredSize;
        }

        /// <summary>
        /// Occurs when the value of the slider changed.
        /// </summary>
        /// <remarks>A ValueChanged event is bubbling</remarks>
        public event EventHandler<RoutedEventArgs> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ValueChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "ValueChanged",
            RoutingStrategy.Bubble,
            typeof(Slider));
        
        /// <summary>
        /// The class handler of the event <see cref="ValueChanged"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnValueChanged(RoutedEventArgs args)
        {

        }

        private static void ValueChangedClassHandler(object sender, RoutedEventArgs args)
        {
            var slider = (Slider)sender;

            slider.OnValueChanged(args);
        }

        protected override void OnTouchDown(TouchEventArgs args)
        {
            base.OnTouchDown(args);

            SetValueFromTouchPosition(args.WorldPosition);
        }

        protected override void OnTouchMove(TouchEventArgs args)
        {
            base.OnTouchMove(args);

            SetValueFromTouchPosition(args.WorldPosition);
        }

        internal override void OnKeyDown(KeyEventArgs args)
        {
            base.OnKeyDown(args);

            if(args.Key == Keys.Right)
                Increase();
            if(args.Key == Keys.Left)
                Decrease();
        }

        /// <summary>
        /// Set <see cref="Value"/> from the world position of a touch event.
        /// </summary>
        /// <param name="touchPostionWorld">The world position of the touch</param>
        protected void SetValueFromTouchPosition(Vector3 touchPostionWorld)
        {
            var axis = (int)Orientation;
            var offsets = TrackStartingOffsets;
            var elementSize = RenderSize[axis];
            var touchPosition = touchPostionWorld[axis] - WorldMatrixInternal[12 + axis] + elementSize/2;
            var ratio = (touchPosition - offsets.X) / (elementSize - offsets.X - offsets.Y);
            Value = (Orientation == Orientation.Vertical ^ IsDirectionReversed) ? 1 - ratio : ratio;
        }
    }
}
