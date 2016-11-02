// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Core;
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_PLATFORM_UWP
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
#endif

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Maps <see cref="InputGesture"/>s to <see cref="InputAction"/>s. This allows the user to change what physical inputs map to a certain input gesture
    /// </summary>
    public class InputActionMapping
    {
        // NOTE: Gestures are compared by reference here since normally they are equal if they are monitoring the same thing
        private readonly HashSet<IInputGesture> gestures = new HashSet<IInputGesture>(ReferenceEqualityComparer<IInputGesture>.Default);
        private readonly List<InputAction> inputActions = new List<InputAction>();
        private readonly Dictionary<string, InputAction> inputActionsByName = new Dictionary<string, InputAction>();
        private readonly InputManager inputManager;

        public InputActionMapping(InputManager inputManager)
        {
            this.inputManager = inputManager;
        }

        /// <summary>
        /// Called before input update to reset gesture states
        /// </summary>
        public void Reset()
        {
            foreach (var action in inputActions)
            {
                // Update gestures
                foreach (var gesture in action.Gestures)
                    gesture.Reset();
            }
        }

        /// <summary>
        /// Updates the input gestures and send input action events
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since the last update</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Update actions
            foreach (var action in inputActions)
                action.Update();
        }

        /// <summary>
        /// Adds a new 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void AddBinding(string name, InputAction action)
        {
            if (action.ActionMapping != null || inputActions.Contains(action))
                throw new InvalidOperationException("Action was already added to a mapping");
            if (inputActionsByName.ContainsKey(name))
                throw new InvalidOperationException("Can't add binding, a binding with the same name already exists");

            // Bind action to mapper so that when the gesture changes later it will call this function again
            action.ActionMapping = this;
            action.MappingName = name;
            inputActions.Add(action);
            inputActionsByName.Add(name, action);

            // Add gestures to event routers
            foreach (var gesture in action.Gestures.OfType<InputGesture>())
            {
                gesture.ActionMapping = this;
                gesture.OnAdded();
            }
        }

        /// <summary>
        /// Removes an action binding
        /// </summary>
        /// <param name="action"></param>
        public void RemoveBinding(InputAction action)
        {
            if (action.ActionMapping == null || !inputActions.Contains(action))
                throw new InvalidOperationException("Action was not added to this mapping");

            // Remove gestures from event routers
            foreach (var gesture in action.Gestures.OfType<InputGesture>())
            {
                gesture.OnRemoved();
            }

            inputActions.Remove(action);
            inputActionsByName.Remove(action.MappingName);
            action.ActionMapping = null;
            action.MappingName = "";
        }

        /// <summary>
        /// Serializes gesture binding setup to a YAML configuration
        /// </summary>
        /// <param name="stream">A stream that receives the serialized YAML</param>
        public void SaveBindings(Stream stream)
        {
            // Collect all the gesture bindings into a key/value pair mapping
            Dictionary<string, List<InputGesture>> settings = new Dictionary<string, List<InputGesture>>();
            foreach (var pair in inputActionsByName)
            {
                settings.Add(pair.Key, pair.Value.Gestures.OfType<InputGesture>().ToList());
            }

#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_PLATFORM_UWP
            // Save the bindings
            YamlSerializer.GetSerializerSettings().PreferredIndent = 2;
            YamlSerializer.Serialize(stream, settings, settings.GetType(), SerializerContextSettings.Default, false);
#endif
        }

        /// <summary>
        /// Deserializes gesture binding setup from a YAML configuration
        /// </summary>
        /// <param name="stream">A stream that provides the YAML configuration</param>
        public bool LoadBindings(Stream stream)
        {
            Dictionary<string, List<InputGesture>> settings = new Dictionary<string, List<InputGesture>>();
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_PLATFORM_UWP
            try
            {
                settings = (Dictionary<string, List<InputGesture>>)YamlSerializer.Deserialize(stream, typeof(Dictionary<string, List<InputGesture>>), SerializerContextSettings.Default);
            }
            catch (YamlException)
            {
                return false;
            }
#endif

            // Load the new gesture bindings
            foreach (var pair in settings)
            {
                InputAction action;
                if (inputActionsByName.TryGetValue(pair.Key, out action))
                {
                    action.Gestures.Clear();
                    action.Gestures.AddRange(pair.Value);
                }
            }

            return true;
        }

        /// <summary>
        /// Removes all bindings that are currently bound
        /// </summary>
        public void ClearBindings()
        {
            InputAction[] actionsToRemove = inputActions.ToArray();
            foreach (var action in actionsToRemove)
            {
                RemoveBinding(action);
            }
            if (inputActionsByName.Count != 0) throw new Exception("Failed to correctly clear bindings");
            if (gestures.Count != 0) throw new Exception("Failed to correctly clear bindings");
            if (inputActions.Count != 0) throw new Exception("Failed to correctly clear bindings");
        }

        /// <summary>
        /// Adds a gesture as a listener to the input manager, if it has any specific event listener. Also adds the gesture to a list of registered gestures (so one is not used twice)
        /// </summary>
        /// <param name="gesture"></param>
        internal void AddInputGesture(IInputGesture gesture)
        {
            if (gestures.Contains(gesture)) throw new InvalidOperationException("Can't add input gesture, Gesture already registered");
            gestures.Add(gesture);
            
            IInputEventListener listener = gesture as IInputEventListener;
            if (listener != null) inputManager.AddListener(listener);
        }

        /// <summary>
        /// Removes the gesture as an event listener and removes it from the list of gesture registered with the action mapping
        /// </summary>
        /// <param name="gesture"></param>
        internal void RemoveInputGesture(IInputGesture gesture)
        {
            if (!gestures.Contains(gesture)) throw new InvalidOperationException("Can't remove input gesture, Gesture was never registered");
            gestures.Remove(gesture);

            IInputEventListener listener = gesture as IInputEventListener;
            if (listener != null) inputManager.RemoveListener(listener);
        }
    }
}