using System;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class SlotDefinition
    {
        public SlotDirection Direction { get; }

        public SlotKind Kind { get; }

        public string Name { get; }

        public string Value { get; }

        public SlotFlags Flags { get; }

        protected SlotDefinition(SlotDirection direction, SlotKind kind, string name, string value, SlotFlags flags)
        {
            Direction = direction;
            Kind = kind;
            Name = name;
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
                Value = definition.Value,
                Flags = definition.Flags,
            };
        }

        public static SlotDefinition NewExecutionInput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Input, SlotKind.Execution, name, null, flags);
        }

        public static SlotDefinition NewExecutionOutput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Output, SlotKind.Execution, name, null, flags);
        }

        public static SlotDefinition NewValueInput(string name, string value, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Input, SlotKind.Value, name, value, flags);
        }

        public static SlotDefinition NewValueOutput(string name, string value, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Output, SlotKind.Value, name, value, flags);
        }
    }
}