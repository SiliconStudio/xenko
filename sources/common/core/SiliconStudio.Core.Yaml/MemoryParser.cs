using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Yaml
{
    public class MemoryParser : IParser
    {
        private readonly IEnumerator<ParsingEvent> parsingEvents;

        public MemoryParser(IEnumerable<ParsingEvent> parsingEvents)
        {
            this.parsingEvents = parsingEvents.GetEnumerator();
        }

        public ParsingEvent Current { get { return parsingEvents.Current; } }

        public bool MoveNext()
        {
            return parsingEvents.MoveNext();
        }
    }
}