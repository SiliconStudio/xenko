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
    [Display("Input Component", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(InputProcessor))]
    [ComponentOrder(11200)]
    public class InputComponent : EntityComponent
    {
        public InputMapping InputMapping;
        internal InputMapper InputMapper;
    }
}
