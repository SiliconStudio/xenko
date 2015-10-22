// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Design
{
    [DataContract]
    public class EntityComponentProperty
    {
        public EntityComponentProperty()
        {
            
        }

        public EntityComponentProperty(EntityComponentPropertyType type, string name, object value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        public EntityComponentPropertyType Type { get; set; }
        public string Name { get; set; }

        public object Value { get; set; }
    }
}