// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal abstract class GestureRecognizer
    {
        protected virtual GestureConfig Config { get; private set; }

        protected List<GestureEvent> CurrentGestureEvents = new List<GestureEvent>();

        protected readonly Dictionary<int, Vector2> FingerIdToBeginPositions = new Dictionary<int, Vector2>();

        protected readonly Dictionary<int, Vector2> FingerIdsToLastPos = new Dictionary<int, Vector2>();

        protected TimeSpan ElapsedSinceBeginning;
        protected TimeSpan ElapsedSinceLast;

        private readonly static List<int> FingerIdsCache = new List<int>();

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
        private bool hasGestureStarted;

        protected virtual int NbOfFingerOnScreen
        {
            get { return FingerIdsToLastPos.Count; }
        }

        internal float ScreenRatio { get; set; }

        protected GestureRecognizer(GestureConfig config, float screenRatio)
        {
            Config = config;
            ScreenRatio = screenRatio;
        }

        public List<GestureEvent> ProcessPointerEvents(TimeSpan deltaTime, List<PointerEvent> events)
        {
            CurrentGestureEvents.Clear();

            ElapsedSinceBeginning += deltaTime;
            ElapsedSinceLast += deltaTime;

            ProcessPointerEventsImpl(deltaTime, events);

            return CurrentGestureEvents;
        }

        protected virtual void ProcessPointerEventsImpl(TimeSpan deltaTime, List<PointerEvent> events)
        {
            AnalysePointerEvents(events);
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

        // avoid reallocation of the dictionary at each update call
        private readonly Dictionary<int, Vector2> fingerIdsToLastMovePos = new Dictionary<int, Vector2>(); 

        protected void AnalysePointerEvents(List<PointerEvent> events)
        {
            foreach (var pointerEvent in events)
            {
                var state = pointerEvent.State;
                var id = pointerEvent.PointerId;
                var pos = pointerEvent.Position;

                switch (state)
                {
                    case PointerState.Down:
                        ProcessDownEventPointer(id, UnnormalizeVector(pos));
                        break;
                    case PointerState.Move:
                        // just memorize the last position to avoid useless processing on move events
                        if (FingerIdToBeginPositions.ContainsKey(id))
                            fingerIdsToLastMovePos[id] = pos;
                        break;
                    case PointerState.Up:
                    case PointerState.Out:
                    case PointerState.Cancel:
                        // process previous move events
                        ProcessAndClearMovePointerEvents();

                        // process the up event
                        ProcessUpEventPointer(id, UnnormalizeVector(pos));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // process move events not followed by an 'up' event
            ProcessAndClearMovePointerEvents();
        }

        protected Vector2 NormalizeVector(Vector2 inputVector)
        {
            return ScreenRatio > 1 ? 
                new Vector2(inputVector.X, inputVector.Y * ScreenRatio) : 
                new Vector2(inputVector.X / ScreenRatio, inputVector.Y);
        }

        protected Vector2 UnnormalizeVector(Vector2 inputVector)
        {
            return ScreenRatio > 1 ? 
                new Vector2(inputVector.X, inputVector.Y / ScreenRatio) : 
                new Vector2(inputVector.X * ScreenRatio, inputVector.Y);
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

        protected abstract void ProcessDownEventPointer(int id, Vector2 pos);

        protected abstract void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos);

        protected abstract void ProcessUpEventPointer(int id, Vector2 pos);
    }
}
