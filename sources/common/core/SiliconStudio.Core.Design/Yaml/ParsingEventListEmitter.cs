// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Yaml
{
    public class ParsingEventListEmitter : IEmitter
    {
        private readonly List<ParsingEvent> parsingEvents;

        public ParsingEventListEmitter(List<ParsingEvent> parsingEvents)
        {
            this.parsingEvents = parsingEvents;
        }

        public void Emit(ParsingEvent @event)
        {
            // Ignore some events
            if (@event is StreamStart || @event is StreamEnd
                || @event is DocumentStart || @event is DocumentEnd)
                return;

            parsingEvents.Add(@event);
        }
    }
}
