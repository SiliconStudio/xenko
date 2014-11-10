namespace SiliconStudio.Paradox.Physics
{
    /// <summary>
    /// Data type for <see cref="PhysicsElement"/>.
    /// </summary>
    [Core.DataContract("PhysicsElementData")]
    public class PhysicsElementData
    {
        /// <summary>
        /// Data field for <see cref="PhysicsElement.Type"/>.
        /// </summary>
        public PhysicsElement.Types Type;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.LinkedBoneName"/>.
        /// </summary>
        public System.String LinkedBoneName;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.Shape"/>.
        /// </summary>
        public Core.Serialization.ContentReference<PhysicsColliderShapeData> Shape;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.CollisionGroup"/>.
        /// </summary>
        public PhysicsElement.CollisionFilterGroups1 CollisionGroup;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.CanCollideWith"/>.
        /// </summary>
        public CollisionFilterGroups CanCollideWith;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.StepHeight"/>.
        /// </summary>
        public System.Single StepHeight;

        /// <summary>
        /// Data field for <see cref="PhysicsElement.Sprite"/>.
        /// </summary>
        public System.Boolean Sprite;
    }
}