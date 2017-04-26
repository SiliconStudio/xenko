// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An object that can respond to actions from various input gestures (keyboard, mouse, touch, gamepad, etc.)
    /// </summary>
    [DataContract]
    public abstract class InputAction
    {
        internal InputActionMapping ActionMapping;
        private string mappingName;

        /// <summary>
        /// The name of the action, as registered in the action mapping
        /// </summary>
        /// <remarks>
        /// Changing the name of an action that has already been added to an <see cref="InputActionMapping"/> will trow an <see cref="InvalidOperationException"/>
        /// </remarks>
        public string MappingName
        {
            get { return mappingName; }
            set
            {
                // Lock mapping name after being added to action mapping
                if (ActionMapping != null) throw new InvalidOperationException("Can't change action name after it has been added to the action mapping");
                mappingName = value; 
            }
        }
        
        /// <summary>
        /// The gestures that are used for this action
        /// </summary>
        [DataMemberIgnore]
        public abstract IReadOnlyList<IInputGesture> ReadOnlyGestures { get; }

        /// <summary>
        /// Pre update of the input action
        /// </summary>
        public virtual void PreUpdate(TimeSpan deltaTime)
        {
        }

        /// <summary>
        /// Updates the input action
        /// </summary>
        public virtual void Update(TimeSpan deltaTime)
        {
        }

        /// <summary>
        /// Tries to add a gesture to this action
        /// </summary>
        /// <param name="gesture">A gesture to add</param>
        /// <returns><c>true</c> if successful; <c>false</c> if the gesture was not of the correct type for this action</returns>
        public abstract bool TryAddGesture(IInputGesture gesture);

        /// <summary>
        /// Removes all the gestures from this action
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Creates a copy of this action and all gestures with it
        /// </summary>
        /// <returns></returns>
        public InputAction Clone()
        {
            using (var memoryStream = new MemoryStream(4096))
            {
                var writer = new BinarySerializationWriter(memoryStream);
                var reader = new BinarySerializationReader(memoryStream);

                writer.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
                reader.Context.SerializerSelector = SerializerSelector.AssetWithReuse;

                // Serialize
                writer.SerializeExtended(this, ArchiveMode.Serialize);

                // Deserialize
                InputAction clone = null;
                memoryStream.Seek(0, SeekOrigin.Begin);
                reader.SerializeExtended(ref clone, ArchiveMode.Deserialize);

                return clone;
            }
        }

        /// <summary>
        /// Clones the list of input gestures used by this action
        /// </summary>
        /// <returns>A copy of this input action</returns>
        public List<IInputGesture> CloneGestures()
        {
            return Clone().ReadOnlyGestures.ToList();
        }

        /// <summary>
        /// Performs a foreach operation on every gesture on this action recursively
        /// </summary>
        /// <param name="action"></param>
        public void GestureForEach(Action<IInputGesture> action)
        {
            foreach (var rootGesture in ReadOnlyGestures)
            {
                var gestures = new List<IInputGesture>();
                ((InputGestureBase)rootGesture).GetGesturesRecursive(gestures);

                foreach (var gesture in gestures)
                {
                    action(gesture);
                }
            }
        }

        /// <summary>
        /// Called when a gesture should be linked to this action
        /// </summary>
        /// <param name="gesture"></param>
        protected abstract void OnGestureAdded(InputGestureBase gesture);

        /// <summary>
        /// Called when a gesture should be unlinked from this action
        /// </summary>
        /// <param name="gesture"></param>
        protected abstract void OnGestureRemoved(InputGestureBase gesture);

        protected void Gestures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var inputManager = ActionMapping?.InputManager;

            // Handles adding/removing new gestures to/from the action mapping when this action is registered as well
            var gesture = e.Item as InputGestureBase;
            if (gesture == null)
                return; // This might happen when adding a new gesture in the GameStudio

            if (ActionMapping == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnGestureAdded(gesture);
                    inputManager?.Gestures.Add(gesture);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnGestureRemoved(gesture);
                    inputManager?.Gestures.Remove(gesture);
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Gestures collection was modified but the action was not supported by the system.");

                case NotifyCollectionChangedAction.Move:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}