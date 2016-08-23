using System;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    public class Slot : IIdentifiable
    {
        public Slot()
        {
            Id = Guid.NewGuid();
        }

        [DataMemberIgnore]
        public Block Owner { get; internal set; }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        [DataMember(0)]
        public SlotDirection Direction { get; set; }

        [DataMember(10)]
        [DefaultValue(SlotKind.Value)]
        public SlotKind Kind { get; set; }

        [DataMember(20)]
        [DefaultValue(null)]
        public string Name { get; set; }

        [DataMember(30)]
        [DefaultValue(null)]
        public Type Type { get; set; }

        [DataMember(40)]
        [DefaultValue(null)]
        public object Value { get; set; }


        [DataMember(50)]
        [DefaultValue(SlotFlags.None)]

        public SlotFlags Flags { get; set; }
    }
}