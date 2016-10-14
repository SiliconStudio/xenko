// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("InputComponent")]
    [Display("Input Mapper", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(InputProcessor))]
    [ComponentOrder(11200)]
    public class InputComponent : EntityComponent
    {
        internal struct MadeBinding
        {
            public int Key;
            public InputEventHandler Handler;
        }

        public InputMapping InputMapping;
        internal InputMapper InputMapper;

        internal readonly List<MadeBinding> entityBindings = new List<MadeBinding>();

        public InputEventHandler AddHandler(int key, InputEventHandler handler)
        {
            if (InputMapper == null)
                throw new NullReferenceException("InputMapper not initialized");
            var ret = InputMapper.AddHandler(key, handler);
            entityBindings.Add(new MadeBinding { Key = key, Handler = handler });
            return ret;
        }
    }
}
