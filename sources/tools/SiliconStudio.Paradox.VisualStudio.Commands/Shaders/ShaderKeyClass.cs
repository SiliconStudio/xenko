// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.VisualStudio.Commands.Shaders
{
    class ShaderKeyClass
    {
        public ShaderKeyClass(string name)
        {
            Name = name;
            Variables = new List<ShaderKeyVariable>();
        }

        public string Name { get; private set; }
        public List<ShaderKeyVariable> Variables { get; private set; }
    }

    enum ShaderKeyVariableCategory
    {
        Value,
        ArrayValue,
        Resource,
    }

    class ShaderKeyVariable
    {
        public ShaderKeyVariable(string name, string type, ShaderKeyVariableCategory category)
        {
            Name = name;
            Type = type;
            Category = category;
        }

        public string Name { get; private set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
        public string Map { get; set; }
        public ShaderKeyVariableCategory Category { get; set; }
    }
}