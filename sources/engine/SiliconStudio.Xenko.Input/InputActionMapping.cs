// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Maps <see cref="InputGesture"/>s to <see cref="InputAction"/>s. This allows the user to change what physical inputs map to a certain input gesture
    /// </summary>
    public class InputActionMapping
    {
        private readonly Dictionary<Type, IInputEventRouter> eventRouters = new Dictionary<Type, IInputEventRouter>();
        private readonly HashSet<InputGesture> gestures = new HashSet<InputGesture>();
        private readonly List<InputAction> inputActions = new List<InputAction>();
        private readonly Dictionary<string, InputAction> actionsByName = new Dictionary<string, InputAction>();
        

        public InputActionMapping()
        {
            // Generate mappings from input event type to a class to processes these and sends them to the correct gestures that accept those
            TypeBasedRegistry<InputEvent> inputEventTypes = new TypeBasedRegistry<InputEvent>();
            var registerEventRouterMethod = GetType().GetMethod("RegisterEventRouter", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var type in inputEventTypes.GetAllTypes())
            {
                var genericMethod = registerEventRouterMethod.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }
        }

        /// <summary>
        /// Updates the input gestures and send input action events
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since the last update</param>
        /// <param name="inputEvents">A list of input events to process</param>
        public void Update(TimeSpan elapsedTime, IReadOnlyList<InputEvent> inputEvents)
        {
            foreach (var action in inputActions)
            {
                // Update gestures
                foreach (var gesture in action.Gestures)
                    gesture.Update(elapsedTime);
            }

            // Send events to input gestures
            foreach (var evt in inputEvents)
            {
                IInputEventRouter router;
                if (!eventRouters.TryGetValue(evt.GetType(), out router))
                    throw new InvalidOperationException($"The event type {evt.GetType()} was not registered with the input mapper and cannot be processed");
                router.RouteEvent(evt);
            }
            
            foreach (var action in inputActions)
                action.Update();
        }

        public void AddBinding(string name, InputAction action)
        {
            if (action.ActionMapping != null || inputActions.Contains(action))
                throw new InvalidOperationException("Action was already added to a mapping");
            if(actionsByName.ContainsKey(name))
                throw new InvalidOperationException("Can't add binding, a binding with the same name already exists");

            // Bind action to mapper so that when the gesture changes later it will call this function again
            action.ActionMapping = this;
            action.MappingName = name;
            inputActions.Add(action);
            actionsByName.Add(name, action);

            // Add gestures to event routers
            foreach (var gesture in action.Gestures)
            {
                AddInputGesture(gesture);
            }
        }

        public void RemoveBinding(InputAction action)
        {
            if(action.ActionMapping == null || !inputActions.Contains(action))
                throw new InvalidOperationException("Action was not added to this mapping");

            // Remove gestures from event routers
            foreach (var gesture in action.Gestures)
            {
                RemoveInputGesture(gesture);
            }

            inputActions.Remove(action);
            actionsByName.Remove(action.MappingName);
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
            foreach (var pair in actionsByName)
            {
                settings.Add(pair.Key, pair.Value.Gestures.ToList());
            }

            // Save the bindings
            YamlSerializer.GetSerializerSettings().PreferredIndent = 2;
            YamlSerializer.Serialize(stream, settings, settings.GetType(), SerializerContextSettings.Default, false);
        }

        /// <summary>
        /// Deserializes gesture binding setup from a YAML configuration
        /// </summary>
        /// <param name="stream">A stream that provides the YAML configuration</param>
        public bool LoadBindings(Stream stream)
        {
            Dictionary<string, List<InputGesture>> settings;
            try
            {
                settings = (Dictionary<string, List<InputGesture>>)YamlSerializer.Deserialize(stream, typeof(Dictionary<string, List<InputGesture>>), SerializerContextSettings.Default);
            }
            catch (YamlException)
            {
                return false;
            }

            // Load the new gesture bindings
            foreach (var pair in settings)
            {
                InputAction action;
                if (actionsByName.TryGetValue(pair.Key, out action))
                {
                    action.Gestures.Clear();
                    action.Gestures.AddRange(pair.Value);
                }
            }

            return true;
        }


        /// <summary>
        /// Registers an event type and adds an entry to the <see cref="eventRouters"/> map
        /// </summary>
        /// <typeparam name="TEventType">The event type that will get routed to the correct <see cref="InputGesture"/>s</typeparam>
        protected void RegisterEventRouter<TEventType>() where TEventType : InputEvent
        {
            var type = typeof(TEventType);
            eventRouters.Add(type, new InputEventRouter<TEventType>());
        }

        internal void AddInputGesture(InputGesture gesture)
        {
            var eventInterfaces = gesture.GetType().FindInterfaces((type, criteria) => type.IsGenericType && typeof(IInputEventListener<>) == type.GetGenericTypeDefinition(), gesture);
            var handledTypes = eventInterfaces.Select(x => x.GenericTypeArguments[0]);
            foreach (var type in handledTypes)
            {
                eventRouters[type].Listeners.Add(gesture);
            }
        }

        internal void RemoveInputGesture(InputGesture gesture)
        {
            foreach (var pair in eventRouters)
            {
                pair.Value.Listeners.Remove(gesture);
            }
        }

        private interface IInputEventRouter
        {
            HashSet<InputGesture> Listeners { get; }
            void RouteEvent(InputEvent evt);
        }

        private class InputEventRouter<TEventType> : IInputEventRouter where TEventType : InputEvent
        {
            public HashSet<InputGesture> Listeners { get; } = new HashSet<InputGesture>();
            public void RouteEvent(InputEvent evt)
            {
                foreach (var gesture in Listeners)
                {
                    ((IInputEventListener<TEventType>)gesture).ProcessEvent((TEventType)evt);
                }
            }
        }
    }
}