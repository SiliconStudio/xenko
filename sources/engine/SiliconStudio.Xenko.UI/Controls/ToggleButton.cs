// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represent a UI toggle button. A toggle but can have two or three states depending on the <see cref="IsThreeState"/> property.
    /// </summary>
    [DebuggerDisplay("ToggleButton - Name={Name}")]
    public class ToggleButton : ButtonBase
    {
        /// <summary>
        /// The key to the CheckedImagePropertyKey dependency property.
        /// </summary>
        public static readonly PropertyKey<ISpriteProvider> CheckedImagePropertyKey = new PropertyKey<ISpriteProvider>("CheckedImageModeKey", typeof(ToggleButton), ObjectInvalidationMetadata.New<ISpriteProvider>(OnToggleImageInvalidated));

        /// <summary>
        /// The key to the IndeterminateImagePropertyKey dependency property.
        /// </summary>
        public static readonly PropertyKey<ISpriteProvider> IndeterminateImagePropertyKey = new PropertyKey<ISpriteProvider>("IndeterminateImageModeKey", typeof(ToggleButton), ObjectInvalidationMetadata.New<ISpriteProvider>(OnToggleImageInvalidated));

        /// <summary>
        /// The key to the UncheckedImagePropertyKey dependency property.
        /// </summary>
        public static readonly PropertyKey<ISpriteProvider> UncheckedImagePropertyKey = new PropertyKey<ISpriteProvider>("UncheckedImageModeKey", typeof(ToggleButton), ObjectInvalidationMetadata.New<ISpriteProvider>(OnToggleImageInvalidated));

        private static void OnToggleImageInvalidated(object propertyOwner, PropertyKey propertyKey, object propertyOldValue)
        {
            var toggle = (ToggleButton)propertyOwner;
            toggle.OnToggleImageInvalidated();
        }

        /// <summary>
        /// Function triggered when one of the <see cref="CheckedImage"/>, <see cref="IndeterminateImage"/> and <see cref="UncheckedImage"/> images are invalidated.
        /// This function can be overridden in inherited classes.
        /// </summary>
        protected virtual void OnToggleImageInvalidated()
        {
        }

        private bool isThreeState;

        private ToggleState state;

        public ToggleButton()
        {
            DrawLayerNumber += 1; // (toggle design image)
            Padding = new Thickness(10, 5, 10, 7);
            State = ToggleState.UnChecked;
        }

        /// <summary>
        /// Gets or sets the image that the button displays when checked
        /// </summary>
        public ISpriteProvider CheckedImage
        {
            get { return DependencyProperties.Get(CheckedImagePropertyKey); }
            set { DependencyProperties.Set(CheckedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when indeterminate
        /// </summary>
        public ISpriteProvider IndeterminateImage
        {
            get { return DependencyProperties.Get(IndeterminateImagePropertyKey); }
            set { DependencyProperties.Set(IndeterminateImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when unchecked
        /// </summary>
        public ISpriteProvider UncheckedImage
        {
            get { return DependencyProperties.Get(UncheckedImagePropertyKey); }
            set { DependencyProperties.Set(UncheckedImagePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the state of the <see cref="ToggleButton"/>
        /// </summary>
        /// <remarks>Setting the state of the toggle button to <see cref="ToggleState.Indeterminate"/> sets <see cref="IsThreeState"/> to true.</remarks>
        public ToggleState State
        {
            get { return state; } 
            set
            {
                if(state == value)
                    return;

                state = value;

                switch (value)
                {
                    case ToggleState.Checked:
                        RaiseEvent(new RoutedEventArgs(CheckedEvent));
                        break;
                    case ToggleState.Indeterminate:
                        IsThreeState = true;
                        RaiseEvent(new RoutedEventArgs(IndeterminateEvent));
                        break;
                    case ToggleState.UnChecked:
                        RaiseEvent(new RoutedEventArgs(UncheckedEvent));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        /// <summary>
        /// Determines whether the control supports two or three states.
        /// </summary>
        /// <remarks>Setting <see cref="IsThreeState"/> to false changes the <see cref="State"/> of the toggle button if currently set to <see cref="ToggleState.Indeterminate"/></remarks>
        public bool IsThreeState
        {
            get { return isThreeState; }
            set
            {
                if(value == false && State == ToggleState.Indeterminate)
                    GoToNextState();

                isThreeState = value;
            }
        }

        /// <summary>
        /// Occurs when a <see cref="ToggleButton"/> is checked.
        /// </summary>
        /// <remarks>A checked event is bubbling</remarks>
        public event EventHandler<RoutedEventArgs> Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        /// <summary>
        /// Occurs when a <see cref="ToggleButton"/> is Indeterminate.
        /// </summary>
        /// <remarks>A Indeterminate event is bubbling</remarks>
        public event EventHandler<RoutedEventArgs> Indeterminate
        {
            add { AddHandler(IndeterminateEvent, value); }
            remove { RemoveHandler(IndeterminateEvent, value); }
        }

        /// <summary>
        /// Occurs when a <see cref="ToggleButton"/> is Unchecked.
        /// </summary>
        /// <remarks>A Unchecked event is bubbling</remarks>
        public event EventHandler<RoutedEventArgs> Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }

        /// <summary>
        /// Move the state of the toggle button to the next state. States order is: Unchecked -> Checked [-> Indeterminate] -> Unchecked -> ...
        /// </summary>
        protected void GoToNextState()
        {
            switch (State)
            {
                case ToggleState.Checked:
                    State = IsThreeState ? ToggleState.Indeterminate : ToggleState.UnChecked;
                    break;
                case ToggleState.Indeterminate:
                    State = ToggleState.UnChecked;
                    break;
                case ToggleState.UnChecked:
                    State = ToggleState.Checked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Identifies the <see cref="Checked"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> CheckedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "Checked",
            RoutingStrategy.Bubble,
            typeof(ToggleButton));

        /// <summary>
        /// Identifies the <see cref="Indeterminate"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> IndeterminateEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "Indeterminate",
            RoutingStrategy.Bubble,
            typeof(ToggleButton));

        /// <summary>
        /// Identifies the <see cref="Unchecked"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> UncheckedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>(
            "Unchecked",
            RoutingStrategy.Bubble,
            typeof(ToggleButton));

        protected override void OnClick(RoutedEventArgs args)
        {
            base.OnClick(args);

            GoToNextState();
        }
    }
}
