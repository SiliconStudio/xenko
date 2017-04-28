// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Input.Gestures;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract]
    [Display("Input Action Mapping", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(InputProcessor), ExecutionMode = ExecutionMode.Runtime)]
    [ComponentOrder(400)]
    public class InputComponent : EntityComponent
    {
        private InputActionConfiguration defaultInputActionConfiguration;
        
        /// <summary>
        /// The default configuration used for this input component
        /// </summary>
        public InputActionConfiguration DefaultInputActionConfiguration
        {
            get { return defaultInputActionConfiguration; }
            set { defaultInputActionConfiguration = value; DefaultConfigurationChanged?.Invoke(this, null); }
        }

        /// <summary>
        /// A set of gestures that is already used in this action mapping
        /// </summary>
        [DataMemberIgnore]
        public HashSet<IInputGesture> BoundGestures => ActionMappingInternal != null ? 
            new HashSet<IInputGesture>(ActionMappingInternal.BoundGestures) : 
            new HashSet<IInputGesture>();

        /// <summary>
        /// The actual action mapping that is used internally
        /// </summary>
        [DataMemberIgnore]
        internal InputActionMapping ActionMappingInternal { get; set; }

        /// <summary>
        /// Raised when the configuration is changed
        /// </summary>
        internal event EventHandler DefaultConfigurationChanged;

        /// <summary>
        /// Retrieve an input action by name, or null if one does not exist with that name in the <see cref="Input.Mapping.InputActionConfiguration"/>
        /// </summary>
        /// <remarks>The bindings of an action can be freely modified to customize the bindings</remarks>
        public InputAction TryGetAction(string name)
        {
            if (ActionMappingInternal == null)
                return null;
            return ActionMappingInternal.TryGetAction(name);
        }

        /// <summary>
        /// Composes a list of current bindings as a <see cref="Input.Mapping.InputActionConfiguration"/>
        /// </summary>
        /// <returns>A list of bindings mapped to their action name</returns>
        public InputActionConfiguration GetBindings()
        {
            return new InputActionConfiguration()
            {
                Actions = ActionMappingInternal?.Actions.Select(x=>x.Clone()).ToList() ?? new List<InputAction>()
            };
        }

        /// <summary>
        /// Restores the bindings from <see cref="Input.Mapping.InputActionConfiguration"/>
        /// </summary>
        /// <remarks>This will clear all the original bindings</remarks>
        /// <param name="bindings">the bindings to restore</param>
        public void RestoreBindings(InputActionConfiguration bindings)
        {
            if (ActionMappingInternal == null) return;
            foreach (var defaultAction in bindings.Actions)
            {
                // For each action, restore the gestures.
                //  this will leave all bindings to actions intact and just restore default gesture bindings
                var action = ActionMappingInternal.TryGetAction(defaultAction.MappingName);
                var defaultGestures = defaultAction.CloneGestures();
                action.Clear();
                foreach(var gesture in defaultGestures)
                    action.TryAddGesture(gesture);
            }
        }

        /// <summary>
        /// Restores the bindings back to the list of default bindings that were configured in the GameStudio
        /// </summary>
        public void RestoreDefaults()
        {
            RestoreBindings(DefaultInputActionConfiguration);
        }
    }
}