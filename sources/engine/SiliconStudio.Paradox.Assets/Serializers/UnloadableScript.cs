using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Serializers
{
    /// <summary>
    /// Represents a Script that could not be loaded properly (usually due to missing/broken assemblies).
    /// Yaml representation is kept as is, so that it can be properly saved again.
    /// </summary>
    class UnloadableScript : Script
    {
        [DataMemberIgnore]
        public List<ParsingEvent> ParsingEvents { get; private set; }

        public UnloadableScript(List<ParsingEvent> parsingEvents)
        {
            ParsingEvents = parsingEvents;
        }
    }
}