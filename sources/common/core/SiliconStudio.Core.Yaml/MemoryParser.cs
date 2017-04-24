// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Yaml
{
    public class MemoryParser : IParser
    {
        private readonly IList<ParsingEvent> parsingEvents;
        private int position = -1;
        private ParsingEvent current;

        public MemoryParser(IList<ParsingEvent> parsingEvents)
        {
            this.parsingEvents = parsingEvents;
        }

        public IList<ParsingEvent> ParsingEvents => parsingEvents;

        /// <inheritdoc/>
        public ParsingEvent Current => current;

        /// <inheritdoc/>
        public bool IsEndOfStream => position >= parsingEvents.Count;

        public int Position
        {
            get { return position; }
            set
            {
                position = value;
                current = (position >= 0) ? parsingEvents[position] : null;
            }
        }

        public bool MoveNext()
        {
            if (++position < parsingEvents.Count)
            {
                current = parsingEvents[position];
                return true;
            }

            return false;
        }
    }
}
