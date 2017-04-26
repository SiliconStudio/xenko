// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Should manage assigning the correct <see cref="InputActionMapping"/> to the correct <see cref="InputComponent"/>.
    /// </summary>
    public class InputProcessor : EntityProcessor<InputComponent, InputProcessor.AssociatedData>
    {
        private InputManager inputManager;

        public override void Update(GameTime time)
        {
            base.Update(time);
            
            // Update every input mapping
            foreach (var pair in ComponentDatas)
            {
                var mapping = pair.Value.LoadedMapping;
                mapping?.Update(time.Elapsed);
            }
        }

        protected internal override void OnSystemAdd()
        {
            base.OnSystemAdd();

            // Retrieve the input manager
            inputManager = Services.GetSafeServiceAs<InputManager>();
        }

        protected internal override void OnSystemRemove()
        {
            foreach (var data in ComponentDatas.Values)
            {
                data.LoadedMapping?.Dispose();
            }

            base.OnSystemRemove();
        }

        protected override AssociatedData GenerateComponentData(Entity entity, InputComponent component)
        {
            var data = new AssociatedData
            {
                ActionConfiguration = component.DefaultInputActionConfiguration,
                LoadedMapping = CreateInputActionMapping(component.DefaultInputActionConfiguration),
            };

            return data;
        }

        protected override void OnEntityComponentAdding(Entity entity, InputComponent component, AssociatedData data)
        {
            component.ActionMappingInternal = data.LoadedMapping;
            component.DefaultConfigurationChanged += Component_DefaultConfigurationChanged;
        }

        protected override void OnEntityComponentRemoved(Entity entity, InputComponent component, AssociatedData data)
        {
            component.DefaultConfigurationChanged -= Component_DefaultConfigurationChanged;
            data.LoadedMapping?.Dispose();
        }

        private void Component_DefaultConfigurationChanged(object sender, EventArgs e)
        {
            var changedComponent = (InputComponent)sender;
            if (changedComponent == null)
                throw new InvalidOperationException();

            UpdateInputComponent(changedComponent);
        }

        private void UpdateInputComponent(InputComponent component)
        {
            var data = ComponentDatas[component];
            data.ActionConfiguration = component.DefaultInputActionConfiguration;

            // Load new action mapping
            var loadedData = CreateInputActionMapping(component.DefaultInputActionConfiguration);
            component.ActionMappingInternal = loadedData;
            data.LoadedMapping?.Dispose();
            data.LoadedMapping = loadedData;
        }

        private InputActionMapping CreateInputActionMapping(InputActionConfiguration inputActionConfiguration)
        {
            if (inputActionConfiguration == null)
                return null;

            var actionMapping = new InputActionMapping(inputManager);

            // Add all the default actions to the mapping
            foreach (var action in inputActionConfiguration.Actions)
            {
                // Clone the action so the defaults can be restored at any point
                var actionClone = action.Clone();
                actionMapping.AddAction(actionClone);
            }

            return actionMapping;
        }
        
        public class AssociatedData
        {
            /// <summary>
            /// The template that is used to create this action mapping
            /// </summary>
            public InputActionConfiguration ActionConfiguration;

            /// <summary>
            /// The actual action mapping currently being used
            /// </summary>
            public InputActionMapping LoadedMapping;
        }
    }
}