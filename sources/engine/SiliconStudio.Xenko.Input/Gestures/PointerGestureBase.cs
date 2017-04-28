// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public abstract class PointerGestureBase : InputGestureBase, IInputEventListener<PointerEvent>
    {
        protected int RestrictedFingerCount;
        protected readonly Dictionary<int, Vector2> FingerIdToBeginPositions = new Dictionary<int, Vector2>();
        protected readonly Dictionary<int, Vector2> FingerIdsToLastPos = new Dictionary<int, Vector2>();
        protected TimeSpan ElapsedSinceBeginning;
        protected TimeSpan ElapsedSinceLast;
        protected TimeSpan DeltaTime;
        protected IPointerDevice PointerDevice;
        private readonly Dictionary<int, Vector2> fingerIdsToLastMovePos = new Dictionary<int, Vector2>();
        private static readonly List<int> FingerIdsCache = new List<int>();
        private bool hasGestureStarted;
        private float screenRatio;
        private int requiredFingerCount;

        // Keeps a list of events generated this frame
        private List<PointerGestureEventArgs> events = new List<PointerGestureEventArgs>();
        
        /// <summary>
        /// The list of events that were triggered on this gesture since the last frame
        /// </summary>
        public IReadOnlyList<PointerGestureEventArgs> Events => events;

        /// <summary>
        /// This value represents the required number of simultaneous finger to tap to trigger the gesture. For example: 1 for single finger, and so on...
        /// </summary>
        /// <remarks>This value is strictly positive.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the allowed range.</exception>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public int RequiredFingerCount
        {
            get { return requiredFingerCount; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");

                if (RestrictedFingerCount != 0 && value != RestrictedFingerCount)
                    throw new ArgumentOutOfRangeException("value");

                requiredFingerCount = value;
            }
        }

        protected virtual int CurrentFingerCount => FingerIdsToLastPos.Count;

        public event EventHandler<PointerGestureEventArgs> Changed;

        public override void PreUpdate(TimeSpan elapsedTime)
        {
            base.PreUpdate(elapsedTime);
            ElapsedSinceBeginning += elapsedTime;
            ElapsedSinceLast += elapsedTime;
            DeltaTime = elapsedTime;
            events.Clear();
        }

        public virtual void ProcessEvent(PointerEvent pointerEvent)
        {
            var eventType = pointerEvent.EventType;
            var id = pointerEvent.PointerId;
            var pos = pointerEvent.Position;

            switch (eventType)
            {
                case PointerEventType.Pressed:
                    ProcessDownEventPointer(id, UnnormalizeVector(pos));
                    break;
                case PointerEventType.Moved:
                    if (pointerEvent.IsDown)
                    {
                        // just memorize the last position to avoid useless processing on move events
                        fingerIdsToLastMovePos[id] = pos;
                    }
                    break;
                case PointerEventType.Released:
                    // process previous move events
                    ProcessAndClearMovePointerEvents();

                    // process the up event
                    ProcessUpEventPointer(id, UnnormalizeVector(pos));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // process move events not followed by an 'up' event
            ProcessAndClearMovePointerEvents();
        }

        protected void SendChangedEvent(PointerGestureEventArgs args)
        {
            Changed?.Invoke(this, args);
            events.Add(args);
        }

        protected abstract void ProcessDownEventPointer(int id, Vector2 pos);

        protected abstract void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos);

        protected abstract void ProcessUpEventPointer(int id, Vector2 pos);

        protected internal override void OnAdded()
        {
            base.OnAdded();

            SelectPointerDevice(InputManager.Pointer);
            InputManager.DeviceAdded += InputManagerOnDeviceChanged;
            InputManager.DeviceRemoved += InputManagerOnDeviceChanged;
        }

        protected internal override void OnRemoved()
        {
            SelectPointerDevice(null);
            InputManager.DeviceAdded -= InputManagerOnDeviceChanged;
            InputManager.DeviceRemoved -= InputManagerOnDeviceChanged;

            base.OnRemoved();
        }

        /// <summary>
        /// Called when the current recoginition should be cancelled, due to device changes or surface size changed, etc.
        /// </summary>
        protected virtual void CancelRecognition()
        {
            FingerIdsCache.Clear();
            fingerIdsToLastMovePos.Clear();
        }

        protected Vector2 ComputeMeanPosition(IEnumerable<Vector2> positions)
        {
            var count = 0;
            var accuPos = Vector2.Zero;
            foreach (var position in positions)
            {
                accuPos += position;
                ++count;
            }

            return accuPos / count;
        }
        
        protected Vector2 NormalizeVector(Vector2 inputVector)
        {
            return screenRatio > 1 ?
                new Vector2(inputVector.X, inputVector.Y * screenRatio) :
                new Vector2(inputVector.X / screenRatio, inputVector.Y);
        }

        protected Vector2 UnnormalizeVector(Vector2 inputVector)
        {
            return screenRatio > 1 ?
                new Vector2(inputVector.X, inputVector.Y / screenRatio) :
                new Vector2(inputVector.X * screenRatio, inputVector.Y);
        }

        private void ProcessAndClearMovePointerEvents()
        {
            if (fingerIdsToLastMovePos.Count > 0)
            {
                FingerIdsCache.Clear();
                FingerIdsCache.AddRange(fingerIdsToLastMovePos.Keys);

                // Unnormalizes vectors here before utilization
                foreach (var id in FingerIdsCache)
                    fingerIdsToLastMovePos[id] = UnnormalizeVector(fingerIdsToLastMovePos[id]);

                ProcessMoveEventPointers(fingerIdsToLastMovePos);
                fingerIdsToLastMovePos.Clear();
            }
        }

        private void InputManagerOnDeviceChanged(object sender, DeviceChangedEventArgs deviceChangedEventArgs)
        {
            SelectPointerDevice(InputManager.Pointer);
        }

        private void SelectPointerDevice(IPointerDevice pointerDevice)
        {
            if (PointerDevice == pointerDevice)
                return;

            CancelRecognition();

            if (PointerDevice != null)
            {
                PointerDevice.SurfaceSizeChanged -= PointerDeviceOnSurfaceSizeChanged;
            }

            PointerDevice = pointerDevice;
            if (PointerDevice != null)
            {
                screenRatio = PointerDevice.SurfaceAspectRatio;
                PointerDevice.SurfaceSizeChanged += PointerDeviceOnSurfaceSizeChanged;
            }
        }

        private void PointerDeviceOnSurfaceSizeChanged(object sender, SurfaceSizeChangedEventArgs eventArgs)
        {
            CancelRecognition();
            screenRatio = PointerDevice.SurfaceAspectRatio;
        }

        protected bool HasGestureStarted
        {
            get { return hasGestureStarted; }
            set
            {
                if (value && !hasGestureStarted)
                {
                    ElapsedSinceBeginning = TimeSpan.Zero;
                    ElapsedSinceLast = TimeSpan.Zero;
                }

                hasGestureStarted = value;
            }
        }
    }
}