// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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