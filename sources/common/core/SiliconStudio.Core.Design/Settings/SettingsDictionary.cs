using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal dictionary class used to serialize a <see cref="SettingsProfile"/>.
    /// </summary>
    internal class SettingsDictionary : Dictionary<UFile, List<ParsingEvent>>
    {
        // Used for temporary internal storage
        [DataMemberIgnore]
        internal SettingsProfile Profile;
    }
}