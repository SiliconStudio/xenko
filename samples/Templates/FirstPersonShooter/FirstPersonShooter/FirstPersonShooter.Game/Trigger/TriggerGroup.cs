// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;

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
            return TriggerEvents.FindAll(match);
        }

        public TriggerEvent Find(Predicate<TriggerEvent> match)
        {
            return TriggerEvents.Find(match);
        }
    }
}
