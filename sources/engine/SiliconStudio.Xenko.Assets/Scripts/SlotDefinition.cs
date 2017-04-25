// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class SlotDefinition
    {
        public SlotDirection Direction { get; }

        public SlotKind Kind { get; }

        /// <summary>
        /// The name of this slot.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of this slot, only used as hint for input slots.
        /// </summary>
        public string Type { get; }

        public string Value { get; }

        public SlotFlags Flags { get; }

        protected SlotDefinition(SlotDirection direction, SlotKind kind, string name, string type, string value, SlotFlags flags)
        {
            Direction = direction;
            Kind = kind;
            Name = name;
            Type = type;
            Value = value;
            Flags = flags;
        }

        public static implicit operator Slot(SlotDefinition definition)
        {
            return new Slot
            {
                Direction = definition.Direction,
                Kind = definition.Kind,
                Name = definition.Name,
                Type = definition.Type,
                Value = definition.Value,
                Flags = definition.Flags,
            };
        }

        public static SlotDefinition NewExecutionInput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Input, SlotKind.Execution, name, null, null, flags);
        }

        public static SlotDefinition NewExecutionOutput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Output, SlotKind.Execution, name, null, null, flags);
        }

        public static SlotDefinition NewValueInput(string name, string type, string value, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Input, SlotKind.Value, name, type, value, flags);
        }

        public static SlotDefinition NewValueOutput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Output, SlotKind.Value, name, null, null, flags);
        }
    }
}
