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

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An object that can respond to actions from various input gestures (keyboard,mouse,touch,gamepad,etc.)
    /// </summary>
    [DataContract]
    public abstract class InputAction
    {
        internal InputActionMapping ActionMapping;
        private string mappingName;
        
        protected InputAction()
        {
            Gestures.CollectionChanged += Gestures_CollectionChanged;
        }

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
                if(ActionMapping != null) throw new InvalidOperationException("Can't change action name after it has been added to the action mapping");
                mappingName = value; 
            }
        }

        /// <summary>
        /// <summary>
        /// Should mouse input be ignored when the mouse is not locked
        /// </summary>
        public bool IgnoreMouseWhenNotLocked { get; set; } = false;

        /// <summary>
        /// The gestures that are used for this action
        /// </summary>
        // TODO: Show only respective types of gestures
        public TrackingCollection<IInputGesture> Gestures { get; } = new TrackingCollection<IInputGesture>();

        /// <summary>
        /// Updates the input action, raising events whenever something changed
        /// </summary>
        public abstract void Update();

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
            return Clone().Gestures.ToList();
        }

        private void Gestures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Handles adding/removing new gestures to/from the action mapping when this action is registered as well
            var gesture = e.Item as InputGesture;
            if (gesture == null) return; // This might happen when adding a new gesture in the GameStudio
            if (ActionMapping == null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    gesture.ActionMapping = ActionMapping;
                    gesture.Action = this;
                    gesture.OnAdded();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    gesture.OnRemoved();
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