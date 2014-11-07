using System.Collections.Generic;

namespace SiliconStudio.Paradox.Physics
{
    /// <summary>
    /// Data type for <see cref="PhysicsComponent"/>.
    /// </summary>
    [Core.DataContract("PhysicsComponentData")]
    public class PhysicsComponentData : EntityModel.Data.EntityComponentData
    {
        /// <summary>
        /// Data field for <see cref="PhysicsComponent.Elements"/>.
        /// </summary>
        public List<PhysicsElementData> Elements = new List<PhysicsElementData>();
    }
}