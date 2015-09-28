using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Gives the ability to control how parent matrix is computed in a <see cref="TransformComponent"/>.
    /// </summary>
    public abstract class TransformLink
    {
        /// <summary>
        /// Compute a world matrix this link represents.
        /// </summary>
        /// <param name="recursive"></param>
        /// <param name="matrix">The computed world matrix.</param>
        public abstract void ComputeMatrix(bool recursive, out Matrix matrix);
    }
}