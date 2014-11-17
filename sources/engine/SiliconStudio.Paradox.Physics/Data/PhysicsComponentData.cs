using System.Collections.Generic;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Physics
{
    /// <summary>
    /// Data type for <see cref="PhysicsComponent"/>.
    /// </summary>
    [Core.DataContract("PhysicsComponentData")]
    public class PhysicsComponentData : EntityComponentData
    {
        /// <summary>
        /// Data field for <see cref="PhysicsComponent.Elements"/>.
        /// </summary>
        public List<PhysicsElementData> Elements = new List<PhysicsElementData>();
    }
}