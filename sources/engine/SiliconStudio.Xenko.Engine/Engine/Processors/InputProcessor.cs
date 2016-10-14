// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class InputProcessor : EntityProcessor<InputComponent, InputProcessor.AssociatedData>
    {
        public class AssociatedData
        {
            public InputMapper inputMapper;
        }

        private readonly Dictionary<InputMapping, InputMapper> inputMappers = new Dictionary<InputMapping, InputMapper>();
        
        public override void Update(GameTime time)
        {
            // Update input mappers
            foreach (var pair in inputMappers)
            {
                pair.Value.Update((float)time.Elapsed.TotalSeconds);
            }
        }
        protected override AssociatedData GenerateComponentData(Entity entity, InputComponent component)
        {
            AssociatedData data = new AssociatedData();
            data.inputMapper = GetOrCreateInputMapper(component.InputMapping);
            return data;

        }
        private InputMapper GetOrCreateInputMapper(InputMapping mapping)
        {
            InputManager inputManager = (InputManager)Services.GetService(typeof(InputManager));

            InputMapper foundInputMapper;
            if (!inputMappers.TryGetValue(mapping, out foundInputMapper))
            {
                // Create and initialize a new input mapper
                foundInputMapper = new InputMapper(inputManager);
                inputMappers.Add(mapping, foundInputMapper);
            }

            return foundInputMapper;
        }
    }
}