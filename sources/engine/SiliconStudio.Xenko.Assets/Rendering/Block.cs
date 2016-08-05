using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract]
    public abstract class Block : IIdentifiable, IAssetPartDesign<Block>
    {
        protected Block()
        {
            Id = Guid.NewGuid();
        }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the base entity in case of prefabs. If null, the entity is not a prefab.
        /// </summary>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group in case of prefabs. If null, the entity doesn't belong to a part.
        /// </summary>
        [DataMember(-80), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BasePartInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the position of the block.
        /// </summary>
        [DataMember(-50), Display(Browsable = false)]
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets the title of that node, as it will be displayed in the editor.
        /// </summary>
        [DataMemberIgnore]
        public virtual string Title => null;

        /// <summary>
        /// Gets the list of slots this block has.
        /// </summary>
        [DataMemberIgnore]
        public ObservableCollection<Slot> Slots { get; } = new ObservableCollection<Slot>();

        public abstract void RegenerateSlots();

        /// <inheritdoc/>
        Block IAssetPartDesign<Block>.Part => this;
    }

    public class FunctionStartBlock : Block
    {
        public override string Title => $"{FunctionName} Start";

        public string FunctionName { get; set; }

        public override void RegenerateSlots()

        {
            Slots.Clear();
            Slots.Add(new Slot { Type = SlotType.Execution, Direction = SlotDirection.Output, Name = "Start" });
        }
    }

    public class ConditionalBranch : Block
    {
        public override string Title => "Condition";

        public override void RegenerateSlots()

        {
            Slots.Clear();
            Slots.Add(new Slot { Type = SlotType.Execution, Direction = SlotDirection.Input });
            Slots.Add(new Slot { Type = SlotType.Value, Direction = SlotDirection.Input, Name = "Condition" });
            Slots.Add(new Slot { Type = SlotType.Execution, Direction = SlotDirection.Output, Name = "True" });
            Slots.Add(new Slot { Type = SlotType.Execution, Direction = SlotDirection.Output, Name = "False" });
        }
    }

    public enum SlotType
    {
        Execution = 0,
        Value = 1,
    }

    public enum SlotDirection
    {
        Input = 0,
        Output = 1,
    }

    public class Slot
    {
        public string Name { get; set; }

        public SlotType Type { get; set; }

        public SlotDirection Direction { get; set; }
    }
}