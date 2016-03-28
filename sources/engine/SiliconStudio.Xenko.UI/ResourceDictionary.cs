using System.Collections.Generic;

namespace SiliconStudio.Xenko.UI
{
    // Note: we probably don't want to mimic WPF if we make this API public,
    // just did it so that it is easier to understand what current
    // implementation does when we rewrite new one.
    /// <summary>
    /// Resources dictionary, containing styles, etc...
    /// </summary>
    internal class ResourceDictionary : Dictionary<object, object>
    {
    }
}