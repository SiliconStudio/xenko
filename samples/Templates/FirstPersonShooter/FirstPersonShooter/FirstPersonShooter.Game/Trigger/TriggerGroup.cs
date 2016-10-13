using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Materials;

namespace FirstPersonShooter.Trigger
{
    [DataContract("TriggerGroup")]
    public class TriggerGroup
    {
        [DataMember(10)]
        [Display("Name")]
        public string Name { get; set; } = "NewTriggerGroup";

        [DataMember(20)]
        [Display("Events")]
        public List<TriggerEvent> TriggerEvents { get; } = new List<TriggerEvent>();

        public TriggerEvent Find(string name) => Find(x => x.Name.Equals(name));

        public List<TriggerEvent> FindAll(Predicate<TriggerEvent> match)
        {
            //TriggerEvents.

            return TriggerEvents.FindAll(match);
        }

        public TriggerEvent Find(Predicate<TriggerEvent> match)
        {
            return TriggerEvents.Find(match);
        }
    }
}
