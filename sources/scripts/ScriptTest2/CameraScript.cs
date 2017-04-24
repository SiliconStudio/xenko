// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Configuration;
using SiliconStudio.Xenko.Input;
using Keys = SiliconStudio.Xenko.Input.Keys;

#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ScriptTest2
{
    [XenkoScript(ScriptFlags.AssemblyStartup)]
    public class CameraScript : ScriptContext
    {
        private Camera camera;
        bool isModifyingPosition = false;
        bool isModifyingFov = false;
        Vector3 fixedPosition = Vector3.Zero;
        Matrix fixedFreeMatrix = Matrix.Identity;
        bool viewChanged = true;

        public CameraComponent TrackingCamera;

        private Stopwatch clock = new Stopwatch();
        private bool flyingAround;
        float pitch = 0;
        float yaw = 0;
        float roll = 0;
        float previousX = 0;
        float previousY = 0;

        private Vector3 localVelocity;

        public CameraScript(IServiceRegistry registry) : base(registry)
        {
            Speed = 10.0f;
            MoveSpeed = 2.0f;
        }

        public float Speed { get; set; }
        public float MoveSpeed { get; set; }

        public class Config
        {
            public Config()
            {
                FlyingAround = false;
                Speed = 5.0f;
            }

            [XmlAttribute("flyingAround")]
            public bool FlyingAround { get; set; }

            [XmlAttribute("speed")]
            public float Speed { get; set; }
        }

        public async Task AutoswitchCamera(EngineContext engineContext)
        {
            bool autoswitch = false;
            while (true)
            {
                // Fetch camera list
                var cameras = Context.EntityManager.Entities
                    .Where(x => x.ContainsKey(CameraComponent.Key))
                    .Select(x => x.Get(CameraComponent.Key)).ToArray();

                if (engineContext.InputManager.IsKeyPressed(Keys.F8))
                {
                    autoswitch = !autoswitch;
                }

                int index = Array.IndexOf(cameras, TrackingCamera);
                if (autoswitch)
                {
                    if (index == 1)
                        index = 2;
                    else
                        index = 1;
                    TrackingCamera = (index < cameras.Length) ? cameras[index] : null;
                }

                await TaskEx.Delay((index == 1) ? 50000 : 10000);
            }
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                camera = value;
            }
        }

        public override async Task Execute()
        {
            var config = AppConfig.GetConfiguration<Config>("CameraScript");
            flyingAround = config.FlyingAround;
            
            //uiControl = Context.RenderContext.UIControl;

            if (camera == null)
            {
                // Near plane and Far plane are swapped in order to increase float precision for far distance as near distance is already having 
                // lots of precisions by the 1/z perspective projection
                //camera = new Camera(new R32G32B32_Float(200, 0, 200), new R32G32B32_Float(0, 0, 200), 1.2f, 1280.0f / 720.0f, 4000000.0f, 10.0f);
                var zNear = 10.0f;
                var zFar = 4000000.0f;
                if (Context.RenderContext.IsZReverse)
                {
                    var temp = zNear;
                    zNear = zFar;
                    zFar = temp;
                }
                camera = new Camera(new Vector3(200, 0, 200), new Vector3(0, 0, 200), 1.2f, RenderContext.Width, RenderContext.Height, (float)RenderContext.Width / RenderContext.Height, zNear, zFar);
                camera.Mode = CameraMode.Free;
            }

            Speed = config.Speed;

            //useful when we need to debug something from a specific point of view (we can first get the WorldToCamera value using a breakpoint then set it below)
            /*Matrix mat = new Matrix();
            mat.M11 = 0.0267117824f;
            mat.M12 = -0.9257705f;
            mat.M13 = -0.377141267f;
            mat.M14 = 0.0f;
            mat.M21 = -0.9996432f;
            mat.M22 = -0.024737807f;
            mat.M23 = -0.0100777093f;
            mat.M24 = 0.0f;
            mat.M31 = 0.0f;
            mat.M32 = 0.377275884f;
            mat.M33 = -0.926100969f;
            mat.M34 = 0.0f;
            mat.M41 = 531.524963f;
            mat.M42 = 501.754761f;
            mat.M43 = 989.462646f;
            mat.M44 = 1.0f;
            camera.WorldToCamera = mat;*/

            //uiControl.MouseWheel += OnUiControlOnMouseWheel;
            var st = new Stopwatch();

            st.Start();

            var viewParameters = Context.RenderContext.RenderPassPlugins.OfType<MainPlugin>().FirstOrDefault().ViewParameters;

            Context.Scheduler.Add(() => AutoswitchCamera(Context));

            var lastTime = DateTime.UtcNow;
            var pauseTime = false;
            while (true)
            {
                await Scheduler.Current.NextFrame();

                var mousePosition = Context.InputManager.MousePosition;

                if (Context.InputManager.IsMouseButtonPressed(MouseButton.Right))
                {
                    camera.Mode = CameraMode.Free;
                    isModifyingPosition = true;

                    pitch = 0;
                    yaw = 0;
                    roll = 0;

                    if (camera.Mode == CameraMode.Free)
                    {
                        fixedFreeMatrix = camera.WorldToCamera;
                    }
                    else
                    {
                        fixedPosition = camera.Position;
                    }
                }
                if (Context.InputManager.IsMouseButtonDown(MouseButton.Right))
                {
                    var deltaX = mousePosition.X - previousX;
                    var deltaY = mousePosition.Y - previousY;
                    if (isModifyingPosition)
                    {

                        yaw += deltaX * Speed / 1000.0f;
                        pitch += deltaY * Speed / 1000.0f;

                        if (camera.Mode == CameraMode.Target)
                        {
                            camera.Position = (Vector3)Vector3.Transform(fixedPosition, Matrix.RotationX(pitch) * Matrix.RotationZ(yaw));
                            Console.WriteLine(camera.Position);
                        }
                        else
                        {
                            camera.WorldToCamera = Camera.YawPitchRoll(camera.Position, fixedFreeMatrix, yaw, -pitch, roll);
                        }

                        //uiControl.Text = "" + camera.Mode;

                        viewChanged = true;
                    }
                    else if (isModifyingFov)
                    {
                        camera.FieldOfView += deltaX / 128.0f;
                        camera.FieldOfView = Math.Max(Math.Min(camera.FieldOfView, (float)Math.PI * 0.9f), 0.01f);

                        viewChanged = true;
                    }
                }
                if (Context.InputManager.IsMouseButtonReleased(MouseButton.Right))
                {
                    isModifyingFov = false;
                    isModifyingPosition = false;

                    if (camera.Mode == CameraMode.Free && flyingAround)
                    {
                        camera.Mode = CameraMode.Target;
                    }
                }

                previousX = mousePosition.X;
                previousY = mousePosition.Y;

                if (Context.InputManager.IsKeyDown(Keys.LeftAlt) && Context.InputManager.IsKeyPressed(Keys.Enter))
                    Context.RenderContext.GraphicsDevice.IsFullScreen = !Context.RenderContext.GraphicsDevice.IsFullScreen;

                if (Context.InputManager.IsKeyPressed(Keys.F2))
                    pauseTime = !pauseTime;

                var currentTime = DateTime.UtcNow;
                if (!pauseTime)
                {
                    Context.CurrentTime += currentTime - lastTime;
                    viewParameters.Set(GlobalKeys.Time, (float)Context.CurrentTime.TotalSeconds);
                    var timeStep = (float)(currentTime - lastTime).TotalMilliseconds;
                    viewParameters.Set(GlobalKeys.TimeStep, timeStep);
                }

                // Set the pause variable for the rendering
                Context.RenderContext.IsPaused = pauseTime;
                lastTime = currentTime;
               
                if (Context.InputManager.IsKeyPressed(Keys.Space))
                {
                    // Fetch camera list
                    var cameras = Context.EntityManager.Entities
                        .Where(x => x.ContainsKey(CameraComponent.Key))
                        .Select(x => x.Get(CameraComponent.Key)).ToArray();

                    // Go to next camera
                    // If no camera or unset, index will be 0, so it will go to first one
                    int index = Array.IndexOf(cameras, TrackingCamera) + 1;
                    TrackingCamera = (index < cameras.Length) ? cameras[index] : null;
                    clock.Restart();

                    //flyingAround = !flyingAround;
                    //if (flyingAround)
                    //{
                    //    fixedPosition = camera.Position;
                    //    clock.Restart();
                    //}
                }

                if (TrackingCamera != null)
                {
                    Matrix projection, worldToCamera;

                    // Overwrite near/far plane with our camera, as they are not reliable
                    TrackingCamera.NearPlane = camera.NearClipPlane;
                    TrackingCamera.FarPlane = camera.FarClipPlane;

                    TrackingCamera.Calculate(out projection, out worldToCamera);

                    viewParameters.Set(TransformationKeys.View, worldToCamera);
                    viewParameters.Set(TransformationKeys.Projection, projection);

                    // TODO: tracking camera doesn't have proper near/far plane values. Use mouse camera near/far instead.
                    viewParameters.Set(CameraKeys.NearClipPlane, camera.NearClipPlane);
                    viewParameters.Set(CameraKeys.FarClipPlane, camera.FarClipPlane);
                    viewParameters.Set(CameraKeys.FieldOfView, TrackingCamera.VerticalFieldOfView);
                    // Console.WriteLine("FOV:{0}", trackingCamera.VerticalFieldOfView);
                    viewParameters.Set(CameraKeys.ViewSize, new Vector2(camera.Width, camera.Height));
                    viewParameters.Set(CameraKeys.Aspect, TrackingCamera.AspectRatio);
                    viewParameters.Set(CameraKeys.FocusDistance, TrackingCamera.FocusDistance);
                }
                else
                {
                    bool moved = false;

                    var localPosition = new Vector3(0, 0, 0);

                    if (Context.InputManager.IsKeyDown(Keys.Left) || Context.InputManager.IsKeyDown(Keys.A))
                    {
                        localPosition.X -= 1.0f;
                        moved = true;
                    }

                    if (Context.InputManager.IsKeyDown(Keys.Right) || Context.InputManager.IsKeyDown(Keys.D))
                    {
                        localPosition.X += 1.0f;
                        moved = true;
                    }

                    if (Context.InputManager.IsKeyDown(Keys.Up) || Context.InputManager.IsKeyDown(Keys.W))
                    {
                        localPosition.Y -= 1.0f;
                        moved = true;
                    }
                    if (Context.InputManager.IsKeyDown(Keys.Down) || Context.InputManager.IsKeyDown(Keys.S))
                    {
                        localPosition.Y += 1.0f;
                        moved = true;
                    }

                    if (Context.InputManager.IsKeyDown(Keys.R))
                    {
                        roll += 0.1f;
                    }

                    if (Context.InputManager.IsKeyDown(Keys.T))
                    {
                        roll -= 0.1f;
                    }

                    var moveSpeedFactor = Context.InputManager.IsKeyDown(Keys.LeftShift) || Context.InputManager.IsKeyDown(Keys.RightShift) ? 0.1f : 1.0f;

                    localPosition.Normalize();
                    localPosition *= (float)clock.ElapsedTicks * 1000.0f * MoveSpeed * moveSpeedFactor / Stopwatch.Frequency;

                    localVelocity = localVelocity * 0.9f + localPosition * 0.1f;

                    if (localVelocity.Length() > MoveSpeed * 0.001f)
                    {

                        if (camera.Mode == CameraMode.Target)
                            camera.Position = camera.Position + localVelocity;
                        else
                        {
                            var destVector = ((Vector3)camera.WorldToCamera.Column3);
                            var leftRightVector = Vector3.Cross(destVector, Vector3.UnitZ);
                            leftRightVector.Normalize();

                            var newPosition = camera.Position - destVector * localVelocity.Y;
                            newPosition = newPosition - leftRightVector * localVelocity.X;
                            camera.Position = newPosition;
                        }

                        viewChanged = true;
                    }

                    if (flyingAround)
                    {
                        camera.Position = (Vector3)Vector3.Transform(camera.Position, Matrix.RotationZ(clock.ElapsedMilliseconds * Speed / 10000.0f));
                        viewChanged = true;
                    }

                    if (Context.InputManager.IsKeyPressed(Keys.F))
                    {
                        camera.FieldOfView -= 0.1f;
                        viewChanged = true;
                    }

                    if (Context.InputManager.IsKeyPressed(Keys.G))
                    {
                        camera.FieldOfView += 0.1f;
                        viewChanged = true;
                    }

                    //if (viewChanged)
                    {
                        viewParameters.Set(TransformationKeys.View, camera.WorldToCamera);
                        viewParameters.Set(TransformationKeys.Projection, camera.Projection);
                        viewParameters.Set(CameraKeys.NearClipPlane, camera.NearClipPlane);
                        viewParameters.Set(CameraKeys.FarClipPlane, camera.FarClipPlane);
                        viewParameters.Set(CameraKeys.FieldOfView, camera.FieldOfView);
                        //Console.WriteLine("FOV:{0}", camera.FieldOfView);
                        viewParameters.Set(CameraKeys.ViewSize, new Vector2(camera.Width, camera.Height));
                        viewParameters.Set(CameraKeys.Aspect, camera.Aspect);
                        viewParameters.Set(CameraKeys.FocusDistance, 0.0f);
                        //Console.WriteLine("Camera: {0}", camera);

                    }

                    if (Context.InputManager.IsKeyPressed(Keys.I))
                    {
                        var worldToCamera = camera.WorldToCamera;
                        var target = (Vector3)worldToCamera.Column3;
                        Console.WriteLine("camera.Position = new R32G32B32_Float({0}f,{1}f,{2}f); camera.Target = camera.Position + new R32G32B32_Float({3}f, {4}f, {5}f);", camera.Position.X, camera.Position.Y, camera.Position.Z, target.X, target.Y, target.Z);
                    }

                    viewChanged = false;
                    clock.Restart();
                }
            }
        }
    }
}
