using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A collection of <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("CameraComponentCollection")]
    public sealed class CameraComponentCollection : List<CameraComponent>
    {
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<CameraComponentCollection> Current = new PropertyKey<CameraComponentCollection>("CameraComponentCollection.Current", typeof(CameraComponentCollection));

        /// <summary>
        /// Gets the camera for the specified slot or null if empty
        /// </summary>
        /// <param name="cameraSlot">The camera slot.</param>
        /// <returns>SiliconStudio.Paradox.Engine.CameraComponent.</returns>
        public CameraComponent GetCamera(SceneCameraSlot cameraSlot)
        {
            int index = cameraSlot;
            if (index >= 0 && index < Count)
            {
                return this[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the current camera collection setup in the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>CameraComponentCollection.</returns>
        public static CameraComponentCollection GetCurrent(RenderContext context)
        {
            return context.Tags.Get(Current);
        }
    }
}