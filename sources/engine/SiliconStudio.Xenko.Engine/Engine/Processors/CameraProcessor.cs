// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor for <see cref="CameraComponent"/>.
    /// </summary>
    public class CameraProcessor : EntityProcessor<CameraComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProcessor"/> class.
        /// </summary>
        public CameraProcessor()
        {
            Order = -10;
        }

        protected override CameraComponent GenerateComponentData(Entity entity, CameraComponent component)
        {
            return component;
        }

        public override void Draw(RenderContext context)
        {
            var graphicsCompositor = Services.GetServiceAs<SceneSystem>()?.GraphicsCompositor;

            // First pass, handle proper detach
            foreach (var matchingCamera in ComponentDatas)
            {
                var camera = matchingCamera.Value;
                if (graphicsCompositor != null)
                {

                    if (camera.Slot.AttachedCompositor != null && camera.Slot.AttachedCompositor != graphicsCompositor)
                    {
                        // The graphics compositor has changed. Let's detach the camera from the old one...
                        DetachCameraFromSlot(camera);
                    }
                    else if (camera.Enabled && camera.Slot.AttachedCompositor == null)
                    {
                        // Either the slot has been changed and need to be re-attached, or the camera has just been enabled.
                        // Make sure this camera is detached from all slots, we'll re-attach it in the second pass.
                        DetachCameraFromAllSlots(camera, graphicsCompositor);
                    }
                    else if (!camera.Enabled && camera.Slot.AttachedCompositor == graphicsCompositor)
                    {
                        // The camera has been disabled and need to be detached.
                        DetachCameraFromSlot(camera);
                    }
                }

            }

            // Second pass, handle proper attach
            foreach (var matchingCamera in ComponentDatas)
            {
                var camera = matchingCamera.Value;

                if (graphicsCompositor != null)
                {
                    if (camera.Enabled && camera.Slot.AttachedCompositor == null)
                    {
                        // Attach to the new slot
                        AttachCameraToSlot(camera);
                    }

                }

                // In case the camera has a custom aspect ratio, we can update it here
                // otherwise it is screen-dependent and we can only update it in the CameraComponentRenderer.
                if (camera.Enabled && camera.UseCustomAspectRatio)
                {
                    camera.Update();
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, CameraComponent component, CameraComponent data)
        {
            base.OnEntityComponentAdding(entity, component, data);

            if (component.Enabled)
                AttachCameraToSlot(component);
        }

        protected override void OnEntityComponentRemoved(Entity entity, CameraComponent component, CameraComponent data)
        {
            if (component.Slot.AttachedCompositor != null)
                DetachCameraFromSlot(component);

            base.OnEntityComponentRemoved(entity, component, data);
        }

        private void AttachCameraToSlot(CameraComponent camera)
        {
            if (!camera.Enabled) throw new InvalidOperationException($"The camera [{camera.Entity.Name}] cannot be attached because it is disabled");
            if (camera.Slot.AttachedCompositor != null) throw new InvalidOperationException($"The camera [{camera.Entity.Name}] is already attached");

            var graphicsCompositor = Services.GetServiceAs<SceneSystem>()?.GraphicsCompositor;
            if (graphicsCompositor != null)
            {
                for (var i = 0; i < graphicsCompositor.Cameras.Count; ++i)
                {
                    var slot = graphicsCompositor.Cameras[i];
                    if (slot.Id == camera.Slot.Id)
                    {
                        if (slot.Camera != null)
                            throw new InvalidOperationException($"Unable to attach camera [{camera.Entity.Name}] to the graphics compositor, camera {slot.Camera.Entity.Name} is already attached to this slot.");

                        slot.Camera = camera;
                        break;
                    }
                }
                camera.Slot.AttachedCompositor = graphicsCompositor;
            }
        }

        private static void DetachCameraFromSlot(CameraComponent camera)
        {
            if (camera.Slot.AttachedCompositor == null)
                throw new InvalidOperationException($"The camera [{camera.Entity.Name}] is not attached");

            for (var i = 0; i < camera.Slot.AttachedCompositor.Cameras.Count; ++i)
            {
                var slot = camera.Slot.AttachedCompositor.Cameras[i];
                if (slot.Id == camera.Slot.Id)
                {
                    if (slot.Camera != camera)
                        throw new InvalidOperationException($"Unable to detach camera [{camera.Entity.Name}] from the graphics compositor, another camera {slot.Camera.Entity.Name} is attached to this slot.");

                    slot.Camera = null;
                    break;
                }
            }
            camera.Slot.AttachedCompositor = null;
        }

        private static void DetachCameraFromAllSlots(CameraComponent camera, GraphicsCompositor graphicsCompositor)
        {
            for (var i = 0; i < graphicsCompositor.Cameras.Count; ++i)
            {
                var slot = graphicsCompositor.Cameras[i];
                if (slot.Camera == camera)
                {
                    slot.Camera = null;
                }
            }
            camera.Slot.AttachedCompositor = null;
        }
    }
}