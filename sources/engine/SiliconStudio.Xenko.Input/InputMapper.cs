// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input
{
    [DataContract]
    public struct InputMapperKeyType
    {
        public bool Equals(InputMapperKeyType other)
        {
            return string.Equals(AssemblyName, other.AssemblyName) && string.Equals(FullName, other.FullName);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InputMapperKeyType && Equals((InputMapperKeyType)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((AssemblyName != null ? AssemblyName.GetHashCode() : 0)*397) ^ (FullName != null ? FullName.GetHashCode() : 0);
            }
        }
        public override string ToString()
        {
            return FullName;
        }

        [DataMember]
        public string AssemblyName;
        [DataMember]
        public string FullName;
    }

    /// <summary>
    /// A class that contains event bindings for responding to certain input events
    /// </summary>
    public class InputEventHandler : IDisposable
    {
        public Action Pressed;
        public Action Released;
        public Action<float> NotNull;
        public Action<float> Changed;
        internal bool Disposed;
        private float lastValue;

        public void Dispose()
        {
            Disposed = true;
        }
        internal void Update(float input)
        {
            if (input != lastValue)
            {
                if (lastValue > 0.0f && input <= 0.0f)
                {
                    Released?.Invoke();
                }
                else if (lastValue <= 0.0f && input > 0.0f)
                {
                    Pressed?.Invoke();
                }
                Changed?.Invoke(input);
            }
            if (input != 0.0f)
                NotNull?.Invoke(input);
            lastValue = input;
        }
    }

    /// <summary>
    /// Class that maps inputs from various sources to events or axis values
    /// </summary>
    public class InputMapper : IDisposable
    {
        /// <summary>
        /// A single binding, which contains mappings to one or multiple physical devices. 
        /// Also keeps a list of handlers which respond to this binding in a certain way (changes, presses, releases, etc.)
        /// </summary>
        private class MappingData
        {
            /// <summary>
            /// The bindings mapped to this action
            /// </summary>
            public readonly HashSet<InputBinding> Bindings = new HashSet<InputBinding>();

            /// <summary>
            /// The button handlers for this action, receiving pressed/release events
            /// </summary>
            public readonly List<InputEventHandler> EventHandlers = new List<InputEventHandler>();

            /// <summary>
            /// The value that was last read from Update()
            /// </summary>
            public float LastValue;
        };

        private readonly InputManager inputManager;
        private readonly Dictionary<int, MappingData> mappings = new Dictionary<int, MappingData>();

        public InputMapper(InputManager inputManager)
        {
            this.inputManager = inputManager;
        }

        public void AddBinding(int key, InputBinding binding)
        {
            MappingData mappingData = GetMappingData(key);

            // Only add same input binding once
            if (!mappingData.Bindings.Contains(binding))
            {
                mappingData.Bindings.Add(binding);
            }
        }
        public void RemoveBinding(int key, InputBinding binding)
        {
            MappingData mappingData = GetMappingData(key);

            if (mappingData.Bindings.Contains(binding))
            {
                mappingData.Bindings.Remove(binding);
            }
        }
        public void ClearBindings(int key)
        {
            MappingData mappingData = GetMappingData(key);
            mappingData.Bindings.Clear();
        }

        // TODO: Allow binding to entity?
        public InputEventHandler AddHandler(int key, InputEventHandler handler)
        {
            GetMappingData(key).EventHandlers.Add(handler);
            return handler;
        }

        /// <summary>
        /// Checks the current state of a virtual button
        /// </summary>
        /// <param name="key">The mapping to check</param>
        /// <returns></returns>
        public bool GetButton(int key)
        {
            var mappingData = GetMappingData(key);
            return mappingData.LastValue > 0.0f;
        }

        /// <summary>
        /// Checks the current state of a virtual axis
        /// </summary>
        /// <param name="key">The mapping to check</param>
        /// <returns></returns>
        public float GetAxis(int key)
        {
            var mappingData = GetMappingData(key);
            return mappingData.LastValue;
        }

        public void Update(float deltaTime)
        {
            foreach (var pair in mappings)
            {
                var action = pair.Value;

                // Gather inputs
                float largestInput = 0.0f;
                foreach (var binding in action.Bindings)
                {
                    float bindingResult = binding.GetValue(inputManager) * binding.Sensitivity;

                    // Apply delta time to non-relative input
                    if (!binding.IsRelative)
                        bindingResult *= deltaTime;

                    if (Math.Abs(bindingResult) > Math.Abs(largestInput))
                        largestInput = bindingResult;
                }

                // Process events
                for (int i = 0; i < action.EventHandlers.Count;)
                {
                    var handler = action.EventHandlers[i];
                    if (handler.Disposed)
                    {
                        // Remove disposed handlers
                        action.EventHandlers.RemoveAt(i);
                        continue;
                    }

                    handler.Update(largestInput);

                    i++;
                }

                // Store value
                action.LastValue = largestInput;
            }
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        private MappingData GetMappingData(int key)
        {
            MappingData mappingData;
            if (!mappings.TryGetValue(key, out mappingData))
            {
                mappingData = new MappingData();
                mappings.Add(key, mappingData);
            }
            return mappingData;
        }
    }


    /// <summary>
    /// Class that maps inputs from various sources to events or axis values
    /// </summary>
    /// <typeparam name="TEnum">The enum that is used to identify mappings</typeparam>
    public class InputMapper<TEnum> : InputMapper where TEnum : IConvertible
    {
        public InputMapper(InputManager inputManager) : base(inputManager)
        {
        }
        public void AddBinding(TEnum key, InputBinding binding)
        {
            AddBinding(key.ToInt32(null), binding);
        }
        public void RemoveBinding(TEnum key, InputBinding binding)
        {
            RemoveBinding(key.ToInt32(null), binding);
        }
        public void ClearBindings(TEnum key)
        {
            ClearBindings(key.ToInt32(null));
        }

        // TODO: Allow binding to entity?
        public InputEventHandler AddHandler(TEnum key, InputEventHandler handler)
        {
            return AddHandler(key.ToInt32(null), handler);
        }

        /// <summary>
        /// Checks the current state of a virtual button
        /// </summary>
        /// <param name="key">The mapping to check</param>
        /// <returns></returns>
        public bool GetButton(TEnum key)
        {
            return GetButton(key.ToInt32(null));
        }

        /// <summary>
        /// Checks the current state of a virtual axis
        /// </summary>
        /// <param name="key">The mapping to check</param>
        /// <returns></returns>
        public float GetAxis(TEnum key)
        {
            return GetAxis(key.ToInt32(null));
        }
    }
}