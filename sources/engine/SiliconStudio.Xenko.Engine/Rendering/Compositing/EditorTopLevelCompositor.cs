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

        /// <summary>
        /// When true, <see cref="PreviewGame"/> will be used as compositor.
        /// </summary>
        public bool EnablePreviewGame { get; set; }

        /// <summary>
        /// Compositor for previewing game, used when <see cref="EnablePreviewGame"/> is true.
        /// </summary>
        public ISceneRenderer PreviewGame { get; set; }

        public List<ISceneRenderer> PreGizmoCompositors { get; } = new List<ISceneRenderer>();

        public List<ISceneRenderer> PostGizmoCompositors { get; } = new List<ISceneRenderer>();

        protected override void CollectInner(RenderContext context)
        {
            if (EnablePreviewGame)
            {
                // Defer to PreviewGame
                PreviewGame?.Collect(context);
            }
            else
            {
                foreach (var gizmoCompositor in PreGizmoCompositors)
                    gizmoCompositor.Collect(context);

                base.CollectInner(context);

                foreach (var gizmoCompositor in PostGizmoCompositors)
                    gizmoCompositor.Collect(context);
            }
        }

        protected override void DrawInner(RenderDrawContext context)
        {
            if (EnablePreviewGame)
            {
                PreviewGame?.Draw(context);
            }
            else
            {
                foreach (var gizmoCompositor in PreGizmoCompositors)
                    gizmoCompositor.Draw(context);

                base.DrawInner(context);

                foreach (var gizmoCompositor in PostGizmoCompositors)
                    gizmoCompositor.Draw(context);
            }
        }

        protected override CameraComponent ResolveCamera(RenderContext renderContext)
        {
            return EditorCamera;
        }
    }
}