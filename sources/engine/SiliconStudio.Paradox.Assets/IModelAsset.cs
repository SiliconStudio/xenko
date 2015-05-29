using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// This interface represents an asset containing a model.
    /// </summary>
    public interface IModelAsset
    {
        /// <summary>
        /// Gets the collection of material instances associated with the model.
        /// </summary>
        [Browsable(false)]
        IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get; }
    }
}