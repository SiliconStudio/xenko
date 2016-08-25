using System;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class SlotDefinition
    {
        public SlotDirection Direction { get; }

        public SlotKind Kind { get; }

        public string Name { get; }

        public virtual string Type => null;

        public virtual object ObjectValue => null;

        public SlotFlags Flags { get; }

        protected SlotDefinition(SlotDirection direction, SlotKind kind, string name, SlotFlags flags)
        {
            Direction = direction;
            Kind = kind;
            Name = name;
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
                Value = definition.ObjectValue,
                Flags = definition.Flags,
            };
        }

        public static SlotDefinition NewExecutionInput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Input, SlotKind.Execution, name, flags);
        }

        public static SlotDefinition NewExecutionOutput(string name, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition(SlotDirection.Output, SlotKind.Execution, name, flags);
        }

        public static SlotDefinition<T> NewValueInput<T>(string name, T value, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition<T>(SlotDirection.Input, SlotKind.Value, name, value, flags);
        }

        public static SlotDefinition<T> NewValueOutput<T>(string name, T value, SlotFlags flags = SlotFlags.None)
        {
            return new SlotDefinition<T>(SlotDirection.Output, SlotKind.Value, name, value, flags);
        }
    }

    public class SlotDefinition<T> : SlotDefinition
    {
        public override string Type => typeof(T).FullName;

        public override object ObjectValue => Value;

        public T Value { get; }

        internal SlotDefinition(SlotDirection direction, SlotKind kind, string name, T value, SlotFlags flags) : base(direction, kind, name, flags)
        {
            Value = value;
        }
    }
}