using System.Collections.Generic;
using SharpYaml.Events;

using SiliconStudio.Core;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Assets.Serializers
{
    /// <summary>
    /// Represents a Script that could not be loaded properly (usually due to missing/broken assemblies).
    /// Yaml representation is kept as is, so that it can be properly saved again.
    /// </summary>
    [DataSerializerGlobal(typeof(InvariantObjectCloneSerializer<UnloadableScript>), Profile = "AssetClone")]
    [Display("Error: unable to load this script")]
    class UnloadableScript : Script
    {
        [DataMemberIgnore]
        public List<ParsingEvent> ParsingEvents { get; private set; }

        public UnloadableScript()
        {
        }

        public UnloadableScript(List<ParsingEvent> parsingEvents)
        {
            ParsingEvents = parsingEvents;
        }
    }
}