// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Used by editor as top level compositor.
    /// </summary>
    public partial class EditorTopLevelCompositor : SceneCameraRenderer, ISharedRenderer
    {
        public CameraComponent EditorCamera { get; set; }

        public List<ISceneRenderer> PreGizmoCompositors { get; } = new List<ISceneRenderer>();

        public List<ISceneRenderer> PostGizmoCompositors { get; } = new List<ISceneRenderer>();

        protected override void CollectInner(RenderContext context)
        {
            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Collect(context);

            base.CollectInner(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Collect(context);
        }

        protected override void DrawInner(RenderDrawContext context)
        {
            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Draw(context);

            base.DrawInner(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Draw(context);
        }

        protected override CameraComponent ResolveCamera(RenderContext renderContext)
        {
            return EditorCamera;
        }
    }
}