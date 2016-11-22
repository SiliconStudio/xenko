// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that triggers on a certain condition
    /// </summary>
    [DataContract]
    public class ButtonAction : InputAction
    {
        private readonly List<ButtonGestureEventArgs> events = new List<ButtonGestureEventArgs>();
        private ButtonState lastState;

        public ButtonAction()
        {
            Gestures.CollectionChanged += Gestures_CollectionChanged;
        }

        /// <summary>
        /// Last state of the button
        /// </summary>
        public ButtonState LastState => lastState;

        public TrackingCollection<IButtonGesture> Gestures { get; } = new TrackingCollection<IButtonGesture>();

        [DataMemberIgnore]
        public override IReadOnlyList<IInputGesture> ReadOnlyGestures => Gestures;

        /// <summary>
        /// Raised when the action was trigerred
        /// </summary>
        public event EventHandler<ButtonGestureEventArgs> Changed;

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

            // Only send the last event
            if (events.Count > 0)
            {
                var evt = events.Last();
                lastState = evt.State;
                Changed?.Invoke(this, evt);
            }
            events.Clear();
        }

        public override string ToString()
        {
            return $"Button Action \"{MappingName}\", {nameof(LastState)}: {LastState}";
        }

        public override bool TryAddGesture(IInputGesture gesture)
        {
            var item = gesture as IButtonGesture;
            if (item != null)
            {
                Gestures.Add(item);
                return true;
            }
            return false;
        }

        public override void Clear()
        {
            Gestures.Clear();
        }

        protected override void OnGestureAdded(InputGestureBase gesture)
        {
            var button = gesture as IButtonGesture;
            button.Changed += ButtonOnChanged;
        }

        protected override void OnGestureRemoved(InputGestureBase gesture)
        {
            var button = gesture as IButtonGesture;
            button.Changed -= ButtonOnChanged;
        }

        private void ButtonOnChanged(object sender, ButtonGestureEventArgs args)
        {
            events.Add(new ButtonGestureEventArgs(args.Device, args.State));
        }
    }
}