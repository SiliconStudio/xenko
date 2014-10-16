// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// A processor that updates camera view and projection along the setup of <see cref="RenderTargetSetter"/>
    /// </summary>
    public class CameraSetter : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public CameraSetter(IServiceRegistry services) : base(services)
        {
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        public CameraComponent Camera { get; set; }

        public override void Load()
        {
            // Clear scene
            Pass.StartPass += OnRender;
        }

        public override void Unload()
        {
            Pass.StartPass -= OnRender;
        }
        
        protected void OnRender(RenderContext context)
        {
            var pass = context.CurrentPass;

            if (Camera != null && Camera.Entity != null)
            {
                var viewParameters = pass.Parameters;

                Matrix projection;
                Matrix worldToCamera;
                Camera.Calculate(out projection, out worldToCamera);

                viewParameters.Set(TransformationKeys.View, worldToCamera);
                viewParameters.Set(TransformationKeys.Projection, projection);
                viewParameters.Set(CameraKeys.NearClipPlane, Camera.NearPlane);
                viewParameters.Set(CameraKeys.FarClipPlane, Camera.FarPlane);
                viewParameters.Set(CameraKeys.FieldOfView, Camera.VerticalFieldOfView);
                viewParameters.Set(CameraKeys.Aspect, Camera.AspectRatio);
                viewParameters.Set(CameraKeys.FocusDistance, Camera.FocusDistance);

                // TODO: move the following code in a more suitable place
                //viewParameters.Set(GlobalKeys.Time, (float)game.PlayTime.TotalTime.TotalSeconds);
                //viewParameters.Set(GlobalKeys.TimeStep, (float)game.PlayTime.ElapsedTime.TotalSeconds);            
            }
        }
    }
}