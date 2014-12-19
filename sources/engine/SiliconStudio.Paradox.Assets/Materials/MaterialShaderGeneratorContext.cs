// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Security.Policy;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderGeneratorContext
    {
        private int idCounter;
        public MaterialShaderGeneratorContext()
        {
            Stack = new Stack<StackOperations>();
        }

        public MaterialAsset FindMaterial(AssetReference<MaterialAsset> materialReference)
        {
            throw new NotImplementedException();
        }

        public Stack<StackOperations> Stack { get; private set; } 

        public StackOperations PushStack()
        {
            var stack = new StackOperations();
            Stack.Push(stack);
            return stack;
        }

        public StackOperations CurrentStack
        {
            get
            {
                return Stack.Peek();
            }
        }

        public void PopStack()
        {
            Stack.Pop();
        }

        public bool ExploreGenerics = false;

        public LoggerResult Log;

        public int NextId()
        {
            return idCounter++;
        }

        public class StackOperations
        {
            public StackOperations()
            {
                Operations = new List<ShaderSource>();
                Streams = new HashSet<string>();
            }

            public List<ShaderSource> Operations { get; private set; }

            public HashSet<string> Streams { get; private set; }

            public void UseStream(string stream)
            {
                if (!Streams.Contains(stream))
                {
                    var prepareMixin = new ShaderMixinSource();
                    prepareMixin.Mixins.Add(new ShaderClassSource("MaterialLayerStreamReset", stream));
                    Operations.Add(prepareMixin);
                }
                Streams.Add(stream);
            }

            public void AddBlendColor3(string stream, ShaderSource classSource)
            {
                // Use Stream before adding operations
                UseStream(stream);

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialLayerComputeColorFloat3Blend", stream));
                mixin.AddComposition("Float3Source", classSource);

                Operations.Add(mixin);
            }

            public void AddBlendColor(string stream, ShaderSource classSource)
            {
                // Use Stream before adding operations
                UseStream(stream);

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialLayerComputeColorFloatBlend", stream));
                mixin.AddComposition("FloatSource", classSource);

                Operations.Add(mixin);
            }


            public ShaderSource SquashOperations()
            {
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialLayerArray"));

                // Squash all operations into MaterialLayerArray
                foreach (var operation in Operations)
                {
                    mixin.AddCompositionToArray("layers", operation);
                }
                Operations.Clear();
                Streams.Clear();
                return mixin;
            }
        }



    }
}
