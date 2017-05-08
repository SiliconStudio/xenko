// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A configuration that contains all actions that were configured on an InputActionMappingAsset in the GameStudio together with the default action bindings
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<InputActionConfiguration>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<InputActionConfiguration>), Profile = "Content")]
    public class InputActionConfiguration
    {
        /// <summary>
        /// Lists all the actions that this input mapping contains
        /// </summary>
        public List<InputAction> Actions { get; set; }

        /// <summary>
        /// Changes all the gamepad gestures on this configuration to a different controller index
        /// </summary>
        /// <param name="targetIndex">The index to change all gamepad gestures to</param>
        public void ShiftGamePadIndex(int targetIndex)
        {
            ProcessGestures(gesture =>
            {
                var gamePadGesture = gesture as IGamePadGesture;
                if (gamePadGesture != null)
                    gamePadGesture.GamePadIndex = targetIndex;
            });
        }

        /// <summary>
        /// Changes all the gamepad gestures on this configuration to a different controller index
        /// </summary>
        /// <param name="sourceIndex">The current index on the gestures</param>
        /// <param name="targetIndex">The index to change all matching gestures to</param>
        public void ShiftGamePadIndex(int sourceIndex, int targetIndex)
        {
            ProcessGestures(gesture =>
            {
                var gamePadGesture = gesture as IGamePadGesture;
                if (gamePadGesture != null && gamePadGesture.GamePadIndex == sourceIndex)
                    gamePadGesture.GamePadIndex = targetIndex;
            });
        }

        private void ProcessGestures(Action<InputGesture> action)
        {
            foreach (var inputAction in Actions)
            {
                inputAction.GestureForEach(action);
            }
        }
    }
}