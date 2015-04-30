// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Rendering.Gizmos
{
    /// <summary>
    /// List the names of the gizmo entity factories
    /// </summary>
    public static class GizmoEntityNames
    {
        // TODO: This will removed and replaced by a the name of the plugin that matches the assets of the assembly.
        public const string SharedAssemblyQualifiedName = "Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
        public const string PresentationAssemblyName = "SiliconStudio.Paradox.Assets.Presentation";

        public const string PresentationAssemblyQualifiedName = ", " + PresentationAssemblyName + ", " + SharedAssemblyQualifiedName;

        public const string GizmoEntityNamespace = "SiliconStudio.Paradox.Assets.Presentation.SceneEditor.Gizmos.";

        public const string CameraGizmoEntityQualifiedName = GizmoEntityNamespace + "CameraGizmo" + PresentationAssemblyQualifiedName;
        public const string LightGizmoEntityQualifiedName = GizmoEntityNamespace + "DispatcherLightGizmo" + PresentationAssemblyQualifiedName;
        public const string PhysicsGizmoEntityQualifiedName = GizmoEntityNamespace + "PhysicsGizmo" + PresentationAssemblyQualifiedName;
    }
}