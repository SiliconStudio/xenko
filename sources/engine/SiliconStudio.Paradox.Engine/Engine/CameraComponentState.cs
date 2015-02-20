using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics.Composers;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// State of a <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("CameraComponentState")]
    public class CameraComponentState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraComponentState"/> class.
        /// </summary>
        /// <param name="cameraComponent">The camera component.</param>
        /// <exception cref="System.ArgumentNullException">cameraComponent</exception>
        public CameraComponentState(CameraComponent cameraComponent)
        {
            if (cameraComponent == null) throw new ArgumentNullException("cameraComponent");
            CameraComponent = cameraComponent;
        }

        /// <summary>
        /// Gets the camera component.
        /// </summary>
        /// <value>The camera component.</value>
        public CameraComponent CameraComponent { get; private set; }

        /// <summary>
        /// The view matrix read-only.
        /// </summary>
        public Matrix View;

        /// <summary>
        /// The projection matrix read-only.
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// Updates the view and projection matrix.
        /// </summary>
        internal void Update()
        {
            CameraComponent.Calculate(out Projection, out View);
        }
    }

    public static class RenderContextCameraExtensions
    {
        /// <summary>
        /// Property key to access the current <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        private static readonly PropertyKey<Dictionary<CameraComponent, CameraComponentState>> States = new PropertyKey<Dictionary<CameraComponent, CameraComponentState>>("CameraComponentStates", typeof(Dictionary<CameraComponent, CameraComponentState>));

        internal static void ClearCameraStates(this RenderContext context)
        {
            GetAllCameras(context).Clear();
        }

        internal static Dictionary<CameraComponent, CameraComponentState> GetAllCameras(this RenderContext context)
        {
            var states = context.Tags.Get(States);
            if (states == null)
            {
                states = new Dictionary<CameraComponent, CameraComponentState>();
                context.Tags.Set(States, states);
            }
            return states;
        }
        
        public static CameraComponentState GetCameraState(this RenderContext context, SceneCameraSlotIndex cameraSlotIndex)
        {
            var cameraCollection = SceneCameraSlotCollection.GetCurrent(context);
            if (cameraCollection == null)
            {
                return null;
            }

            // If no camera found, just skip this part.
            var camera = cameraCollection.GetCamera(cameraSlotIndex);
            if (camera == null)
            {
                return null;
            }

            var allCameras = context.GetAllCameras();
            CameraComponentState state;
            allCameras.TryGetValue(camera, out state);
            return state;
        }
    }
}