// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Should manage assigning the correct <see cref="InputActionMapping"/> to the correct <see cref="InputComponent"/>. 
    /// this processor should also take care of loading the user specified settings and saving back changed settings to some storage 
    /// that is persistent between game launches
    /// </summary>
    public class InputProcessor : EntityProcessor<InputComponent, InputProcessor.AssociatedData>
    {
        private InputManager inputManager;
        private readonly Dictionary<InputActionConfiguration, LoadedData> loadedActionMappings = new Dictionary<InputActionConfiguration, LoadedData>();

        public override void Update(GameTime time)
        {
            base.Update(time);

            // Update loaded mappings on components in case they change
            foreach (var pair in ComponentDatas)
            {
                if (pair.Key.DefaultInputActionConfiguration != pair.Value.ActionConfiguration)
                {
                    pair.Value.ActionConfiguration = pair.Key.DefaultInputActionConfiguration;

                    // Load new action mapping
                    var loadedData = GetOrCreateInputActionMapping(pair.Key.DefaultInputActionConfiguration);
                    pair.Key.ActionMappingInternal = loadedData.ActionMapping;
                    if (pair.Value.LoadedMapping != null) pair.Value.LoadedMapping.Release();
                    pair.Value.LoadedMapping = loadedData;
                    if (loadedData != null) loadedData.AddReference();
                }
            }

            // Update every input mapping
            foreach (var data in loadedActionMappings.Values)
            {
                data.ActionMapping.Update(time.Elapsed);
            }
        }

        protected internal override void OnSystemAdd()
        {
            base.OnSystemAdd();

            // Retrieve the input manager
            inputManager = (InputManager)Services.GetService(typeof(InputManager));
        }

        protected internal override void OnSystemRemove()
        {
            base.OnSystemRemove();

            foreach (var data in loadedActionMappings)
            {
                data.Value.Release();
            }
            loadedActionMappings.Clear();
        }

        protected override AssociatedData GenerateComponentData(Entity entity, InputComponent component)
        {
            var data = new AssociatedData()
            {
                ActionConfiguration = component.DefaultInputActionConfiguration,
                LoadedMapping = GetOrCreateInputActionMapping(component.DefaultInputActionConfiguration),
            };
            if (data.LoadedMapping != null)
            {
                data.LoadedMapping.AddReference();
                component.ActionMappingInternal = data.LoadedMapping.ActionMapping;
            }
            return data;
        }

        private LoadedData GetOrCreateInputActionMapping(InputActionConfiguration inputActionConfiguration)
        {
            if (inputActionConfiguration == null)
                return null;

            LoadedData loadedData;
            if (!loadedActionMappings.TryGetValue(inputActionConfiguration, out loadedData))
            {
                var actionMapping = new InputActionMapping(inputManager);
                loadedData = new LoadedData { ActionMapping = actionMapping };
                loadedActionMappings.Add(inputActionConfiguration, loadedData);

                // Add all the default actions to the mapping
                foreach (var action in inputActionConfiguration.Actions)
                {
                    // Clone the action so the defaults can be restored at any point
                    var actionClone = action.Clone();
                    actionMapping.AddAction(actionClone);
                }
            }
            return loadedData;
        }

        public class LoadedData : IReferencable
        {
            public InputActionMapping ActionMapping;

            private int referenceCount;

            public int ReferenceCount
            {
                get { return referenceCount; }
            }

            public int AddReference()
            {
                return ++referenceCount;
            }

            public int Release()
            {
                --referenceCount;
                if (referenceCount == 0)
                {
                    ActionMapping.Dispose();
                    ActionMapping = null;
                }
                return referenceCount;
            }
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
            public LoadedData LoadedMapping;
        }
    }
}